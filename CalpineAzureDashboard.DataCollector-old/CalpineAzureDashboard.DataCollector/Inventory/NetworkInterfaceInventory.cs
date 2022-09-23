using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.Models;

namespace CalpineAzureDashboard.DataCollector.Inventory
{
    public class NetworkInterfaceInventory : AzureInventory<NetworkInterfaceModel>
    {
        public NetworkInterfaceInventory(AzureService azureService, IRepository<NetworkInterfaceModel> repository) : 
            base(azureService, repository, "network interface", "Create or Update Network Interface") { }

        public override async Task<IEnumerable<NetworkInterfaceModel>> GetInventoryAsync(
            IEnumerable<NetworkInterfaceModel> inventoryParam = null)
        {
            if (inventoryParam != null) {
                return inventoryParam;
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s started at: {DateTime.Now}");
            var inventory = new List<NetworkInterfaceModel>();

            foreach (var subscription in Subscriptions) {
                try {
                    var networkInterfaces = await AzureService.GetNetworkInterfacesAsync(subscription.SubscriptionId);

                    foreach (var nic in networkInterfaces) {
                        try {
                            var newNicInventory = new NetworkInterfaceModel();
                            newNicInventory.AzureId = nic.Id;
                            newNicInventory.SubscriptionId = subscription.SubscriptionId;
                            newNicInventory.Subscription = subscription.DisplayName;
                            newNicInventory.ResourceGroup = nic.ResourceGroupName;
                            newNicInventory.Region = nic.Region?.Name;
                            newNicInventory.Name = nic.Name;
                            newNicInventory.NumberOfDnsServers = nic.DnsServers.Count;
                            newNicInventory.NumberOfAppliedDnsServers = nic.AppliedDnsServers.Count;
                            newNicInventory.InternalDnsNameLabel = nic.InternalDnsNameLabel;
                            newNicInventory.InternalDomainNameSuffix = nic.InternalDomainNameSuffix;
                            newNicInventory.InternalFqdn = nic.InternalFqdn;
                            newNicInventory.IsAcceleratedNetworkingEnabled = nic.IsAcceleratedNetworkingEnabled;
                            newNicInventory.IsIpForwardingEnabled = nic.IsIPForwardingEnabled;
                            newNicInventory.MacAddress = nic.MacAddress;
                            newNicInventory.NetworkSecurityGroupId = nic.NetworkSecurityGroupId;
                            newNicInventory.PrimaryPrivateIp = nic.PrimaryPrivateIP;
                            newNicInventory.PrimaryPrivateIpAllocationMethod = nic.PrimaryPrivateIPAllocationMethod?.Value;
                            newNicInventory.VirtualMachineAzureId = nic.VirtualMachineId;

                            var ipConfigurations = new List<NetworkInterfaceModel.IpConfigurationModel>();
                            foreach (var ipConfiguration in nic.IPConfigurations) {
                                try {
                                    var newIpConfiguration = new NetworkInterfaceModel.IpConfigurationModel();
                                    newIpConfiguration.Name = ipConfiguration.Value?.Name;
                                    newIpConfiguration.AzureId = ipConfiguration.Value?.Inner?.Id;
                                    newIpConfiguration.IsPrimary = ipConfiguration.Value?.IsPrimary;
                                    newIpConfiguration.VirtualNetworkId = ipConfiguration.Value?.NetworkId;
                                    newIpConfiguration.SubnetId = ipConfiguration.Value?.Inner?.Subnet?.Id;
                                    newIpConfiguration.PrivateIpAddress = ipConfiguration.Value?.PrivateIPAddress;
                                    newIpConfiguration.PrivateIpAddressVersion = ipConfiguration.Value?.PrivateIPAddressVersion?.Value;
                                    newIpConfiguration.PrivateIpAllocationMethod = ipConfiguration.Value?.PrivateIPAllocationMethod?.Value;
                                    newIpConfiguration.PublicIpAddressId = ipConfiguration.Value?.PublicIPAddressId;

                                    ipConfigurations.Add(newIpConfiguration);
                                }
                                catch (Exception ex) {
                                    Log.Error($"An exception occurred getting details for ip configuration: {ipConfiguration.Value?.Name}", ex);
                                }
                            }

                            newNicInventory.IpConfigurations = ipConfigurations;
                            inventory.Add(newNicInventory);
                        }
                        catch (Exception ex) {
                            Log.Error($"An exception occurred getting details for network interface: {nic.Id}", ex);
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

        public override async Task<IEnumerable<NetworkInterfaceModel>> ProcessInventoryAsync(
            IEnumerable<NetworkInterfaceModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var itemsToInsert = new List<NetworkInterfaceModel>();
            var itemsToUpdate = new List<NetworkInterfaceModel>();

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

            foreach (var networkInterface in inventory) {
                foreach (var ipConfiguration in networkInterface.IpConfigurations) {
                    ipConfiguration.NetworkInterfaceId = networkInterface.Id;
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