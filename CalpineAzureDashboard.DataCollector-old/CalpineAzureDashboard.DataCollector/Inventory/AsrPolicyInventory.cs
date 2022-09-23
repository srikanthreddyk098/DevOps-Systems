using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.Models.PowerShellModels;

namespace CalpineAzureDashboard.DataCollector.Inventory
{
    public class AsrPolicyInventory : AzureInventory<AsrPolicyModel>
    {
        public AsrPolicyInventory(AzureService azureService, IRepository<AsrPolicyModel> repository) : 
            base(azureService, repository, "ASR policy", null) { }

        public override async Task<IEnumerable<AsrPolicyModel>> GetInventoryAsync(IEnumerable<AsrPolicyModel> inventoryParam = null)
        {
            if (inventoryParam != null) {
                return inventoryParam;
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s started at: {DateTime.Now}");
            var inventory = new List<AsrPolicyModel>();

            foreach (var subscription in Subscriptions) {
                try {
                    var asrPolicies = await AzureService.GetAsrPoliciesAsync(subscription.SubscriptionId);

                    if (asrPolicies.HadErrors) {
                        Log.Error($"An error occurred getting details for {InventoryType}: {Environment.NewLine} {asrPolicies.GetErrors()}");
                        //throw new Exception($"Unable to get details for {InventoryType}. Check logs for details.");
                    }

                    foreach (var asrPolicy in asrPolicies.Items) {
                        try {
                            var newAsrPolicy = new AsrPolicyModel();
                            newAsrPolicy.AzureId = asrPolicy.ID;
                            newAsrPolicy.SubscriptionId = subscription.SubscriptionId;
                            newAsrPolicy.Subscription = subscription.DisplayName;
                            newAsrPolicy.ResourceGroup = asrPolicy.ID.Split('/')[4];
                            newAsrPolicy.VaultName = asrPolicy.ID.Split('/')[8];
                            newAsrPolicy.Name = asrPolicy.Name;
                            newAsrPolicy.FriendlyName = asrPolicy.FriendlyName;
                            newAsrPolicy.ReplicationProvider = asrPolicy.ReplicationProvider;
                            newAsrPolicy.AppConsistentFrequencyInMinutes = asrPolicy.ReplicationProviderSettings.AppConsistentFrequencyInMinutes;
                            newAsrPolicy.CrashConsistentFrequencyInMinutes = asrPolicy.ReplicationProviderSettings.CrashConsistentFrequencyInMinutes;
                            newAsrPolicy.MultiVmSyncStatus = asrPolicy.ReplicationProviderSettings.MultiVmSyncStatus;
                            newAsrPolicy.RecoveryPointHistory = asrPolicy.ReplicationProviderSettings.RecoveryPointHistory;
                            newAsrPolicy.RecoveryPointThresholdInMinutes = asrPolicy.ReplicationProviderSettings.RecoveryPointThresholdInMinutes;
                            newAsrPolicy.Type = asrPolicy.Type;

                            inventory.Add(newAsrPolicy);
                        }
                        catch (Exception ex) {
                            throw new Exception($"An exception occurred getting details for {InventoryType}: {asrPolicy.ID}", ex);
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

        public override async Task<IEnumerable<AsrPolicyModel>> ProcessInventoryAsync(IEnumerable<AsrPolicyModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var itemsToInsert = new List<AsrPolicyModel>();
            var itemsToUpdate = new List<AsrPolicyModel>();

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