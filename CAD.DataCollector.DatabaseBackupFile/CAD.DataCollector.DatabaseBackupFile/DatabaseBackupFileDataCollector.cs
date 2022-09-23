using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CAD.DataCollector.DatabaseBackupFile.Models;
using CAD.DataCollector.DatabaseBackupFile.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CAD.DataCollector.DatabaseBackupFile
{
    public class DatabaseBackupFileDataCollector : BackgroundService
    {
        private readonly ILogger<DatabaseBackupFileDataCollector> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly DatabaseBackupFileRepository _databaseBackupFileRepository;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public DatabaseBackupFileDataCollector(ILogger<DatabaseBackupFileDataCollector> logger, IConfiguration configuration, IHostApplicationLifetime hostApplicationLifetime)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionString = configuration["connectionString"];
            _databaseBackupFileRepository = new DatabaseBackupFileRepository(_connectionString);
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try {
                var startTime = DateTime.Now;
                _logger.LogInformation($"Starting DatabaseBackupFileDataCollector run at: {startTime}");

                await _databaseBackupFileRepository.TruncateStagingTable();

                var storageAccountRepository = new DatabaseBackupStorageAccountRepository(_connectionString);
                var storageAccounts = await storageAccountRepository.GetDatabaseBackupStorageAccounts();

                var tenantGroups = storageAccounts.GroupBy(x => x.TenantId);
                var tasks = new List<Task>();
                foreach (var tenantGroup in tenantGroups) {
                    tasks.Add(ProcessTenantGroup(tenantGroup, cancellationToken));
                }

                await Task.WhenAll(tasks);
                await _databaseBackupFileRepository.ProcessStagingTable();
                await _databaseBackupFileRepository.TruncateStagingTable();

                var endTime = DateTime.Now;
                var elapsedTime = (int) (endTime - startTime).TotalMinutes;
                _logger.LogInformation($"DatabaseBackupFileDataCollector run completed in {elapsedTime} minutes.");

                _hostApplicationLifetime.StopApplication();
            }
            catch (Exception ex) {
                _logger.LogCritical(ex, "An unexpected exception occurred. Exiting main thread.");
            }
        }

        private async Task ProcessTenantGroup(IGrouping<string, DatabaseBackupStorageAccountModel> tenantGroups, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();
            var configurationGroups = tenantGroups.GroupBy(x => new {x.TenantId, x.ClientId, x.ClientSecret});
            foreach (var configurationGroup in configurationGroups) {
                string tenantId = configurationGroup.Key.TenantId;
                string clientId = _configuration[configurationGroup.Key.ClientId];
                string clientSecret = _configuration[configurationGroup.Key.ClientSecret];
                var azureService = new AzureService(tenantId, clientId, clientSecret, cancellationToken);
                tasks.Add(ProcessTenantAuthenticationGroup(azureService, configurationGroup, cancellationToken));
            }

            await Task.WhenAll(tasks);
        }

        private async Task ProcessTenantAuthenticationGroup(AzureService azureService, IEnumerable<DatabaseBackupStorageAccountModel> configurationGroup, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();
            foreach (var configuration in configurationGroup) {
                tasks.Add(Task.Run(async () =>
                {
                    try {
                        var storageAccount = await azureService.GetStorageAccountByResourceGroupNameAsync(configuration.SubscriptionId,
                            configuration.ResourceGroup, configuration.Name);
                        if (storageAccount == null) {
                            _logger.LogError($"Failed to get the following storage account details from Azure: {configuration.Name}");
                            return;
                        }
                        try {
                            string storageAccountConnectionString =
                                $"DefaultEndpointsProtocol=https;AccountName={storageAccount.Name};AccountKey={storageAccount.GetKeys().First().Value};EndpointSuffix=core.windows.net";
                            try {
                                var blobServiceClient = new BlobServiceClient(storageAccountConnectionString);
                                var blobContainerResultSegment = blobServiceClient.GetBlobContainersAsync().AsPages();

                                var databaseBackupFiles = new List<DatabaseBackupFileModel>();
                                //enumerate the containers returned for each page
                                await foreach (Page<BlobContainerItem> containerPage in blobContainerResultSegment) {
                                    foreach (BlobContainerItem blobContainerItem in containerPage.Values) {
                                        if (blobContainerItem.Name == "donotdelete" ||
                                            blobContainerItem.Name.StartsWith("Custom", StringComparison.OrdinalIgnoreCase)) {
                                            continue;
                                        }

                                        //get blobs in the container
                                        var blobContainerClient = new BlobContainerClient(storageAccountConnectionString, blobContainerItem.Name);
                                        var blobResultSegment = blobContainerClient.GetBlobsAsync().AsPages();

                                        //enumerate the blobs returned for each page.
                                        await foreach (Page<BlobItem> blobPage in blobResultSegment) {
                                            foreach (BlobItem blobItem in blobPage.Values) {
                                                var databaseBackupFile = GetBlobFiles(blobItem, blobContainerItem.Name);
                                                if (databaseBackupFile != null) {
                                                    databaseBackupFile.TenantId = configuration.TenantId;
                                                    databaseBackupFile.Subscription = configuration.Subscription;
                                                    databaseBackupFile.SubscriptionId = configuration.SubscriptionId;
                                                    databaseBackupFile.ResourceGroup = configuration.ResourceGroup;
                                                    databaseBackupFile.StorageAccountAzureId = storageAccount.Id;
                                                    databaseBackupFile.StorageAccountName = storageAccount.Name;
                                                    databaseBackupFiles.Add(databaseBackupFile);
                                                }
                                            }
                                        }
                                    }
                                }

                                await UpdateDatabaseAsync(databaseBackupFiles);
                            }
                            catch (Exception ex) {
                                _logger.LogError(ex, $"An exception occurred getting blob files for storage account: {configuration.Name}.");
                            }
                        }
                        catch (Exception ex) {
                            _logger.LogError(ex, $"An exception occurred getting keys for storage account: {configuration.Name}.");
                        }
                    }
                    catch (Exception ex) {
                        _logger.LogError(ex, $"An exception occurred getting details for storage account: {configuration.Name}.");
                    }

                    //return storageAccount;
                }, cancellationToken));
            }

            try {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex) {
                _logger.LogError(ex, $"An exception occurred processing storage accounts:");
            }
        }

        private static DatabaseBackupFileModel GetBlobFiles(BlobItem item, string containerName, DateTime? timeStarted = null)
        {
            if (timeStarted == null) {
                timeStarted = DateTime.Now;
            }

            var elapsedTimeInMinutes = DateTime.Now.Subtract(timeStarted.Value).TotalMinutes;
            if (elapsedTimeInMinutes > 1200) {
                throw new Exception($"Took too long getting blob files. {elapsedTimeInMinutes} minutes have elapsed.");
            }

            var blob = item;
            if (!blob.Name.Contains(".bak") && !blob.Name.Contains(".abf")) {
                return null;
            }

            var sizeInBytes = blob.Properties.ContentLength;
            var azureDatabaseBackupFile = new DatabaseBackupFileModel {
                ServerName = containerName.Split('-')[0],
                DatabaseName = ParseDatabaseName(blob.Name),
                BackupFileName = ParseBlobName(blob.Name),
                LastModifiedDateUtc = blob.Properties.LastModified?.UtcDateTime,
                SizeInMb = sizeInBytes.HasValue ? (long) Math.Ceiling((decimal) sizeInBytes.Value / 1024 / 1024) : sizeInBytes
            };

            if (containerName.Contains("-")) {
                var instanceName = containerName.Split('-')[1];
                if (!instanceName.Equals("mssqlserver") && !instanceName.Equals("oracleserver")) {
                    azureDatabaseBackupFile.InstanceName = instanceName;
                }
            }

            return azureDatabaseBackupFile;
        }

        private static string ParseBlobName(string blobName)
        {
            return blobName.Contains("/") ? blobName.Split('/')[1] : blobName;
        }

        private static string ParseDatabaseName(string filename)
        {
            if (filename.Contains("/")) {
                return filename.Split('/')[0];
            }

            var substrings = filename.Split('_');

            var sb = new StringBuilder();
            foreach (var substring in substrings) {
                if (Regex.Match(substring.Split('.')[0], @"(^\d{8}$)").Success) {
                    break;
                }

                if (substring.Equals("backup", StringComparison.OrdinalIgnoreCase)) {
                    break;
                }

                if (substring.Equals("level", StringComparison.OrdinalIgnoreCase)) {
                    break;
                }

                sb.Append(Regex.Match(substring, ".bak").Success ? substring.Split('.')[0] : substring);
                sb.Append("_");
            }

            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }

        private async Task UpdateDatabaseAsync(IEnumerable<DatabaseBackupFileModel> databaseBackupFiles)
        {
            try {
                await _databaseBackupFileRepository.InsertAsync(databaseBackupFiles);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "An exception occurred inserting record for storage account: {storageAccountSize.AzureId}.");
            }
        }
    }
}
