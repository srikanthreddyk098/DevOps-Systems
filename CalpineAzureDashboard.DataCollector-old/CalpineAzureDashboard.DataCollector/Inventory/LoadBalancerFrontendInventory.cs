using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.Models;

namespace CalpineAzureDashboard.DataCollector.Inventory
{
    public class LoadBalancerFrontendInventory : AzureInventory<LoadBalancerModel.FrontendModel>
    {
        public LoadBalancerFrontendInventory(AzureService azureService, IRepository<LoadBalancerModel.FrontendModel> repository) : 
            base(azureService, repository, "load balancer frontend", null) { }

        public override async Task<IEnumerable<LoadBalancerModel.FrontendModel>> GetInventoryAsync(IEnumerable<LoadBalancerModel.FrontendModel> inventoryParam = null)
        {
            return await Task.Run(() => inventoryParam);
        }

        public override async Task<IEnumerable<LoadBalancerModel.FrontendModel>> ProcessInventoryAsync(IEnumerable<LoadBalancerModel.FrontendModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var existingInventory = (await Repository.GetCollectionAsync()).ToList();

            var itemsToInsert = new List<LoadBalancerModel.FrontendModel>();
            var itemsToUpdate = new List<LoadBalancerModel.FrontendModel>();

            Log.Debug($"Processing Azure inventory for {inventory.Count} {InventoryType}s started at: {DateTime.Now}");
            foreach (var item in inventory) {
                try {
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
                    Log.Error($"An exception occurred checking whether {InventoryType} already exists in the database with Azure id {item.AzureId}.", ex);
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