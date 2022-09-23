using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.Models.PowerShellModels;

namespace CalpineAzureDashboard.DataCollector.Inventory
{
    public class AsrReplicatedItemHealthErrorInventory : AzureInventory<AsrReplicatedItemModel.HealthErrorModel>
    {
        public AsrReplicatedItemHealthErrorInventory(AzureService azureService, IRepository<AsrReplicatedItemModel.HealthErrorModel> repository) : 
            base(azureService, repository, "ASR replicated item health error", null) { }

        public override async Task<IEnumerable<AsrReplicatedItemModel.HealthErrorModel>> GetInventoryAsync(
            IEnumerable<AsrReplicatedItemModel.HealthErrorModel> inventoryParam = null)
        {
            return await Task.Run(() => inventoryParam);
        }

        public override async Task<IEnumerable<AsrReplicatedItemModel.HealthErrorModel>> ProcessInventoryAsync(
            IEnumerable<AsrReplicatedItemModel.HealthErrorModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var existingInventory = (await Repository.GetCollectionAsync()).ToList();

            var itemsToInsert = new List<AsrReplicatedItemModel.HealthErrorModel>();
            var itemsToUpdate = new List<AsrReplicatedItemModel.HealthErrorModel>();

            Log.Debug($"Processing Azure inventory for {inventory.Count} {InventoryType}s started at: {DateTime.Now}");
            foreach (var item in inventory) {
                try {
                    if (!existingInventory.Any()) {
                        itemsToInsert.Add(item);
                        continue;
                    }

                    var existingItem = existingInventory.FirstOrDefault(x => x.IsEqual(item));

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
                    Log.Error($"An exception occurred checking whether {InventoryType} already exists in the database with Azure id {item.AzureId}.", ex);
                }
            }

            var itemsToDelete = existingInventory.Where(x => !inventory.Any(y => y.IsEqual(x)));

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