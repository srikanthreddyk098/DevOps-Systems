using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.Models.PowerShellModels;

namespace CalpineAzureDashboard.DataCollector.Inventory
{
    public class AsrReplicatedItemDiskInventory : AzureInventory<AsrReplicatedItemModel.DiskDetailModel>
    {
        public AsrReplicatedItemDiskInventory(AzureService azureService, IRepository<AsrReplicatedItemModel.DiskDetailModel> repository) : 
            base(azureService, repository, "ASR replicated item disk", null) { }

        public override async Task<IEnumerable<AsrReplicatedItemModel.DiskDetailModel>> GetInventoryAsync(
            IEnumerable<AsrReplicatedItemModel.DiskDetailModel> inventoryParam = null)
        {
            return await Task.Run(() => inventoryParam);
        }

        public override async Task<IEnumerable<AsrReplicatedItemModel.DiskDetailModel>> ProcessInventoryAsync(
            IEnumerable<AsrReplicatedItemModel.DiskDetailModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var existingInventory = (await Repository.GetCollectionAsync()).ToList();

            var itemsToInsert = new List<AsrReplicatedItemModel.DiskDetailModel>();
            var itemsToUpdate = new List<AsrReplicatedItemModel.DiskDetailModel>();

            Log.Debug($"Processing Azure inventory for {inventory.Count} {InventoryType}s started at: {DateTime.Now}");
            foreach (var item in inventory) {
                try {
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