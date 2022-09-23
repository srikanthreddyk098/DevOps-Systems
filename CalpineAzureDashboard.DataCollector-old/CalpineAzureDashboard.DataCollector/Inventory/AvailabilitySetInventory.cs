using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.Models;

namespace CalpineAzureDashboard.DataCollector.Inventory
{
    public class AvailabilitySetInventory : AzureInventory<AvailabilitySetModel>
    {
        public AvailabilitySetInventory(AzureService azureService, IRepository<AvailabilitySetModel> repository) : 
            base(azureService, repository, "availability set", "Create or Update Availability Set") { }

        public override async Task<IEnumerable<AvailabilitySetModel>> GetInventoryAsync(
            IEnumerable<AvailabilitySetModel> inventoryParam = null)
        {
            if (inventoryParam != null) {
                return inventoryParam;
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s started at: {DateTime.Now}");
            var inventory = new List<AvailabilitySetModel>();

            foreach (var subscription in Subscriptions) {
                try {
                    var availabilitySets = await AzureService.GetAvailabilitySetsAsync(subscription.SubscriptionId);

                    foreach (var availabilitySet in availabilitySets) {
                        try {
                            var newAvailabilitySet = new AvailabilitySetModel();
                            newAvailabilitySet.AzureId = availabilitySet.Id;
                            newAvailabilitySet.SubscriptionId = subscription.SubscriptionId;
                            newAvailabilitySet.Subscription = subscription.DisplayName;
                            newAvailabilitySet.ResourceGroup = availabilitySet.ResourceGroupName;
                            newAvailabilitySet.Region = availabilitySet.RegionName;
                            newAvailabilitySet.Name = availabilitySet.Name;
                            newAvailabilitySet.Sku = availabilitySet.Sku?.Value;
                            newAvailabilitySet.FaultDomainCount = availabilitySet.FaultDomainCount;
                            newAvailabilitySet.UpdateDomainCount = availabilitySet.UpdateDomainCount;

                            inventory.Add(newAvailabilitySet);
                        }
                        catch (Exception ex) {
                            Log.Error($"An exception occurred getting details for {InventoryType} with Azure id: {availabilitySet.Id}", ex);
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

        public override async Task<IEnumerable<AvailabilitySetModel>> ProcessInventoryAsync(IEnumerable<AvailabilitySetModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var itemsToInsert = new List<AvailabilitySetModel>();
            var itemsToUpdate = new List<AvailabilitySetModel>();

            var existingInventory = (await Repository.GetCollectionAsync()).ToList();

            Log.Debug($"Processing Azure inventory for {inventory.Count} {InventoryType}s started at: {DateTime.Now}");
            foreach (var item in inventory) {
                try {
                    if (existingInventory.Count.Equals(0)) {
                        itemsToInsert.Add(item);
                        continue;
                    }

                    var existingItem =  existingInventory.FirstOrDefault(x => x.AzureId.Equals(item.AzureId, StringComparison.OrdinalIgnoreCase));

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