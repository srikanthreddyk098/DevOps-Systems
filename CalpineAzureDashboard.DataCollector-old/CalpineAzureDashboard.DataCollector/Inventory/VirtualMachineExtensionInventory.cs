using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.Models;

namespace CalpineAzureDashboard.DataCollector.Inventory
{
    public class VirtualMachineExtensionInventory : AzureInventory<VirtualMachineModel.VirtualMachineExtensionModel>
    {
        public VirtualMachineExtensionInventory(AzureService azureService, IRepository<VirtualMachineModel.VirtualMachineExtensionModel> repository) :
            base(azureService, repository, "virtual machine extension", "Create or Update Virtual Machine Extension") { }

        public override async Task<IEnumerable<VirtualMachineModel.VirtualMachineExtensionModel>> GetInventoryAsync(
            IEnumerable<VirtualMachineModel.VirtualMachineExtensionModel> inventoryParam = null)
        {
            return await Task.Run(() => inventoryParam);
        }

        public override async Task<IEnumerable<VirtualMachineModel.VirtualMachineExtensionModel>> ProcessInventoryAsync(
            IEnumerable<VirtualMachineModel.VirtualMachineExtensionModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }
            
            var itemsToInsert = new List<VirtualMachineModel.VirtualMachineExtensionModel>();
            var itemsToUpdate = new List<VirtualMachineModel.VirtualMachineExtensionModel>();

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
                            var subscriptionId = item.AzureId.Split('/')[2];
                            var resourceGroup = item.AzureId.Split('/')[4];
                            //var createdEvent = GetCreatedEvent(subscriptionId, resourceGroup, item.AzureId).Result;
                            var logs = AzureService.GetActivityLogsByResourceId(subscriptionId, resourceGroup, item.AzureId);
                            var createdEvent = logs.Where(x => !string.IsNullOrEmpty(x.OperationName?.LocalizedValue) &&
                                                               x.OperationName.LocalizedValue.StartsWith("Create or Update Virtual Machine Extension",
                                                                   StringComparison.OrdinalIgnoreCase)).OrderBy(x => x.EventTimestamp)
                                .FirstOrDefault();

                            //var createdEvent2 = await GetCreatedEvent(subscriptionId, resourceGroup, item.AzureId);
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