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
    public class AdRoleAssignmentInventory : AzureInventory<AdRoleAssignmentModel>
    {
        public AdRoleAssignmentInventory(AzureService azureService, IRepository<AdRoleAssignmentModel> repository) : 
            base(azureService, repository, "AD role definition", null) { }

        public override async Task<IEnumerable<AdRoleAssignmentModel>> GetInventoryAsync(IEnumerable<AdRoleAssignmentModel> inventoryParam = null)
        {
            if (inventoryParam != null) {
                return inventoryParam;
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s started at: {DateTime.Now}");
            var inventory = new List<AdRoleAssignmentModel>();

            foreach (var subscription in Subscriptions) {
                try {
                    var roleAssignments = await AzureService.GetAzureAdSubscriptionRoleAssignmentsAsync(subscription.SubscriptionId);

                    foreach (var roleAssignment in roleAssignments) {
                        try {
                            if (roleAssignment.GetType().Name.Equals("RoleAssignmentImpl")) {
                                var role = (RoleAssignmentImpl) roleAssignment;

                                var newRoleAssigment = new AdRoleAssignmentModel();
                                newRoleAssigment.AzureId = role.Inner.Id;
                                newRoleAssigment.Name = role.Inner.Name;
                                newRoleAssigment.PrincipalId = role.Inner.PrincipalId;
                                newRoleAssigment.RoleDefinitionId = role.Inner.RoleDefinitionId;
                                newRoleAssigment.Scope = role.Inner.Scope;
                                newRoleAssigment.CanDelegate = role.Inner.CanDelegate;

                                inventory.Add(newRoleAssigment);
                            }
                            else {
                                throw new Exception(
                                    $"Encountered unsupported AD role assignment type: {roleAssignment.GetType()}. Please add support in Azure dashboard.");
                            }
                        }
                        catch (Exception ex) {
                            Log.Error($"An exception occurred getting details for {InventoryType} with Azure id: {roleAssignment.Id}", ex);
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

        public override async Task<IEnumerable<AdRoleAssignmentModel>> ProcessInventoryAsync(IEnumerable<AdRoleAssignmentModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var itemsToInsert = new List<AdRoleAssignmentModel>();
            var itemsToUpdate = new List<AdRoleAssignmentModel>();

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