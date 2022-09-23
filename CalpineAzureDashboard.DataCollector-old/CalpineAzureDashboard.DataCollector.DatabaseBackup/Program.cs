using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.Models;
using log4net;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace CalpineAzureDashboard.DataCollector.DatabaseBackup
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        private static AzureService _azureService;
        private static AzureDatabaseBackupFileRepository<AzureDatabaseBackupFile> _azureDatabaseBackupFileRepository;

        static void Main(string[] args)
        {
            try {
                var keyVaultUri = ConfigurationManager.AppSettings["keyVaultUri"];
                var connectionString = GetKeyVaultSecretAsync(keyVaultUri, "connectionString").GetAwaiter().GetResult();
                _azureDatabaseBackupFileRepository = new AzureDatabaseBackupFileRepository<AzureDatabaseBackupFile>(connectionString);

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
                var tasks = new List<Task<IEnumerable<AzureDatabaseBackupFile>>>();
                var storageAccounts = GetDatabaseBackupStorageAccounts();
                foreach (var storageAccount in storageAccounts) {
                    tasks.Add(GetAzureDatabaseBackupFilesAsync(storageAccount));
                }

                var results = Task.WhenAll(tasks.ToArray()).GetAwaiter().GetResult();
                var azureDatabaseBackupFiles = new List<AzureDatabaseBackupFile>();
                foreach (var result in results) {
                    azureDatabaseBackupFiles.AddRange(result);
                }

                var inventory = UpdateDatabaseAsync(azureDatabaseBackupFiles).GetAwaiter().GetResult();
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

        private static IEnumerable<DatabaseBackupStorageAccount> GetDatabaseBackupStorageAccounts()
        {
            return _azureDatabaseBackupFileRepository.GetDatabaseBackupStorageAccounts().GetAwaiter().GetResult();

            //var text = System.IO.File.ReadAllText("DatabaseStorageAccounts.json");
            //var databaseBackupStorageAccounts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DatabaseBackupStorageAccount>>(text);
            //return databaseBackupStorageAccounts;
        }

        private static async Task<IEnumerable<AzureDatabaseBackupFile>> GetAzureDatabaseBackupFilesAsync(
            DatabaseBackupStorageAccount databaseBackupStorageAccount)
        {
            var azureDatabaseBackupFiles = new List<AzureDatabaseBackupFile>();

            try {
                var storageAccount = await _azureService.GetStorageAccountByResourceGroupNameAsync(databaseBackupStorageAccount.SubscriptionId,
                    databaseBackupStorageAccount.ResourceGroup, databaseBackupStorageAccount.Name);

                if (storageAccount == null) {
                    throw new Exception($"Unable to get storage account object for: {databaseBackupStorageAccount.Name}");
                }
                
                var keys = storageAccount.GetKeys().FirstOrDefault();
                if (keys == null) {
                    throw new Exception($"Unable to get storage account keys for: {storageAccount.Id}");
                }

                var storageAccountConnectionString =
                    $"DefaultEndpointsProtocol=https;AccountName={storageAccount.Name};AccountKey={keys?.Value};EndpointSuffix=core.windows.net";

                try {
                    var blobClient = CloudStorageAccount.Parse(storageAccountConnectionString).CreateCloudBlobClient();
                    var containers = blobClient.ListContainers().ToList();

                    var blobTasks = new List<Task<IEnumerable<AzureDatabaseBackupFile>>>();
                    foreach (var container in containers) {
                        if (container.Name.StartsWith("Custom", StringComparison.OrdinalIgnoreCase)) {
                            continue;
                        }

                        try {
                            var blobs = container.ListBlobs();
                            foreach (var item in blobs) {
                                try {
                                    blobTasks.Add(Task.Run(() => GetBlobFiles(item, container.Name)));
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
                        if (result == null) continue;

                        foreach (var azureDatabaseBackupFile in result) {
                            azureDatabaseBackupFile.Subscription = databaseBackupStorageAccount.Subscription;
                            azureDatabaseBackupFile.SubscriptionId = databaseBackupStorageAccount.SubscriptionId;
                            azureDatabaseBackupFile.ResourceGroup = databaseBackupStorageAccount.ResourceGroup;
                            azureDatabaseBackupFile.StorageAccountAzureId = storageAccount.Id;
                            azureDatabaseBackupFile.StorageAccountName = storageAccount.Name;
                            azureDatabaseBackupFiles.Add(azureDatabaseBackupFile);
                        }
                    }
                }
                catch (Exception ex) {
                    Log.Error($"An exception occurred getting blob files for: {storageAccount.Id}.", ex);
                }
            }
            catch (Exception ex) {
                Log.Error($"An exception occurred processing storage account: {databaseBackupStorageAccount.Name}.", ex);
            }

            return azureDatabaseBackupFiles;
        }

        private static IEnumerable<AzureDatabaseBackupFile> GetBlobFiles(IListBlobItem item, string containerName, DateTime? timeStarted = null)
        {
            if (timeStarted == null) {
                timeStarted = DateTime.Now;
            }

            var elapsedTimeInMinutes = DateTime.Now.Subtract(timeStarted.Value).TotalMinutes;
            if (elapsedTimeInMinutes > 1200) {
                throw new Exception($"Took too long getting blob files. {elapsedTimeInMinutes} minutes have elapsed.");
            }

            var type = item.GetType();
            if (type.IsSubclassOf(typeof(CloudBlob))) {
                var blob = (CloudBlob) item;
                if (!blob.Name.Contains(".bak") && !blob.Name.Contains(".abf")) {
                    return null;
                }

                var azureDatabaseBackupFile = new AzureDatabaseBackupFile {
                    BackupFileName = ParseBlobName(blob.Name),
                    DatabaseName = ParseDatabaseName(blob.Name),
                    LastModifiedDateUtc = blob.Properties.LastModified?.UtcDateTime,
                    ServerName = containerName.Split('-')[0],
                    SizeInMb = (int) Math.Ceiling((decimal) blob.Properties.Length / 1024 / 1024)
                };

                if (containerName.Contains("-")) {
                    var instanceName = containerName.Split('-')[1];
                    if (!instanceName.Equals("mssqlserver") && !instanceName.Equals("oracleserver")) {
                        azureDatabaseBackupFile.InstanceName = instanceName;
                    }
                }

                return new List<AzureDatabaseBackupFile> {azureDatabaseBackupFile};
            }

            if (type == typeof(CloudBlobDirectory)) {
                var directory = (CloudBlobDirectory) item;
                //Log.Debug("Directory: " + directory.Uri);
                var azureDatabaseBackupFiles = new List<AzureDatabaseBackupFile>();
                foreach (var directoryItem in directory.ListBlobs()) {
                    var result = GetBlobFiles(directoryItem, containerName, timeStarted);
                    if (result != null) {
                        azureDatabaseBackupFiles.AddRange(result);
                    }
                }

                return azureDatabaseBackupFiles;
            }

            throw new Exception($"Unsupported blob type found: {type}");
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

        private static async Task<List<AzureDatabaseBackupFile>> UpdateDatabaseAsync(IEnumerable<AzureDatabaseBackupFile> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            try {
                var itemsToInsert = new List<AzureDatabaseBackupFile>();
                var itemsToUpdate = new List<AzureDatabaseBackupFile>();

                var existingInventory = (await _azureDatabaseBackupFileRepository.GetCollectionAsync()).ToList();

                foreach (var item in inventory) {
                    try {
                        if (!existingInventory.Any()) {
                            itemsToInsert.Add(item);
                            continue;
                        }

                        var existingItem = existingInventory.FirstOrDefault(x =>
                            string.Equals(x.SubscriptionId, item.SubscriptionId, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(x.ResourceGroup, item.ResourceGroup, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(x.StorageAccountAzureId, item.StorageAccountAzureId, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(x.ServerName, item.ServerName, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(x.InstanceName, item.InstanceName, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(x.DatabaseName, item.DatabaseName, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(x.BackupFileName, item.BackupFileName, StringComparison.OrdinalIgnoreCase));

                        if (existingItem == null) {
                            itemsToInsert.Add(item);
                        }
                        else {
                            item.Id = existingItem.Id;

                            if (!item.IsEqual(existingItem)) {
                                itemsToUpdate.Add(item);
                            }
                        }
                    }
                    catch (Exception ex) {
                        Log.Error($"An exception occurred checking whether database backup file already exists in the database: {item.BackupFileName}.", ex);
                    }
                }

                var itemsToDelete = existingInventory.Where(x => !inventory.Any(y => y.IsEqual(x)));
                var recordsDeleted = 0;
                foreach (var item in itemsToDelete) {
                    try {
                        item.Id = await _azureDatabaseBackupFileRepository.DeleteAsync(item);
                        if (item.Id == null) {
                            Log.Error($"Something went wrong deleting database backup file with id: {item.Id}.");
                        }
                        else {
                            recordsDeleted++;
                        }
                    }
                    catch (Exception ex) {
                        Log.Error($"An exception occurred deleting database backup file with id: {item.Id}.", ex);
                    }
                }

                var recordsUpdated = 0;
                foreach (var item in itemsToUpdate) {
                    try {
                        item.Id = await _azureDatabaseBackupFileRepository.UpdateAsync(item);
                        if (item.Id == null) {
                            Log.Error($"Something went wrong updating database backup file with id: {item.Id}.");
                        }
                        else {
                            recordsUpdated++;
                        }
                    }
                    catch (Exception ex) {
                        Log.Error($"An exception occurred updating database backup file with id: {item.Id}.", ex);
                    }
                }

                var recordsInserted = 0;
                foreach (var item in itemsToInsert) {
                    try {
                        item.Id = await _azureDatabaseBackupFileRepository.InsertAsync(item);
                        if (item.Id == null) {
                            Log.Error($"Something went wrong inserting database backup file: {item.BackupFileName}.");
                        }
                        else {
                            recordsInserted++;
                        }
                    }
                    catch (Exception ex) {
                        Log.Error($"An exception occurred inserting database backup file: {item.BackupFileName}.", ex);
                    }
                }

                Log.Debug($"  Number of database backup file records inserted: {recordsInserted}");
                Log.Debug($"  Number of database backup file records updated: {recordsUpdated}");
                Log.Debug($"  Number of database backup file records deleted: {recordsDeleted}");
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing database backup files.", ex);
            }

            return inventory;
        }
    }
}
