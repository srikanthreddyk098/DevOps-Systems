using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CAD.DataCollector.StorageAccountSize.Models;
using CAD.DataCollector.StorageAccountSize.Repositories;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.File;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace CAD.DataCollector.StorageAccountSize
{
    public class StorageAccountSizeDataCollector : BackgroundService
    {
        private readonly ILogger<StorageAccountSizeDataCollector> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _connectionString;
        private readonly StorageAccountSizeRepository _storageAccountSizeRepository;

        public StorageAccountSizeDataCollector(ILogger<StorageAccountSizeDataCollector> logger, IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = clientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(10);
            _connectionString = configuration["connectionString"];
            _storageAccountSizeRepository = new StorageAccountSizeRepository(_connectionString);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try {
                    var startTime = DateTime.Now;
                    _logger.LogInformation($"Starting StorageAccountSizeDataCollector run at: {startTime}");

                    var apiConfigurationRepository = new ApiConfigurationRepository(_connectionString);
                    var apiConfigurations = await apiConfigurationRepository.GetAllAsync(new {ResourceType = "StorageAccount"});

                    var tenantGroups = apiConfigurations.GroupBy(x => x.TenantId);
                    foreach (var tenantGroup in tenantGroups) {
                        await ProcessTenantGroup(tenantGroup, cancellationToken);
                    }

                    var endTime = DateTime.Now;
                    var elapsedTime = (endTime - startTime).TotalMinutes;
                    _logger.LogInformation($"StorageAccountSizeDataCollector run completed in {elapsedTime} minutes.");
            }
            catch (Exception ex) {
                _logger.LogCritical(ex, "An unexpected exception occurred. Exiting main thread.");
            }
        }

        private async Task ProcessTenantGroup(IGrouping<string, ApiConfigurationModel> tenantGroup, CancellationToken cancellationToken)
        {
            var tenantAuthenticationGroups = tenantGroup.GroupBy(x => new {x.ResourceUrl, x.TenantId, x.ClientId, x.ClientSecret});
            foreach (var tenantAuthenticationGroup in tenantAuthenticationGroups) {
                //can put each group in it's own thread
                string tenantId = tenantAuthenticationGroup.Key.TenantId;
                string resourceUrl = tenantAuthenticationGroup.Key.ResourceUrl;
                string clientId = tenantAuthenticationGroup.Key.ClientId;
                string clientSecret = tenantAuthenticationGroup.Key.ClientSecret;
                await ProcessTenantAuthenticationGroup(tenantId, resourceUrl, _configuration[clientId], _configuration[clientSecret], tenantAuthenticationGroup,
                    cancellationToken);
            }
        }

        private async Task ProcessTenantAuthenticationGroup(string tenantId, string resourceUrl, string clientId, string clientSecret,
            IEnumerable<ApiConfigurationModel> tenantAuthenticationGroups, CancellationToken cancellationToken)
        {
            var azureService = new AzureApiService(tenantId, clientId, clientSecret, resourceUrl, _httpClient, cancellationToken);
            var apiConfigurationRepository = new ApiConfigurationRepository(_connectionString);
            var configurations = await apiConfigurationRepository.GetAllDetailAsync(new {TenantId = tenantId, ResourceType = "StorageAccount"});

            var tasks = new List<Task>();
            foreach (var configuration in configurations) {
                tasks.Add(Task.Run(async () =>
                {
                    try {
                        if (string.IsNullOrEmpty(configuration.Url)) {
                            throw new Exception($"Url for {configuration.ResourceType} is empty or null!");
                        }

                        var storageAccountsJson = await azureService.GetDataAsync(configuration.HttpMethod, configuration.Url);

                        foreach (string storageAccountJson in storageAccountsJson) {
                            if (cancellationToken.IsCancellationRequested) {
                                cancellationToken.ThrowIfCancellationRequested();
                            }

                            var jsonObject = JObject.Parse(storageAccountJson);

                            try {
                                var storageAccount = new StorageAccountModel {
                                    AzureId = jsonObject["id"].ToString(),
                                    TenantId = tenantId,
                                    SubscriptionId = Regex.Match(configuration.Url, @"(?<=subscriptions\/).*?(?=\/)").Value,
                                    Name = jsonObject["name"].ToString(),
                                    PrimaryFileEndpoint = jsonObject["properties"]["primaryEndpoints"]["file"]?.ToString()
                                };

                                var storageAccountSize = await GetStorageAccountSizeAsync(azureService, storageAccount, cancellationToken);
                                storageAccountSize.TenantId = tenantId;
                                storageAccountSize.AzureId = storageAccount.AzureId;
                                storageAccountSize.DateCaptured = DateTime.Now.Date;
                                await UpdateDatabaseAsync(storageAccountSize);
                            }
                            catch (Exception ex) {
                                _logger.LogError(ex, $"An exception occurred processing: {jsonObject["id"]}.");
                            }
                        }
                    }
                    catch (Exception ex) {
                        throw new Exception($"An exception occurred calling API with url: {configuration.Url}", ex);
                    }
                }, cancellationToken));
            }

            try {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex) {
                _logger.LogError(ex, $"An exception occurred processing storage account sizes :");
            }
        }

        private async Task<StorageAccountSizeModel> GetStorageAccountSizeAsync(AzureApiService azureService, StorageAccountModel storageAccount,
            CancellationToken cancellationToken)
        {
            var storageAccountSize = new StorageAccountSizeModel();

            try {
                var url = $"https://management.azure.com{storageAccount.AzureId}/listKeys?api-version=2019-06-01";
                var azureStorageAccountKeys = await azureService.GetDataAsync("Post", url, "keys");
                var key = JObject.Parse(azureStorageAccountKeys.FirstOrDefault())["value"];

                var storageAccountConnectionString =
                    $"DefaultEndpointsProtocol=https;AccountName={storageAccount.Name};AccountKey={key};EndpointSuffix=core.windows.net";

                //get blob size
                try {
                    var blobClient = CloudStorageAccount.Parse(storageAccountConnectionString).CreateCloudBlobClient();
                    var containers = blobClient.ListContainers().ToList();
                    storageAccountSize.NumberOfBlobContainers = containers.Count;

                    var blobSize = 0m;
                    var numberOfBlobs = 0;
                    var blobTasks = new List<Task<Tuple<long, int>>>();
                    foreach (var container in containers) {
                        try {
                            var blobs = container.ListBlobs();
                            foreach (var item in blobs) {
                                try {
                                    blobTasks.Add(Task.Run(() => GetBlobSize(item), cancellationToken));
                                }
                                catch (Exception ex) {
                                    throw new Exception($"An exception occurred getting blob size for blob: {item.Uri}.", ex);
                                }
                            }
                        }
                        catch (Exception ex) {
                            throw new Exception($"An exception occurred getting blobs for container: {container.Uri}.", ex);
                        }
                    }

                    var results = await Task.WhenAll(blobTasks);
                    foreach (var result in results) {
                        blobSize += result.Item1;
                        numberOfBlobs += result.Item2;
                    }

                    storageAccountSize.BlobSizeInGb = (int) Math.Ceiling(blobSize / 1024 / 1024 / 1024);
                    storageAccountSize.NumberOfBlobs = numberOfBlobs;
                }
                catch (Exception ex) {
                    _logger.LogError(ex, $"An exception occurred getting blob size: {storageAccount.AzureId}.");
                }

                try {
                    //get file share size
                    if (storageAccount.PrimaryFileEndpoint != null) {
                        var fileClient = CloudStorageAccount.Parse(storageAccountConnectionString).CreateCloudFileClient();
                        var shares = fileClient.ListShares().ToList();
                        int? shareSize = null;
                        foreach (var share in shares) {
                            if (shareSize == null) {
                                shareSize = share?.Properties?.Quota;
                            }
                            else {
                                shareSize += share?.Properties?.Quota ?? 0;
                            }
                        }

                        storageAccountSize.NumberOfFileShares = shares.Count;
                        storageAccountSize.FileShareSizeInGb = shareSize;
                    }
                }
                catch (Exception ex) {
                    _logger.LogError(ex, $"An exception occurred getting file share size for: {storageAccount.AzureId}.");
                }
            }
            catch (Exception ex) {
                _logger.LogError(ex, $"An exception occurred getting keys for: {storageAccount.AzureId}.");
            }

            return storageAccountSize;
        }

        private async Task<Tuple<long, int>> GetBlobSize(IListBlobItem item, DateTime? timeStarted = null)
        {

            if (timeStarted == null) {
                timeStarted = DateTime.Now;
            }

            var elapsedTimeInMinutes = DateTime.Now.Subtract(timeStarted.Value).TotalMinutes;
            if (elapsedTimeInMinutes > 1200) {
                throw new Exception($"Took too long getting blob size. {elapsedTimeInMinutes} minutes have elapsed.");
            }

            var type = item.GetType();
            if (type.IsSubclassOf(typeof(CloudBlob))) {
                var blob = (CloudBlob) item;
                //Log.Debug("CloudBob: " + blob.Uri);
                return Tuple.Create(blob.Properties.Length, 1);
            }

            if (type == typeof(CloudPageBlob)) {
                var blob = (CloudPageBlob) item;
                //Log.Debug("CloudPageBob: " + blob.Uri);
                return Tuple.Create(blob.Properties.Length, 1);
            }

            if (type == typeof(CloudAppendBlob)) {
                var blob = (CloudAppendBlob) item;
                //Log.Debug("CloudAppendBob: " + blob.Uri);
                return Tuple.Create(blob.Properties.Length, 1);
            }

            if (type == typeof(CloudBlobDirectory)) {
                var directory = (CloudBlobDirectory) item;
                //Log.Debug("Directory: " + directory.Uri);
                var directorySize = 0L;
                var numberOfBlobs = 0;
                var tasks = new List<Task<Tuple<long, int>>>();
                foreach (var directoryItem in directory.ListBlobs()) {
                    tasks.Add(Task.Run(() => GetBlobSize(directoryItem, timeStarted)));
                }

                var results = await Task.WhenAll(tasks);
                foreach (var result in results) {
                    directorySize += result.Item1;
                    numberOfBlobs += result.Item2;
                }

                return Tuple.Create(directorySize, numberOfBlobs);
            }

            throw new Exception($"Unsupported blob type found: {type}");
        }

        private async Task UpdateDatabaseAsync(StorageAccountSizeModel storageAccountSize)
        {
            try {
                await _storageAccountSizeRepository.InsertAsync(storageAccountSize);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "An exception occurred inserting record for storage account: {storageAccountSize.AzureId}.");
            }
        }
    }
}
