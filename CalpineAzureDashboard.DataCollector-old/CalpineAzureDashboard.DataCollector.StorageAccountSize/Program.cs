using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.Models;
using log4net;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.File;

namespace CalpineAzureDashboard.DataCollector.StorageAccountSize
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
        private static AzureService _azureService;
        private static StorageAccountRepository<StorageAccountModel> _storageAccountRepository;

        static void Main(string[] args)
        {
            try {
                var keyVaultUri = ConfigurationManager.AppSettings["keyVaultUri"];
                var connectionString = GetKeyVaultSecretAsync(keyVaultUri, "connectionString").GetAwaiter().GetResult();
                _storageAccountRepository = new StorageAccountRepository<StorageAccountModel>(connectionString);

                string clientId = GetKeyVaultSecretAsync(keyVaultUri, "clientId").GetAwaiter().GetResult();
                string clientSecret = GetKeyVaultSecretAsync(keyVaultUri, "clientSecret").GetAwaiter().GetResult();
                string tenantId = GetKeyVaultSecretAsync(keyVaultUri, "tenantId").GetAwaiter().GetResult();
                _azureService = new AzureService(clientId, clientSecret, tenantId);
            }
            catch (Exception ex) {
                Log.Error("An exception occurred getting configuration values:", ex);
                Environment.Exit(2);
            }

            try {
                var storageAccounts = _storageAccountRepository.GetCollectionAsync().GetAwaiter().GetResult();

                var tasks = new List<Task>();

                foreach (var storageAccount in storageAccounts) {
                    tasks.Add(ProcessAsync(storageAccount));
                }

                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception ex) {
                Log.Error("An uncaught exception occurred:", ex);
                Environment.Exit(1);
            }
        }

        private static async Task<string> GetKeyVaultSecretAsync(string keyVaultUrl, string secretName)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            var secret = await keyVaultClient.GetSecretAsync($"{keyVaultUrl}/secrets/{secretName}").ConfigureAwait(false);
            return secret.Value;
        }

        private static async Task ProcessAsync(StorageAccountModel originalStorageAccount)
        {
            try {
                var updatedStorageAccount = await GetStorageAccountSizeAsync(originalStorageAccount);
                await UpdateStorageAccountAsync(originalStorageAccount, updatedStorageAccount);
            }
            catch (Exception ex) {
                Log.Error($"An exception occurred processing: {originalStorageAccount.Id}.", ex);
            }
        }

        private static async Task<StorageAccountModel> GetStorageAccountSizeAsync(StorageAccountModel originalStorageAccount)
        {
            var storageAccount = originalStorageAccount.ShallowCopy();
            var azureStorageAccount = await _azureService.GetStorageAccountByIdAsync(storageAccount.SubscriptionId, storageAccount.AzureId);

            try {
                var storageAccountConnectionString =
                    $"DefaultEndpointsProtocol=https;AccountName={storageAccount.Name};AccountKey={azureStorageAccount.GetKeys().First().Value};EndpointSuffix=core.windows.net";

                //get blob size
                try {
                    var blobClient = CloudStorageAccount.Parse(storageAccountConnectionString).CreateCloudBlobClient();
                    var containers = blobClient.ListContainers().ToList();
                    storageAccount.NumberOfBlobContainers = containers.Count;

                    var blobSize = 0m;
                    var numberOfBlobs = 0;
                    var blobTasks = new List<Task<Tuple<long, int>>>();
                    foreach (var container in containers) {
                        try {
                            var blobs = container.ListBlobs();
                            foreach (var item in blobs) {
                                try {
                                    blobTasks.Add(Task.Run(() => GetBlobSize(item)));
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

                    storageAccount.BlobSizeInGb = (int) Math.Ceiling(blobSize / 1024 / 1024 / 1024);
                    storageAccount.NumberOfBlobs = numberOfBlobs;
                }
                catch (Exception ex) {
                    Log.Error($"An exception occurred getting blob size: {storageAccount.Id}.", ex);
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

                        storageAccount.FileShareSizeInGb = shareSize;
                        storageAccount.NumberOfFileShares = shares.Count;
                    }
                }
                catch (Exception ex) {
                    Log.Error($"An exception occurred getting file share size for: {storageAccount.Id}.", ex);
                }
            }
            catch (Exception ex) {
                Log.Error($"An exception occurred getting keys for: {storageAccount.Id}.", ex);
            }

            return storageAccount;
        }

        private static Tuple<long, int> GetBlobSize(IListBlobItem item, DateTime? timeStarted = null)
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
                foreach (var directoryItem in directory.ListBlobs()) {
                    var tuple = GetBlobSize(directoryItem, timeStarted);
                    directorySize += tuple.Item1;
                    numberOfBlobs += tuple.Item2;
                }

                return Tuple.Create(directorySize, numberOfBlobs);
            }

            throw new Exception($"Unsupported blob type found: {type}");
        }

        private static async Task UpdateStorageAccountAsync(StorageAccountModel originalStorageAccount, StorageAccountModel updatedStorageAccount)
        {
            if (originalStorageAccount.IsEqual(updatedStorageAccount)) {
                return;
            }

            try {
                if (await _storageAccountRepository.UpdateAsync(updatedStorageAccount) != 1) {
                    Log.Debug($"Unexpected result! Record was not updated for: {updatedStorageAccount.Id}");
                }
            }
            catch (Exception ex) {
                Log.Error($"An exception occurred updating database record for: {updatedStorageAccount.Id}.", ex);
            }
        }
    }
}
