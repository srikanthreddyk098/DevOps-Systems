using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.Models;

namespace CalpineAzureDashboard.DataCollector.Inventory
{
    public class PublicIpInventory : AzureInventory<PublicIpModel>
    {
        public PublicIpInventory(AzureService azureService, IRepository<PublicIpModel> repository) : 
            base(azureService, repository, "public ip", "Create or Update Public Ip Address") { }

        public override async Task<IEnumerable<PublicIpModel>> GetInventoryAsync(
            IEnumerable<PublicIpModel> inventoryParam = null)
        {
            if (inventoryParam != null) {
                return inventoryParam;
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s started at: {DateTime.Now}");
            var inventory = new List<PublicIpModel>();

            foreach (var subscription in Subscriptions) {
                try {
                    var publicIps = await AzureService.GetPublicIpAddressesAsync(subscription.SubscriptionId);

                    foreach (var publicIp in publicIps) {
                        try {
                            var newPublicIp = new PublicIpModel();
                            newPublicIp.AzureId = publicIp.Id;
                            newPublicIp.SubscriptionId = subscription.SubscriptionId;
                            newPublicIp.Subscription = subscription.DisplayName;
                            newPublicIp.ResourceGroup = publicIp.ResourceGroupName;
                            newPublicIp.Region = publicIp.RegionName;
                            newPublicIp.Name = publicIp.Name;
                            newPublicIp.IpAddress = publicIp.IPAddress;
                            newPublicIp.Version = publicIp.Version?.Value;
                            newPublicIp.Fqdn = publicIp.Fqdn;
                            newPublicIp.HasAssignedLoadBalancer = publicIp.HasAssignedLoadBalancer;
                            newPublicIp.HasAssignedNetworkInterface = publicIp.HasAssignedNetworkInterface;
                            newPublicIp.IpAllocationMethod = publicIp.IPAllocationMethod?.Value;
                            newPublicIp.IdleTimeoutInMinutes = publicIp.IdleTimeoutInMinutes;
                            newPublicIp.LeafDomainLabel = publicIp.LeafDomainLabel;
                            newPublicIp.ReverseFqdn = publicIp.ReverseFqdn;
                            newPublicIp.AvailabilityZones = publicIp.AvailabilityZones.Count > 0 ? string.Join("; ", publicIp.AvailabilityZones) : null;

                            inventory.Add(newPublicIp);
                        }
                        catch (Exception ex) {
                            Log.Error($"An exception occurred getting details for public ip address: {publicIp.Id}", ex);
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

        public override async Task<IEnumerable<PublicIpModel>> ProcessInventoryAsync(
            IEnumerable<PublicIpModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var itemsToInsert = new List<PublicIpModel>();
            var itemsToUpdate = new List<PublicIpModel>();

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