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
    class AdApplicationInventory : AzureInventory<AdApplicationModel>
    {
        public AdApplicationInventory(AzureService azureService, IRepository<AdApplicationModel> repository) : 
            base(azureService, repository, "AD application", null) { }

        public override async Task<IEnumerable<AdApplicationModel>> GetInventoryAsync(IEnumerable<AdApplicationModel> inventoryParam = null)
        {
            if (inventoryParam != null) {
                return inventoryParam;
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s started at: {DateTime.Now}");
            var inventory = new List<AdApplicationModel>();

            try {
                var adApplications = await AzureService.GetAzureAdApplicationsAsync();

                foreach (var adApplication in adApplications) {
                    try {
                        var newAdApplication = new AdApplicationModel();
                        newAdApplication.AzureId = adApplication.Id;
                        newAdApplication.Name = adApplication.Inner.DisplayName;
                        newAdApplication.AdditionalProperties = JsonConvert.SerializeObject(adApplication.Inner.AdditionalProperties);
                        newAdApplication.ApplicationId = adApplication.Inner.AppId;
                        newAdApplication.ApplicationPermissions = JsonConvert.SerializeObject(adApplication.Inner.AppPermissions);
                        newAdApplication.AvailableToOtherTenants = adApplication.Inner.AvailableToOtherTenants;
                        newAdApplication.DeletionTimestamp = adApplication.Inner.DeletionTimestamp;
                        newAdApplication.Homepage = adApplication.Inner.Homepage;
                        newAdApplication.IdentifierUris = adApplication.Inner.IdentifierUris.FirstOrDefault();
                        newAdApplication.Oauth2AllowImplicitFlow = adApplication.Inner.Oauth2AllowImplicitFlow;
                        newAdApplication.ObjectId = adApplication.Inner.ObjectId;
                        newAdApplication.ReplyUrls = string.Join(",", adApplication.Inner.ReplyUrls);

                        inventory.Add(newAdApplication);
                    }
                    catch (Exception ex) {
                        Log.Error($"An exception occurred getting details for {InventoryType} with id {adApplication.Id}", ex);
                    }
                }
            }
            catch (Exception ex) {
                throw new Exception($"An exception occurred getting {InventoryType} inventory.", ex);
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s finished at: {DateTime.Now}");
            return inventory;
        }

        public override async Task<IEnumerable<AdApplicationModel>> ProcessInventoryAsync(IEnumerable<AdApplicationModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var itemsToInsert = new List<AdApplicationModel>();
            var itemsToUpdate = new List<AdApplicationModel>();

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