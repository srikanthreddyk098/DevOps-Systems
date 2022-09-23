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
    public class AdUserInventory : AzureInventory<AdUserModel>
    {
        public AdUserInventory(AzureService azureService, IRepository<AdUserModel> repository) : 
            base(azureService, repository, "AD user", null) { }

        public override async Task<IEnumerable<AdUserModel>> GetInventoryAsync(IEnumerable<AdUserModel> inventoryParam = null)
        {
            if (inventoryParam != null) {
                return inventoryParam;
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s started at: {DateTime.Now}");
            var inventory = new List<AdUserModel>();

            try {
                var adUsers = await AzureService.GetAzureAdUsersAsync();

                foreach (var adUser in adUsers) {
                    try {
                        var newAdUser = new AdUserModel();
                        newAdUser.AzureId = adUser.Id;
                        newAdUser.AccountEnabled = adUser.Inner.AccountEnabled;
                        newAdUser.AdditionalProperties = JsonConvert.SerializeObject(adUser.Inner.AdditionalProperties);
                        newAdUser.DeletionTimestamp = adUser.Inner.DeletionTimestamp;
                        newAdUser.DisplayName = adUser.Inner.DisplayName;
                        newAdUser.GivenName = adUser.Inner.GivenName;
                        newAdUser.Mail = adUser.Inner.Mail;
                        newAdUser.MailNickname = adUser.Inner.MailNickname;
                        newAdUser.ObjectId = adUser.Inner.ObjectId;
                        newAdUser.SignInNames = adUser.Inner.SignInNames?.Count > 0 ? string.Join(",", adUser.Inner.SignInNames.Select(x => x.Value)) : null;
                        newAdUser.Surname = adUser.Inner.Surname;
                        newAdUser.UsageLocation = adUser.Inner.UsageLocation;
                        newAdUser.UserPrincipalName = adUser.Inner.UserPrincipalName;
                        newAdUser.UserType = adUser.Inner.UserType.Value;

                        inventory.Add(newAdUser);
                    }
                    catch (Exception ex) {
                        Log.Error($"An exception occurred getting details for {InventoryType} with Azure id: {adUser.Id}", ex);
                    }
                }
            }
            catch (Exception ex) {
                throw new Exception($"An exception occurred getting {InventoryType} inventory.", ex);
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s finished at: {DateTime.Now}");
            return inventory;
        }

        public override async Task<IEnumerable<AdUserModel>> ProcessInventoryAsync(IEnumerable<AdUserModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var itemsToInsert = new List<AdUserModel>();
            var itemsToUpdate = new List<AdUserModel>();

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