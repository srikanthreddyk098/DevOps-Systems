using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.Models;
using Microsoft.Azure.Management.Graph.RBAC.Fluent;

namespace CalpineAzureDashboard.DataCollector.Inventory
{
    public class AdRoleDefinitionInventory : AzureInventory<AdRoleDefinitionModel>
    {
        public AdRoleDefinitionInventory(AzureService azureService, IRepository<AdRoleDefinitionModel> repository) : 
            base(azureService, repository, "AD role definition", null) { }

        public override async Task<IEnumerable<AdRoleDefinitionModel>> GetInventoryAsync(IEnumerable<AdRoleDefinitionModel> inventoryParam = null)
        {
            if (inventoryParam != null) {
                return inventoryParam;
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s started at: {DateTime.Now}");
            var inventory = new List<AdRoleDefinitionModel>();

            foreach (var subscription in Subscriptions) {
                try {
                    var roleDefinitions = await AzureService.GetAzureAdSubscriptionRoleDefinitionsAsync(subscription.SubscriptionId);

                    foreach (var roleDefinition in roleDefinitions) {
                        try {
                            if (roleDefinition.GetType().Name.Equals("RoleDefinitionImpl")) {
                                var role = (RoleDefinitionImpl) roleDefinition;

                                var newRoleDefinition = new AdRoleDefinitionModel();
                                newRoleDefinition.AzureId = role.Inner.Id;
                                newRoleDefinition.Name = role.Inner.Name;
                                newRoleDefinition.FriendlyName = role.Inner.RoleName;
                                newRoleDefinition.Description = role.Inner.Description;
                                newRoleDefinition.Type = role.Inner.RoleType;
                                newRoleDefinition.Scopes = role.Inner.AssignableScopes.Any() ? string.Join(",", role.Inner.AssignableScopes) : null;

                                if (role.Inner.Permissions.Count > 1) {
                                    throw new Exception($"Role definition with name '{role.Inner.Name}' has more than one Permissions object. " +
                                                        "Currently only one permission object is expected. Please add support in Azure dashboard.");
                                }

                                if (role.Inner.Permissions.Count == 1) {
                                    var permissions = role.Inner.Permissions.First();
                                    newRoleDefinition.Actions = permissions.Actions.Any() ? string.Join(",", permissions.Actions) : null;
                                    newRoleDefinition.NotActions = permissions.NotActions.Any() ? string.Join(",", permissions.NotActions) : null;
                                    newRoleDefinition.DataActions = permissions.DataActions.Any() ? string.Join(",", permissions.DataActions) : null;
                                    newRoleDefinition.NotDataActions = permissions.NotDataActions.Any() ? string.Join(",", permissions.NotDataActions) : null;
                                }

                                inventory.Add(newRoleDefinition);
                            }
                            else {
                                throw new
                                    Exception(
                                        $"Encountered unsupported AD role definition type: {roleDefinition.GetType()}. Please add support in Azure dashboard.");
                            }
                        }
                        catch (Exception ex) {
                            Log.Error($"An exception occurred getting details for {InventoryType} with Azure id: {roleDefinition.Id}", ex);
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

        public override async Task<IEnumerable<AdRoleDefinitionModel>> ProcessInventoryAsync(IEnumerable<AdRoleDefinitionModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var itemsToInsert = new List<AdRoleDefinitionModel>();
            var itemsToUpdate = new List<AdRoleDefinitionModel>();

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