using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.Models;
using Microsoft.Azure.Management.RecoveryServices.Backup.Models;

namespace CalpineAzureDashboard.DataCollector.Inventory
{
    class VaultBackupJobInventory : AzureInventory<VaultBackupJobModel>
    {
        public VaultBackupJobInventory(AzureService azureService, IRepository<VaultBackupJobModel> repository) : 
            base(azureService, repository, "vault backup job", null) { }

        public override async Task<IEnumerable<VaultBackupJobModel>> GetInventoryAsync(IEnumerable<VaultBackupJobModel> inventoryParam = null)
        {
            if (inventoryParam != null) {
                return inventoryParam;
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s started at: {DateTime.Now}");
            var inventory = new List<VaultBackupJobModel>();

            var query = "SELECT TOP 1 * FROM [VaultBackupJob] ORDER BY [EndTimeUtc] DESC";
            var latestActivityLogInDb = await Repository.Get(query);;
            
            //always go back at least 7 days to update any statuses that had an InProgress status when the backup was run
            var startDateTimeUtc = latestActivityLogInDb?.EndTimeUtc;
            if (startDateTimeUtc == null) {
                startDateTimeUtc = DateTime.UtcNow.AddDays(-60);
            }
            else if (startDateTimeUtc > DateTime.UtcNow.AddDays(-7)) {
                startDateTimeUtc = DateTime.UtcNow.AddDays(-7);
            }
            var endDateTimeUtc = DateTime.UtcNow;

            foreach (var subscription in Subscriptions) {
                
                try {
                    var vaults = await AzureService.GetRecoveryServicesVaultsAsync(subscription.SubscriptionId);

                    foreach (var vault in vaults) {
                        try {
                            var resourceGroup = vault.Id.Split('/')[4];
                            var jobs = await AzureService.GetBackupJobsAsync(subscription.SubscriptionId, vault.Name, resourceGroup, startDateTimeUtc.Value,
                                endDateTimeUtc);

                            foreach (var job in jobs) {
                                try {
                                    var newBackupJob = new VaultBackupJobModel();
                                    newBackupJob.SubscriptionId = subscription.SubscriptionId;
                                    newBackupJob.Subscription = subscription.DisplayName;
                                    newBackupJob.ResourceGroup = resourceGroup;
                                    newBackupJob.VaultName = vault.Name;
                                    newBackupJob.Name = job.Name;
                                    newBackupJob.AzureId = job.Id;
                                    newBackupJob.ActivityId = job.Properties.ActivityId;
                                    newBackupJob.BackupManagementType = job.Properties.BackupManagementType;
                                    newBackupJob.EndTimeUtc = job.Properties.EndTime;
                                    newBackupJob.EntityFriendlyName = job.Properties.EntityFriendlyName;
                                    newBackupJob.Operation = job.Properties.Operation;
                                    newBackupJob.StartTimeUtc = job.Properties.StartTime;
                                    newBackupJob.Status = job.Properties.Status;

                                    if (job.Properties.GetType().Name.Equals("AzureIaaSVMJob")) {
                                        var properties = (AzureIaaSVMJob) job.Properties;
                                        if (properties.ErrorDetails?.Count > 1) {
                                            throw new Exception("Encountered more than one ErrorDetails object. Please add support in Azure dashboard.");
                                        }

                                        if (properties.ExtendedInfo != null) {
                                            throw new Exception("Encountered unsupported ExtendedInfo job property. Please add support in Azure dashboard. ");
                                        }

                                        newBackupJob.Duration = properties.Duration;

                                        var errorDetails = properties.ErrorDetails?[0];
                                        if (errorDetails != null) {
                                            newBackupJob.ErrorCode = errorDetails.ErrorCode;
                                            newBackupJob.ErrorString = errorDetails.ErrorString;
                                            newBackupJob.ErrorTitle = errorDetails.ErrorTitle;

                                            if (errorDetails.Recommendations != null && errorDetails.Recommendations.Count > 1) {
                                                throw new Exception(
                                                    $"Encountered {errorDetails.Recommendations.Count} recommendations in the first error details object. " +
                                                    "Expected only one. Please add support in Azure dashboard.");
                                            }

                                            newBackupJob.ErrorRecommendation = errorDetails.Recommendations.FirstOrDefault();
                                        }

                                        if (properties.ExtendedInfo != null) {
                                            throw new Exception("Encountered non-null ExtendedInfo object. Please review and add support in Azure dashboard.");
                                        }

                                        newBackupJob.VirtualMachineVersion = properties.VirtualMachineVersion;

                                        inventory.Add(newBackupJob);
                                    }
                                    else if (job.Properties.GetType().Name.Equals("MabJob")) {
                                        var properties = (MabJob) job.Properties;
                                        if (properties.ErrorDetails?.Count > 1) {
                                            throw new Exception("Encountered more than one ErrorDetails object. Please add support in Azure dashboard.");
                                        }

                                        if (properties.ExtendedInfo != null) {
                                            throw new Exception("Encountered unsupported ExtendedInfo job property. Please add support in Azure dashboard. ");
                                        }

                                        newBackupJob.Duration = properties.Duration;

                                        var errorDetails = properties.ErrorDetails?[0];
                                        if (errorDetails != null) {
                                            newBackupJob.ErrorString = errorDetails.ErrorString;

                                            if (errorDetails.Recommendations != null && errorDetails.Recommendations.Count > 1) {
                                                throw new Exception(
                                                    $"Encountered {errorDetails.Recommendations.Count} recommendations in the first error details object. " +
                                                    "Expected only one. Please add support in Azure dashboard.");
                                            }

                                            newBackupJob.ErrorRecommendation = errorDetails.Recommendations.FirstOrDefault();
                                        }

                                        if (properties.ExtendedInfo != null) {
                                            throw new Exception("Encountered non-null ExtendedInfo object. Please review and add support in Azure dashboard.");
                                        }

                                        newBackupJob.MabServerName = properties.MabServerName;
                                        newBackupJob.MabServerType = properties.MabServerType;
                                        newBackupJob.WorkloadType = properties.WorkloadType;

                                        inventory.Add(newBackupJob);
                                    }
                                    else if (job.Properties.GetType().Name.Equals("AzureWorkloadJob")) {
                                        var properties = (AzureWorkloadJob) job.Properties;

                                        if (properties.ErrorDetails?.Count > 1) {
                                            throw new Exception("Encountered more than one ErrorDetails object. Please add support in Azure dashboard.");
                                        }

                                        if (properties.ExtendedInfo != null) {
                                            throw new Exception("Encountered non-null ExtendedInfo object. Please review and add support in Azure dashboard.");
                                        }

                                        newBackupJob.Duration = properties.Duration;

                                        var errorDetails = properties.ErrorDetails?[0];
                                        if (errorDetails != null) {
                                            newBackupJob.ErrorCode = errorDetails.ErrorCode;
                                            newBackupJob.ErrorString = errorDetails.ErrorString;
                                            newBackupJob.ErrorTitle = errorDetails.ErrorTitle;

                                            if (errorDetails.Recommendations != null && errorDetails.Recommendations.Count > 1) {
                                                throw new Exception(
                                                    $"Encountered {errorDetails.Recommendations.Count} recommendations in the first error details object. " +
                                                    "Expected only one. Please add support in Azure dashboard.");
                                            }

                                            newBackupJob.ErrorRecommendation = errorDetails.Recommendations.FirstOrDefault();
                                        }

                                        newBackupJob.WorkloadType = properties.WorkloadType;

                                        inventory.Add(newBackupJob);
                                    }
                                    else {
                                        throw new Exception($"Encountered unsupported backup job properties type: {job.Properties.GetType().FullName}. " +
                                                            "Please add support in Azure dashboard. ");
                                    }
                                }
                                catch (Exception ex) {
                                    Log.Error($"An exception occurred getting details for backup job with id {job.Id}", ex);
                                }
                            }
                        }
                        catch (Exception ex) {
                            Log.Error($"An exception occurred getting details for vault with id {vault.Id}", ex);
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

        public override async Task<IEnumerable<VaultBackupJobModel>> ProcessInventoryAsync(IEnumerable<VaultBackupJobModel> inventoryParam)
        {
            var inventory = inventoryParam.GroupBy(x => x.AzureId).Select(x => x.OrderByDescending(y => y.StartTimeUtc).FirstOrDefault()).ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var itemsToInsert = new List<VaultBackupJobModel>();
            var itemsToUpdate = new List<VaultBackupJobModel>();

            var existingInventory = (await Repository.GetCollectionAsync()).ToList();

            Log.Debug($"Processing Azure inventory for {inventory.Count} {InventoryType}s started at: {DateTime.Now}");
            foreach (var item in inventory) {
                try {
                    if (existingInventory.Count.Equals(0)) {
                        itemsToInsert.Add(item);
                        continue;
                    }

                    var existingItem = existingInventory.Where(x => x.AzureId.Equals(item.AzureId, StringComparison.OrdinalIgnoreCase)).OrderByDescending(x => x.StartTimeUtc).ThenBy(x => x.EndTimeUtc).FirstOrDefault();

                    if (existingItem == null) {
                        //get the first "Create or Update" event to determine who created the resource and when
                        try {
                            var createdEvent = GetCreatedEvent(item.SubscriptionId, item.ResourceGroup, item.AzureId);
                            item.CreatedBy = createdEvent?.Caller;
                            item.CreatedDtUtc = createdEvent?.EventTimestamp;
                        }
                        catch (Exception ex) {
                            Log.Error($"Something went wrong getting created event from the activity logs for {InventoryType} with Azure id: {item.AzureId}.",
                                ex);
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

            //var itemsToDelete = existingInventory.Where(x => !inventory.Any(y => y.AzureId.Equals(x.AzureId, StringComparison.OrdinalIgnoreCase)));

            //var recordsDeleted = await DeleteInventoryAsync(itemsToDelete);
            var recordsUpdated = await UpdateInventoryAsync(itemsToUpdate);
            var recordsInserted = await InsertInventoryAsync(itemsToInsert);

            Log.Debug($"  Number of {InventoryType} records inserted: {recordsInserted}");
            Log.Debug($"  Number of {InventoryType} records updated: {recordsUpdated}");
            //Log.Debug($"  Number of {InventoryType} records deleted: {recordsDeleted}");

            Log.Debug($"Processing Azure inventory for {inventory.Count} {InventoryType}s finished at: {DateTime.Now}");
            return inventory;
        }
    }
}