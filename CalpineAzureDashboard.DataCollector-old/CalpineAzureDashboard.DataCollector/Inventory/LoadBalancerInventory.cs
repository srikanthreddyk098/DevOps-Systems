using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.Models;

namespace CalpineAzureDashboard.DataCollector.Inventory
{
    public class LoadBalancerInventory : AzureInventory<LoadBalancerModel>
    {
        public LoadBalancerInventory(AzureService azureService, IRepository<LoadBalancerModel> repository) : 
            base(azureService, repository, "load balancer", "Create or Update Load Balancer") { }

        public override async Task<IEnumerable<LoadBalancerModel>> GetInventoryAsync(
            IEnumerable<LoadBalancerModel> inventoryParam = null)
        {
            if (inventoryParam != null) {
                return inventoryParam;
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s started at: {DateTime.Now}");
            var inventory = new List<LoadBalancerModel>();

            foreach (var subscription in Subscriptions) {
                try {
                    var loadBalancers = await AzureService.GetLoadBalancersAsync(subscription.SubscriptionId);

                    foreach (var loadBalancer in loadBalancers) {
                        try {
                            var newLoadBalancer = new LoadBalancerModel();
                            newLoadBalancer.AzureId = loadBalancer.Id;
                            newLoadBalancer.SubscriptionId = subscription.SubscriptionId;
                            newLoadBalancer.Subscription = subscription.DisplayName;
                            newLoadBalancer.ResourceGroup = loadBalancer.ResourceGroupName;
                            newLoadBalancer.Region = loadBalancer.RegionName;
                            newLoadBalancer.Name = loadBalancer.Name;

                            var newFrontends = new List<LoadBalancerModel.FrontendModel>();
                            foreach (var frontend in loadBalancer.Frontends) {
                                try {
                                    var newFrontend = new LoadBalancerModel.FrontendModel();
                                    newFrontend.AzureId = frontend.Value.Inner.Id;
                                    newFrontend.Name = frontend.Value.Inner.Name;
                                    newFrontend.PrivateIpAddress = frontend.Value.Inner.PrivateIPAddress;
                                    newFrontend.PrivateIpAllocationMethod = frontend.Value.Inner.PrivateIPAllocationMethod?.Value;
                                    newFrontend.PublicIpAddressId = frontend.Value.Inner.PublicIPAddress?.Id;
                                    newFrontend.SubnetId = frontend.Value.Inner.Subnet?.Id;

                                    newFrontends.Add(newFrontend);
                                }
                                catch (Exception ex) {
                                    Log.Error($"An exception occurred getting details for {InventoryType} frontend: {frontend.Value?.Inner?.Id}", ex);
                                }
                            }

                            var newBackends = new List<LoadBalancerModel.BackendModel>();
                            foreach (var backend in loadBalancer.Backends) {
                                foreach (var ipConfigurations in backend.Value.BackendNicIPConfigurationNames) {
                                    try {
                                        var newBackend = new LoadBalancerModel.BackendModel();
                                        newBackend.AzureId = backend.Value?.Inner?.Id;
                                        newBackend.Name = backend.Value?.Inner?.Name;
                                        newBackend.IpConfigurationName = ipConfigurations.Value;
                                        newBackend.NetworkInterfaceName = ipConfigurations.Key;

                                        newBackends.Add(newBackend);
                                    }
                                    catch (Exception ex) {
                                        Log.Error($"An exception occurred getting details for {InventoryType} backend: {backend.Value?.Inner?.Id}", ex);
                                    }
                                }
                            }

                            newLoadBalancer.Frontends = newFrontends;
                            newLoadBalancer.Backends = newBackends;
                            inventory.Add(newLoadBalancer);
                        }
                        catch (Exception ex) {
                            Log.Error($"An exception occurred getting details for {InventoryType}: {loadBalancer.Id}", ex);
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

        public override async Task<IEnumerable<LoadBalancerModel>> ProcessInventoryAsync(
            IEnumerable<LoadBalancerModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var itemsToInsert = new List<LoadBalancerModel>();
            var itemsToUpdate = new List<LoadBalancerModel>();

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

            foreach (var loadBalancer in inventory) {
                foreach (var frontend in loadBalancer.Frontends) {
                    frontend.LoadBalancerId = loadBalancer.Id;
                }

                foreach (var backend in loadBalancer.Backends) {
                    backend.LoadBalancerId = loadBalancer.Id;
                }
            }

            Log.Debug($"  Number of {InventoryType} records inserted: {recordsInserted}");
            Log.Debug($"  Number of {InventoryType} records updated: {recordsUpdated}");
            Log.Debug($"  Number of {InventoryType} records deleted: {recordsDeleted}");

            Log.Debug($"Processing Azure inventory for {inventory.Count} {InventoryType}s finished at: {DateTime.Now}");
            return inventory;
        }
    }
}