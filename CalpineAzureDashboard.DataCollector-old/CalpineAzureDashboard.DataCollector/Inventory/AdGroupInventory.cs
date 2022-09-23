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
    public class AdGroupInventory : AzureInventory<AdGroupModel>
    {
        public AdGroupInventory(AzureService azureService, IRepository<AdGroupModel> repository) : 
            base(azureService, repository, "AD group", null) { }

        public override async Task<IEnumerable<AdGroupModel>> GetInventoryAsync(IEnumerable<AdGroupModel> inventoryParam = null)
        {
            if (inventoryParam != null) {
                return inventoryParam;
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s started at: {DateTime.Now}");
            var inventory = new List<AdGroupModel>();

            try {
                var adGroups = await AzureService.GetAzureAdGroupsAsync();

                foreach (var adGroup in adGroups) {
                    try {
                        var newAdGroup = new AdGroupModel();
                        newAdGroup.AzureId = adGroup.Inner.ObjectId;
                        newAdGroup.AdditionalProperties = JsonConvert.SerializeObject(adGroup.Inner.AdditionalProperties);
                        newAdGroup.DeletionTimestamp = adGroup.Inner.DeletionTimestamp;
                        newAdGroup.DisplayName = adGroup.Inner.DisplayName;
                        newAdGroup.Mail = adGroup.Inner.Mail;
                        newAdGroup.ObjectId = adGroup.Inner.ObjectId;
                        newAdGroup.SecurityEnabled = adGroup.Inner.SecurityEnabled;

                        var userObjectIds = new List<string>();
                        var groupObjectIds = new List<string>();
                        var servicePrincipalObjectIds = new List<string>();

                        var members = await adGroup.ListMembersAsync();
                        foreach (var member in members) {
                            if (member == null) continue;

                            try {
                                var type = member.GetType();
                                if (type.Name == "ActiveDirectoryUserImpl") {
                                    userObjectIds.Add(member.Id);
                                }
                                else if (type.Name == "ActiveDirectoryGroupImpl") {
                                    groupObjectIds.Add(member.Id);
                                }
                                else if (type.Name == "ServicePrincipalImpl") {
                                    servicePrincipalObjectIds.Add(member.Id);
                                }
                                else {
                                    Log.Error($"Found not supported object type for {InventoryType} {adGroup.Inner.DisplayName}: {type} ");
                                }
                            }
                            catch (Exception ex) {
                                Log.Error($"An exception occurred getting members for {InventoryType} with Azure id: {adGroup.Id}", ex);
                            }
                        }

                        newAdGroup.UserObjectIds = userObjectIds;
                        newAdGroup.GroupObjectIds = groupObjectIds;
                        newAdGroup.ServicePrincipalObjectIds = servicePrincipalObjectIds;
                        inventory.Add(newAdGroup);
                    }
                    catch (Exception ex) {
                        Log.Error($"An exception occurred getting details for {InventoryType} with Azure id: {adGroup.Id}", ex);
                    }
                }
            }
            catch (Exception ex) {
                throw new Exception($"An exception occurred getting {InventoryType} inventory.", ex);
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s finished at: {DateTime.Now}");
            return inventory;
        }

        public override async Task<IEnumerable<AdGroupModel>> ProcessInventoryAsync(IEnumerable<AdGroupModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var itemsToInsert = new List<AdGroupModel>();
            var itemsToUpdate = new List<AdGroupModel>();

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