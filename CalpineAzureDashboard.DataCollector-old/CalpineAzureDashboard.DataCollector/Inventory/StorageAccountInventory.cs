using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.Models;
using Microsoft.Azure.Management.Storage.Fluent.Models;
using Newtonsoft.Json;

namespace CalpineAzureDashboard.DataCollector.Inventory
{
    public class StorageAccountInventory : AzureInventory<StorageAccountModel>
    {
        private readonly object _lockObject = new object();

        public StorageAccountInventory(AzureService azureService, IRepository<StorageAccountModel> repository) :
            base(azureService, repository, "storage account", "Create/Update Storage Account")
        {
        }

        public override async Task<IEnumerable<StorageAccountModel>> GetInventoryAsync(IEnumerable<StorageAccountModel> inventoryParam = null)
        {
            if (inventoryParam != null) {
                return inventoryParam;
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s started at: {DateTime.Now}");
            var inventory = new List<StorageAccountModel>();

            foreach (var subscription in Subscriptions) {
                try {
                    var storageAccounts = await AzureService.GetStorageAccountsAsync(subscription.SubscriptionId);

                    foreach (var storageAccount in storageAccounts) {
                        try {
                            var newStorageAccount = new StorageAccountModel();
                            newStorageAccount.AzureId = storageAccount.Id;
                            newStorageAccount.SubscriptionId = subscription.SubscriptionId;
                            newStorageAccount.Subscription = subscription.DisplayName;
                            newStorageAccount.ResourceGroup = storageAccount.ResourceGroupName;
                            newStorageAccount.Region = storageAccount.Inner.Location;
                            newStorageAccount.Name = storageAccount.Name;
                            newStorageAccount.SkuName = storageAccount.Inner.Sku?.Name.ToString();
                            newStorageAccount.SkuTier = storageAccount.Inner.Sku?.Tier?.ToString();
                            newStorageAccount.Kind = storageAccount.Inner.Kind?.ToString();
                            newStorageAccount.AccessTier = storageAccount.Inner.AccessTier?.ToString();
                            newStorageAccount.CreationTime = storageAccount.Inner.CreationTime;
                            newStorageAccount.CustomDomain = storageAccount.Inner.CustomDomain?.Name;
                            newStorageAccount.UseSubDomain = storageAccount.Inner.CustomDomain?.UseSubDomainName;
                            newStorageAccount.EnableHttpsTrafficOnly = storageAccount.Inner.EnableHttpsTrafficOnly;
                            newStorageAccount.KeySource = storageAccount.Inner.Encryption?.KeySource.Value;
                            newStorageAccount.KeyVaultName = storageAccount.Inner.Encryption?.KeyVaultProperties?.KeyName;
                            newStorageAccount.KeyVaultUri = storageAccount.Inner.Encryption?.KeyVaultProperties?.KeyVaultUri;
                            newStorageAccount.KeyVaultVersion = storageAccount.Inner.Encryption?.KeyVaultProperties?.KeyVersion;
                            newStorageAccount.BlobEncryptionEnabled = storageAccount.Inner.Encryption?.Services?.Blob?.Enabled;
                            newStorageAccount.BlobEncryptionLastEnabledTime = storageAccount.Inner.Encryption?.Services?.Blob?.LastEnabledTime;
                            newStorageAccount.FileEncryptionEnabled = storageAccount.Inner.Encryption?.Services?.File?.Enabled;
                            newStorageAccount.FileEncryptionLastEnabledTime = storageAccount.Inner.Encryption?.Services?.File?.LastEnabledTime;
                            newStorageAccount.QueueEncryptionEnabled = storageAccount.Inner.Encryption?.Services?.Queue?.Enabled;
                            newStorageAccount.QueueEncryptionLastEnabledTime = storageAccount.Inner.Encryption?.Services?.Queue?.LastEnabledTime;
                            newStorageAccount.TableEncryptionEnabled = storageAccount.Inner.Encryption?.Services?.Table?.Enabled;
                            newStorageAccount.TableEncryptionLastEnabledTime = storageAccount.Inner.Encryption?.Services?.Table?.LastEnabledTime;
                            newStorageAccount.SystemAssignedManagedServiceIdentityPrincipalId =
                                storageAccount.SystemAssignedManagedServiceIdentityPrincipalId;
                            newStorageAccount.SystemAssignedManagedServiceIdentityTenantId =
                                storageAccount.SystemAssignedManagedServiceIdentityTenantId;
                            newStorageAccount.LastGeoFailoverTime = storageAccount.Inner.LastGeoFailoverTime;
                            newStorageAccount.IsAccessAllowedFromAllNetworks = storageAccount.IsAccessAllowedFromAllNetworks;
                            newStorageAccount.CanAccessFromAzureServices = storageAccount.CanAccessFromAzureServices;
                            newStorageAccount.CanReadMetricsFromAnyNetwork = storageAccount.CanReadMetricsFromAnyNetwork;
                            newStorageAccount.CanReadLogEntriesFromAnyNetwork = storageAccount.CanReadLogEntriesFromAnyNetwork;
                            newStorageAccount.IpAddressesWithAccess =
                                storageAccount.IPAddressesWithAccess.Any() ? string.Join(",", storageAccount.IPAddressesWithAccess) : null;
                            newStorageAccount.IpAddressRangesWithAccess =
                                storageAccount.IPAddressRangesWithAccess.Any() ? string.Join(",", storageAccount.IPAddressRangesWithAccess) : null;
                            newStorageAccount.NetworkSubnetsWithAccess =
                                storageAccount.NetworkSubnetsWithAccess.Any() ? string.Join(",", storageAccount.NetworkSubnetsWithAccess) : null;
                            newStorageAccount.ProvisioningState = storageAccount.Inner.ProvisioningState.ToString();
                            newStorageAccount.PrimaryLocation = storageAccount.Inner.PrimaryLocation;
                            newStorageAccount.SecondaryLocation = storageAccount.Inner.SecondaryLocation;
                            newStorageAccount.PrimaryStatus = storageAccount.Inner.StatusOfPrimary?.ToString();
                            newStorageAccount.SecondaryStatus = storageAccount.Inner.StatusOfSecondary?.ToString();
                            newStorageAccount.PrimaryBlobEndpoint = storageAccount.Inner.PrimaryEndpoints?.Blob;
                            newStorageAccount.PrimaryFileEndpoint = storageAccount.Inner.PrimaryEndpoints?.File;
                            newStorageAccount.PrimaryQueueEndpoint = storageAccount.Inner.PrimaryEndpoints?.Queue;
                            newStorageAccount.PrimaryTableEndpoint = storageAccount.Inner.PrimaryEndpoints?.Table;
                            newStorageAccount.SecondaryBlobEndpoint = storageAccount.Inner.SecondaryEndpoints?.Blob;
                            newStorageAccount.SecondaryFileEndpoint = storageAccount.Inner.SecondaryEndpoints?.File;
                            newStorageAccount.SecondaryQueueEndpoint = storageAccount.Inner.SecondaryEndpoints?.Queue;
                            newStorageAccount.SecondaryTableEndpoint = storageAccount.Inner.SecondaryEndpoints?.Table;

                            if (storageAccount.Inner.Tags != null && storageAccount.Inner.Tags.Any()) {
                                newStorageAccount.Tags = JsonConvert.SerializeObject(storageAccount.Inner.Tags);
                            }

                            inventory.Add(newStorageAccount);
                        }
                        catch (Exception ex) {
                            Log.Error($"An exception occurred getting details for {InventoryType}: {storageAccount.Id}", ex);
                        }
                    }
                }
                catch (Exception ex) {
                    throw new Exception($"An exception occurred getting {InventoryType} inventory for subscription: {subscription.DisplayName}.", ex);
                }
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s finished at: {DateTime.Now}");
            return inventory;
        }

        public override async Task<IEnumerable<StorageAccountModel>> ProcessInventoryAsync(IEnumerable<StorageAccountModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var itemsToInsert = new List<StorageAccountModel>();
            var itemsToUpdate = new List<StorageAccountModel>();

            var existingInventory = (await Repository.GetCollectionAsync()).ToList();

            Log.Debug($"Processing Azure inventory for {inventory.Count} {InventoryType}s started at: {DateTime.Now}");
            foreach (var item in inventory) {
                try {
                    if (existingInventory.Count.Equals(0)) {
                        itemsToInsert.Add(item);
                        continue;
                    }

                    var existingItem = existingInventory.FirstOrDefault(x => x.AzureId.Equals(item.AzureId, StringComparison.OrdinalIgnoreCase));

                    if (existingItem == null) {
                        //get the first "Create or Update" event to determine who created the resource and when
                        try {
                            var createdEvent = GetCreatedEvent(item.SubscriptionId, item.ResourceGroup, item.AzureId, item.CreatedDtUtc,
                                item.CreatedDtUtc?.Add(new TimeSpan(0, 0, 1)));
                            item.CreatedBy = createdEvent?.Caller;
                            item.CreatedDtUtc = createdEvent?.EventTimestamp;
                        }
                        catch (Exception ex) {
                            Log.Error($"Something went wrong getting created event from the activity logs for {InventoryType} with Azure id: {item.AzureId}.", ex);
                        }

                        itemsToInsert.Add(item);
                    }
                    else {
                        item.Id = existingItem.Id;
                        item.CreatedBy = existingItem.CreatedBy;
                        item.CreatedDtUtc = existingItem.CreatedDtUtc;

                        item.BlobSizeInGb = existingItem.BlobSizeInGb;
                        item.NumberOfBlobContainers = existingItem.NumberOfBlobContainers;
                        item.NumberOfBlobs = existingItem.NumberOfBlobs;
                        item.FileShareSizeInGb = existingItem.FileShareSizeInGb;
                        item.NumberOfFileShares = existingItem.NumberOfFileShares;

                        if (!item.IsEqual(existingItem)) {
                            itemsToUpdate.Add(item);
                        }
                    }
                }
                catch (Exception ex) {
                    Log.Error($"An exception occurred checking whether {InventoryType} already exists in the database with id {item.AzureId}.", ex);
                }
            }

            var itemsToDelete = existingInventory.Where(x => !inventory.Any(y => y.AzureId.Equals(x.AzureId, StringComparison.OrdinalIgnoreCase)));

            var recordsDeleted = await DeleteInventoryAsync(itemsToDelete);
            var recordsUpdated = await UpdateInventoryAsync(itemsToUpdate);
            var recordsInserted = await InsertInventoryAsync(itemsToInsert);

            Log.Debug($"  Number of {InventoryType} records inserted: {recordsInserted}");
            Log.Debug($"  Number of {InventoryType} records updated: {recordsUpdated}");
            Log.Debug($"  Number of {InventoryType} records deleted: {recordsDeleted}");

            Log.Debug($"Processing Azure inventory for {inventory.Count} {InventoryType}s finished at: {DateTime.Now}");

            return inventory;
        }
    }
}