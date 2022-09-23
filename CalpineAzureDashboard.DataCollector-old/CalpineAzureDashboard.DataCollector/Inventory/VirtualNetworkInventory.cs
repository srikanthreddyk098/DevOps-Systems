using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.Models;

namespace CalpineAzureDashboard.DataCollector.Inventory
{
    public class VirtualNetworkInventory : AzureInventory<VirtualNetworkModel>
    {
        public VirtualNetworkInventory(AzureService azureService, IRepository<VirtualNetworkModel> repository) : base(azureService, repository, "virtual network",
                                                                                                                      "Create or Update Virtual Network") { }

        public override async Task<IEnumerable<VirtualNetworkModel>> GetInventoryAsync(IEnumerable<VirtualNetworkModel> inventoryParam = null)
        {
            if (inventoryParam != null) {
                return inventoryParam;
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s started at: {DateTime.Now}");
            var inventory = new List<VirtualNetworkModel>();

            foreach (var subscription in Subscriptions) {
                try {
                    var virtualNetworks = await AzureService.GetNetworksAsync(subscription.SubscriptionId);

                    foreach (var virtualNetwork in virtualNetworks) {
                        try {
                            var newVirtualNetwork = new VirtualNetworkModel();
                            newVirtualNetwork.AzureId = virtualNetwork.Id;
                            newVirtualNetwork.SubscriptionId = subscription.SubscriptionId;
                            newVirtualNetwork.Subscription = subscription.DisplayName;
                            newVirtualNetwork.ResourceGroup = virtualNetwork.ResourceGroupName;
                            newVirtualNetwork.Region = virtualNetwork.Region?.Name;
                            newVirtualNetwork.Name = virtualNetwork.Name;
                            newVirtualNetwork.AddressSpace = virtualNetwork.AddressSpaces?.Count > 0 ? String.Join(",", virtualNetwork.AddressSpaces) : null;
                            newVirtualNetwork.DdosProtectionPlan = virtualNetwork.DdosProtectionPlanId;
                            newVirtualNetwork.DnsServers = virtualNetwork.DnsServerIPs?.Count > 0 ? string.Join(",", virtualNetwork.DnsServerIPs) : null;
                            newVirtualNetwork.EnableDdosProtection = virtualNetwork.IsDdosProtectionEnabled;
                            newVirtualNetwork.EnableVmProtection = virtualNetwork.IsVmProtectionEnabled;
                            if (newVirtualNetwork.Tags != null && virtualNetwork.Tags.Count > 0) {
                                var tags = string.Join(";", virtualNetwork.Tags.Select(x => x.Key + ":" + x.Value));
                                newVirtualNetwork.Tags = string.IsNullOrEmpty(tags) ? null : tags;
                            }

                            var virtualNetworkPeerings = new List<VirtualNetworkModel.VirtualNetworkPeeringModel>();
                            foreach (var peering in await virtualNetwork.Peerings.ListAsync()) {
                                try {
                                    var newPeering = new VirtualNetworkModel.VirtualNetworkPeeringModel();
                                    newPeering.Name = peering.Name;
                                    newPeering.AzureId = peering.Id;
                                    newPeering.AllowForwardedTraffic = peering.Inner?.AllowForwardedTraffic;
                                    newPeering.AllowGatewayTransit = peering.Inner?.AllowGatewayTransit;
                                    newPeering.AllowVirtualNetworkAccess = peering.Inner?.AllowVirtualNetworkAccess;
                                    newPeering.PeeringState = peering.Inner?.PeeringState?.Value;
                                    newPeering.ProvisioningState = peering.Inner?.ProvisioningState.Value;
                                    newPeering.RemoteAddressSpace = peering.Inner?.RemoteAddressSpace?.AddressPrefixes.Count > 0
                                        ? string.Join(",", peering.Inner?.RemoteAddressSpace.AddressPrefixes)
                                        : null;
                                    newPeering.RemoteVirtualNetwork = peering.Inner?.RemoteVirtualNetwork.Id;
                                    newPeering.UseRemoteGateways = peering.Inner?.UseRemoteGateways;

                                    virtualNetworkPeerings.Add(newPeering);
                                }
                                catch (Exception ex) {
                                    Log.Error($"An exception occurred getting details for peering: {peering?.Name}", ex);
                                }
                            }

                            newVirtualNetwork.Peerings = virtualNetworkPeerings;

                            var subnets = new List<SubnetModel>();
                            foreach (var subnet in virtualNetwork.Subnets) {
                                try {
                                    var subnetInner = subnet.Value.Inner;
                                    if (subnetInner == null) {
                                        continue;
                                    }

                                    var newSubnet = new SubnetModel();
                                    newSubnet.Name = subnetInner.Name;
                                    newSubnet.AzureId = subnetInner.Id;
                                    newSubnet.AddressPrefix = subnetInner.AddressPrefix;
                                    newSubnet.NetworkSecurityGroupAzureId = subnetInner.NetworkSecurityGroup?.Id;
                                    newSubnet.ResourceNavigationLinks = subnetInner.ResourceNavigationLinks?.Count > 0
                                        ? string.Join(",", subnetInner.ResourceNavigationLinks)
                                        : null;
                                    newSubnet.RouteTableAzureId = subnetInner.RouteTable?.Id;
                                    newSubnet.ProvisioningState = subnetInner.ProvisioningState.Value;

                                    var ipConfigurations = new List<SubnetModel.IpConfigurationModel>();
                                    if (subnetInner.IpConfigurations != null) {
                                        foreach (var ipConfiguration in subnetInner.IpConfigurations) {
                                            try {
                                                var newIpConfiguration = new SubnetModel.IpConfigurationModel();
                                                newIpConfiguration.Name = ipConfiguration.Name;
                                                newIpConfiguration.AzureId = ipConfiguration.Id;
                                                newIpConfiguration.PrivateIpAddress = ipConfiguration.PrivateIPAddress;
                                                newIpConfiguration.PrivateIpAllocationMethod = ipConfiguration.PrivateIPAllocationMethod?.Value;
                                                newIpConfiguration.ProvisioningState = ipConfiguration.ProvisioningState.Value;
                                                newIpConfiguration.PublicIpAddressId = ipConfiguration.PublicIPAddress?.Id;

                                                ipConfigurations.Add(newIpConfiguration);
                                            }
                                            catch (Exception ex) {
                                                Log.Error($"An exception occurred getting details for ip configuration: {ipConfiguration?.Name}", ex);
                                            }
                                        }
                                    }

                                    newSubnet.IpConfigurations = ipConfigurations;

                                    var serviceEndpoints = new List<SubnetModel.SubnetServiceEndpointModel>();
                                    if (subnetInner.ServiceEndpoints != null) {
                                        foreach (var serviceEndpoint in subnetInner.ServiceEndpoints) {
                                            try {
                                                var newServiceEndpoint = new SubnetModel.SubnetServiceEndpointModel();
                                                newServiceEndpoint.Locations = serviceEndpoint.Locations?.Count > 0
                                                    ? string.Join(",", serviceEndpoint.Locations)
                                                    : null;
                                                newServiceEndpoint.ProvisioningState = serviceEndpoint.ProvisioningState.Value;
                                                newServiceEndpoint.Service = serviceEndpoint.Service;

                                                serviceEndpoints.Add(newServiceEndpoint);
                                            }
                                            catch (Exception ex) {
                                                Log.Error($"An exception occurred getting details for service endpoint: {serviceEndpoint?.Service}", ex);
                                            }
                                        }
                                    }

                                    newSubnet.ServiceEndpoints = serviceEndpoints;
                                    subnets.Add(newSubnet);
                                }
                                catch (Exception ex) {
                                    Log.Error($"An exception occurred getting details for subnet: {subnet.Value?.Name}", ex);
                                }
                            }

                            newVirtualNetwork.Subnets = subnets;
                            inventory.Add(newVirtualNetwork);
                        }
                        catch (Exception ex) {
                            Log.Error($"An exception occurred getting details for network interface: {virtualNetwork.Id}", ex);
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

        public override async Task<IEnumerable<VirtualNetworkModel>> ProcessInventoryAsync(IEnumerable<VirtualNetworkModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var itemsToInsert = new List<VirtualNetworkModel>();
            var itemsToUpdate = new List<VirtualNetworkModel>();

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

            foreach (var virtualNetwork in inventory) {
                foreach (var peering in virtualNetwork.Peerings) {
                    peering.VirtualNetworkId = virtualNetwork.Id;
                }
                foreach (var subnet in virtualNetwork.Subnets) {
                    subnet.VirtualNetworkId = virtualNetwork.Id;
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