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
    public class NetworkSecurityGroupInventory : AzureInventory<NetworkSecurityGroupModel>
    {
        public NetworkSecurityGroupInventory(AzureService azureService, IRepository<NetworkSecurityGroupModel> repository) : 
            base(azureService, repository, "network security group", "Create or Update Network Security Group") { }

        public override async Task<IEnumerable<NetworkSecurityGroupModel>> GetInventoryAsync(IEnumerable<NetworkSecurityGroupModel> inventoryParam = null)
        {
            if (inventoryParam != null) {
                return inventoryParam;
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s started at: {DateTime.Now}");
            var inventory = new List<NetworkSecurityGroupModel>();

            foreach (var subscription in Subscriptions) {
                try {
                    var networkSecurityGroups = await AzureService.GetNetworkSecurityGroupsAsync(subscription.SubscriptionId);

                    foreach (var networkSecurityGroup in networkSecurityGroups) {
                        try {
                            var newNetworkSecurityGroup = new NetworkSecurityGroupModel();
                            newNetworkSecurityGroup.AzureId = networkSecurityGroup.Id;
                            newNetworkSecurityGroup.SubscriptionId = subscription.SubscriptionId;
                            newNetworkSecurityGroup.Subscription = subscription.DisplayName;
                            newNetworkSecurityGroup.ResourceGroup = networkSecurityGroup.ResourceGroupName;
                            newNetworkSecurityGroup.Region = networkSecurityGroup.RegionName;
                            newNetworkSecurityGroup.Name = networkSecurityGroup.Name;
                            newNetworkSecurityGroup.NetworkInterfaceAzureIds =
                                networkSecurityGroup.NetworkInterfaceIds?.Count > 0 ? string.Join(",", networkSecurityGroup.NetworkInterfaceIds) : null;
                            newNetworkSecurityGroup.ProvisioningState = networkSecurityGroup.Inner.ProvisioningState.Value;
                            newNetworkSecurityGroup.ResourceGuid = networkSecurityGroup.Inner.ResourceGuid;
                            newNetworkSecurityGroup.SubnetAzureIds = networkSecurityGroup.Inner.Subnets?.Count > 0
                                ? string.Join(",", networkSecurityGroup.Inner.Subnets.Select(x => x.Id))
                                : null;
                            if (networkSecurityGroup.Tags != null && networkSecurityGroup.Tags.Any()) {
                                newNetworkSecurityGroup.Tags = JsonConvert.SerializeObject(networkSecurityGroup.Tags);
                            }

                            var securityRules = new List<NetworkSecurityGroupModel.SecurityRuleModel>();
                            foreach (var securityRule in networkSecurityGroup.SecurityRules.Values) {
                                try {
                                    var newSecurityRule = new NetworkSecurityGroupModel.SecurityRuleModel();
                                    newSecurityRule.AzureId = securityRule.Inner.Id;
                                    newSecurityRule.Name = securityRule.Inner.Name;
                                    newSecurityRule.Access = securityRule.Inner.Access?.Value;
                                    newSecurityRule.Description = securityRule.Description;
                                    newSecurityRule.DestinationAddressPrefix = securityRule.DestinationAddressPrefix;
                                    newSecurityRule.DestinationAddressPrefixes = securityRule.DestinationAddressPrefixes?.Count > 0
                                        ? string.Join(",", securityRule.DestinationAddressPrefixes)
                                        : null;
                                    newSecurityRule.DestinationApplicationSecurityGroupAzureIds = securityRule.DestinationApplicationSecurityGroupIds?.Count > 0
                                        ? string.Join(",", securityRule.DestinationApplicationSecurityGroupIds)
                                        : null;
                                    newSecurityRule.DestinationPortRange = securityRule.DestinationPortRange;
                                    newSecurityRule.DestinationPortRanges = securityRule.DestinationPortRanges?.Count > 0
                                        ? string.Join(",", securityRule.DestinationPortRanges)
                                        : null;
                                    newSecurityRule.Direction = securityRule.Direction;
                                    newSecurityRule.IsDefaultSecurityRule = false;
                                    newSecurityRule.Priority = securityRule.Priority;
                                    newSecurityRule.Protocol = securityRule.Protocol;
                                    newSecurityRule.ProvisioningState = securityRule.Inner.ProvisioningState.Value;
                                    newSecurityRule.SourceAddressPrefix = securityRule.SourceAddressPrefix;
                                    newSecurityRule.SourceAddressPrefixes = securityRule.SourceAddressPrefixes?.Count > 0
                                        ? string.Join(",", securityRule.SourceAddressPrefixes)
                                        : null;
                                    newSecurityRule.SourceApplicationSecurityGroupAzureIds = securityRule.SourceApplicationSecurityGroupIds?.Count > 0
                                        ? string.Join(",", securityRule.SourceApplicationSecurityGroupIds)
                                        : null;
                                    newSecurityRule.SourcePortRange = securityRule.SourcePortRange;
                                    newSecurityRule.SourcePortRanges = securityRule.SourcePortRanges?.Count > 0
                                        ? string.Join(",", securityRule.SourcePortRanges)
                                        : null;

                                    securityRules.Add(newSecurityRule);
                                }
                                catch (Exception ex) {
                                    Log.Error($"An exception occurred getting details for {InventoryType} security rule: {securityRule.Inner?.Id}", ex);
                                }
                            }


                            foreach (var securityRule in networkSecurityGroup.DefaultSecurityRules.Values) {
                                try {
                                    var newSecurityRule = new NetworkSecurityGroupModel.SecurityRuleModel();
                                    newSecurityRule.AzureId = securityRule.Inner.Id;
                                    newSecurityRule.Name = securityRule.Inner.Name;
                                    newSecurityRule.Access = securityRule.Inner.Access?.Value;
                                    newSecurityRule.Description = securityRule.Description;
                                    newSecurityRule.DestinationAddressPrefix = securityRule.DestinationAddressPrefix;
                                    newSecurityRule.DestinationAddressPrefixes = securityRule.DestinationAddressPrefixes.Count > 0
                                        ? string.Join(",", securityRule.DestinationAddressPrefixes)
                                        : null;
                                    newSecurityRule.DestinationApplicationSecurityGroupAzureIds = securityRule.DestinationApplicationSecurityGroupIds.Count > 0
                                        ? string.Join(",", securityRule.DestinationApplicationSecurityGroupIds)
                                        : null;
                                    newSecurityRule.DestinationPortRange = securityRule.DestinationPortRange;
                                    newSecurityRule.DestinationPortRanges = securityRule.DestinationPortRanges.Count > 0
                                        ? string.Join(",", securityRule.DestinationPortRanges)
                                        : null;
                                    newSecurityRule.Direction = securityRule.Direction;
                                    newSecurityRule.IsDefaultSecurityRule = true;
                                    newSecurityRule.Priority = securityRule.Priority;
                                    newSecurityRule.Protocol = securityRule.Protocol;
                                    newSecurityRule.ProvisioningState = securityRule.Inner.ProvisioningState.Value;
                                    newSecurityRule.SourceAddressPrefix = securityRule.SourceAddressPrefix;
                                    newSecurityRule.SourceAddressPrefixes = securityRule.SourceAddressPrefixes.Count > 0
                                        ? string.Join(",", securityRule.SourceAddressPrefixes)
                                        : null;
                                    newSecurityRule.SourceApplicationSecurityGroupAzureIds = securityRule.SourceApplicationSecurityGroupIds.Count > 0
                                        ? string.Join(",", securityRule.SourceApplicationSecurityGroupIds)
                                        : null;
                                    newSecurityRule.SourcePortRange = securityRule.SourcePortRange;
                                    newSecurityRule.SourcePortRanges = securityRule.SourcePortRanges.Count > 0
                                        ? string.Join(",", securityRule.SourcePortRanges)
                                        : null;

                                    securityRules.Add(newSecurityRule);
                                }
                                catch (Exception ex) {
                                    Log.Error($"An exception occurred getting details for {InventoryType} security rule: {securityRule.Inner?.Id}", ex);
                                }
                            }

                            newNetworkSecurityGroup.SecurityRules = securityRules;
                            inventory.Add(newNetworkSecurityGroup);
                        }
                        catch (Exception ex) {
                            Log.Error($"An exception occurred getting details for {InventoryType}: {networkSecurityGroup.Id}", ex);
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

        public override async Task<IEnumerable<NetworkSecurityGroupModel>> ProcessInventoryAsync(IEnumerable<NetworkSecurityGroupModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var itemsToInsert = new List<NetworkSecurityGroupModel>();
            var itemsToUpdate = new List<NetworkSecurityGroupModel>();

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

            foreach (var networkSecurityGroup in inventory) {
                foreach (var securityRule in networkSecurityGroup.SecurityRules) {
                    securityRule.NetworkSecurityGroupId = networkSecurityGroup.Id;
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