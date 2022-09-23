using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.Models;
using Newtonsoft.Json;

namespace CalpineAzureDashboard.DataCollector.Inventory
{
    public class ResourceGroupInventory : AzureInventory<ResourceGroupModel>
    {
        public ResourceGroupInventory(AzureService azureService, IRepository<ResourceGroupModel> repository) : 
            base(azureService, repository, "resource group", "Update resource group") { }

        public override async Task<IEnumerable<ResourceGroupModel>> GetInventoryAsync(IEnumerable<ResourceGroupModel> inventoryParam = null)
        {
            if (inventoryParam != null) {
                return inventoryParam;
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s started at: {DateTime.Now}");
            var inventory = new List<ResourceGroupModel>();

            foreach (var subscription in Subscriptions) {
                try {
                    var resourceGroups = await AzureService.GetResourceGroupsInSubscriptionAsync(subscription.SubscriptionId);

                    foreach (var resourceGroup in resourceGroups) {
                        try {
                            var newResourceGroup = new ResourceGroupModel();
                            newResourceGroup.AzureId = resourceGroup.Id;
                            newResourceGroup.SubscriptionId = subscription.SubscriptionId;
                            newResourceGroup.Subscription = subscription.DisplayName;
                            newResourceGroup.Region = resourceGroup.RegionName;
                            newResourceGroup.Name = resourceGroup.Name;
                            newResourceGroup.ProvisioningState = resourceGroup.ProvisioningState;
                            newResourceGroup.ManagedBy = resourceGroup.Inner.ManagedBy;
                            if (resourceGroup.Tags != null && resourceGroup.Tags.Any()) {
                                newResourceGroup.Tags = JsonConvert.SerializeObject(resourceGroup.Tags);
                                if (resourceGroup.Tags.TryGetValue("BUnit", out var businessUnit)) {
                                    newResourceGroup.BusinessUnit = businessUnit;
                                }
                            }

                            inventory.Add(newResourceGroup);
                        }
                        catch (Exception ex) {
                            Log.Error($"An exception occurred getting details for {InventoryType}: {resourceGroup.Id}", ex);
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

        public override async Task<IEnumerable<ResourceGroupModel>> ProcessInventoryAsync(IEnumerable<ResourceGroupModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var itemsToInsert = new List<ResourceGroupModel>();
            var itemsToUpdate = new List<ResourceGroupModel>();

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
                            var createdEvent = GetCreatedEvent(item.SubscriptionId, item.Name, item.AzureId, item.CreatedDtUtc,
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