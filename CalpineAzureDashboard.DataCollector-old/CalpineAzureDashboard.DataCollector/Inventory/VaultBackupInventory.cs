using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.Models;
using Microsoft.Azure.Management.RecoveryServices.Backup.Models;
using Newtonsoft.Json;

namespace CalpineAzureDashboard.DataCollector.Inventory
{
    class VaultBackupInventory : AzureInventory<VaultBackupModel>
    {
        public VaultBackupInventory(AzureService azureService, IRepository<VaultBackupModel> repository) : 
            base(azureService, repository, "vault backup", "Create Backup Protected Item") { }

        public override async Task<IEnumerable<VaultBackupModel>> GetInventoryAsync(IEnumerable<VaultBackupModel> inventoryParam = null)
        {
            if (inventoryParam != null) {
                return inventoryParam;
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s started at: {DateTime.Now}");
            var inventory = new List<VaultBackupModel>();

            foreach (var subscription in Subscriptions) {
                try {
                    var vaults = await AzureService.GetRecoveryServicesVaultsAsync(subscription.SubscriptionId);

                    foreach (var vault in vaults) {
                        try {
                            var resourceGroup = vault.Id.Split('/')[4];
                            var backups = await AzureService.GetVaultProtectedItemsAsync(subscription.SubscriptionId, vault.Name, resourceGroup);

                            foreach (var backup in backups) {
                                try {
                                    var newBackup = new VaultBackupModel();
                                    newBackup.SubscriptionId = subscription.SubscriptionId;
                                    newBackup.Subscription = subscription.DisplayName;
                                    newBackup.ResourceGroup = resourceGroup;
                                    newBackup.VaultName = vault.Name;
                                    newBackup.Name = backup.Name;
                                    newBackup.AzureId = backup.Id;
                                    newBackup.BackupManagementType = backup.Properties.BackupManagementType;
                                    newBackup.BackupSetName = backup.Properties.BackupSetName;
                                    newBackup.ContainerName = backup.Properties.ContainerName;
                                    newBackup.CreateMode = backup.Properties.CreateMode;
                                    newBackup.LastRecoveryPointUtc = backup.Properties.LastRecoveryPoint;
                                    newBackup.PolicyId = string.IsNullOrEmpty(backup.Properties.PolicyId) ? null : backup.Properties.PolicyId;
                                    newBackup.WorkloadType = backup.Properties.WorkloadType;

                                    if (backup.Properties.WorkloadType.Equals("VM")) {
                                        var properties = (AzureIaaSVMProtectedItem) backup.Properties;
                                        newBackup.ExtendedInfo = properties.ExtendedInfo == null ? null : JsonConvert.SerializeObject(properties.ExtendedInfo);
                                        newBackup.FriendlyName = properties.FriendlyName;
                                        newBackup.HealthDetailsCount = properties.HealthDetails?.Count;
                                        newBackup.HealthCode = properties.HealthDetails?[0].Code;
                                        newBackup.HealthMessage = properties.HealthDetails?[0].Message;
                                        newBackup.HealthRecommendationsCount =
                                            properties.HealthDetails?[0].Recommendations.Count;
                                        newBackup.HealthStatus = properties.HealthStatus;
                                        newBackup.LastBackupStatus = properties.LastBackupStatus;
                                        newBackup.LastBackupTimeUtc = properties.LastBackupTime;
                                        newBackup.ProtectedItemDataId = properties.ProtectedItemDataId;
                                        newBackup.ProtectionState = properties.ProtectionState;
                                        newBackup.ProtectionStatus = properties.ProtectionStatus;
                                        newBackup.SourceResourceId = properties.SourceResourceId;

                                        inventory.Add(newBackup);
                                    }
                                    else if (backup.Properties.WorkloadType.Equals("FileFolder")) {
                                        var properties = (MabFileFolderProtectedItem) backup.Properties;
                                        newBackup.ComputerName = properties.ComputerName;
                                        newBackup.DeferredDeleteSyncTimeInUtc =
                                            properties.DeferredDeleteSyncTimeInUTC.HasValue
                                                ? new DateTime(properties.DeferredDeleteSyncTimeInUTC.Value)
                                                : (DateTime?) null;
                                        newBackup.ExtendedInfo = properties.ExtendedInfo == null ? null : JsonConvert.SerializeObject(properties.ExtendedInfo);
                                        newBackup.FriendlyName = properties.FriendlyName;
                                        newBackup.IsScheduledForDeferredDelete =
                                            properties.IsScheduledForDeferredDelete;
                                        newBackup.LastBackupStatus = properties.LastBackupStatus;
                                        newBackup.ProtectionState = properties.ProtectionState;
                                        newBackup.SourceResourceId = properties.SourceResourceId;

                                        inventory.Add(newBackup);
                                    }
                                    else {
                                        throw new Exception($"Encountered unsupported backup workload type: {backup.Properties.WorkloadType}. " +
                                                            $"Please add support in Azure dashboard. Full typename is {backup.Properties.GetType().FullName}");
                                    }
                                }
                                catch (Exception ex) {
                                    throw new Exception($"An exception occurred getting details for backup with id {backup.Id}", ex);
                                }
                            }
                        }
                        catch (Exception ex) {
                            throw new Exception($"An exception occurred getting details for vault with id {vault.Id}", ex);
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

        public override async Task<IEnumerable<VaultBackupModel>> ProcessInventoryAsync(IEnumerable<VaultBackupModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var itemsToInsert = new List<VaultBackupModel>();
            var itemsToUpdate = new List<VaultBackupModel>();

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
                            var createdEvent = GetCreatedEvent(item.SubscriptionId, item.ResourceGroup, item.AzureId);
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