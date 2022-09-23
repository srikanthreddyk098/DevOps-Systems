using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureDashboard.Azure;
using AzureDashboard.Data.Repository;
using AzureDashboard.Models;
using Microsoft.Azure.Management.RecoveryServices.Backup.Models;
using DayOfWeek = Microsoft.Azure.Management.RecoveryServices.Backup.Models.DayOfWeek;

namespace AzureDashboard.Console.Inventory
{
    class VaultPolicyInventory : AzureInventory<VaultPolicyModel>
    {
        public VaultPolicyInventory(AzureService azureService, IRepository<VaultPolicyModel> repository) : base(azureService, repository, "backup policy",
            "Create Protection Policy")
        {
        }

        public async override Task<IEnumerable<VaultPolicyModel>> GetInventoryAsync(
            IEnumerable<VaultPolicyModel> inventoryParam = null)
        {
            if (inventoryParam != null) {
                return inventoryParam;
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s started at: {DateTime.Now}");
            var inventory = new List<VaultPolicyModel>();

            foreach (var subscription in Subscriptions) {
                try {
                    var vaults = await AzureService.GetBackupVaultsAsync(subscription.SubscriptionId);

                    foreach (var vault in vaults) {
                        try {
                            var resourceGroup = vault.Id.Split('/')[4];
                            var policies = await AzureService.GetBackupPoliciesAsync(subscription.SubscriptionId,
                                vault.Name,
                                resourceGroup);

                            foreach (var policy in policies) {
                                try {
                                    var newPolicy = new VaultPolicyModel();
                                    newPolicy.AzureId = policy.Id;
                                    newPolicy.SubscriptionId = subscription.SubscriptionId;
                                    newPolicy.Subscription = subscription.DisplayName;
                                    newPolicy.ResourceGroup = resourceGroup;
                                    newPolicy.VaultName = vault.Name;
                                    newPolicy.Name = policy.Name;
                                    newPolicy.ProtectedItemsCount = policy.Properties.ProtectedItemsCount;

                                    if (policy.Properties.GetType().Name == "AzureIaaSVMProtectionPolicy") {
                                        var properties = (AzureIaaSVMProtectionPolicy) policy.Properties;

                                        newPolicy.Name = policy.Name;
                                        newPolicy.AzureId = policy.Id;
                                        newPolicy.ProtectedItemsCount = properties.ProtectedItemsCount;
                                        newPolicy.TimeZone = properties.TimeZone;
                                        newPolicy.InstantRpRetentionRangeInDays =
                                            properties.InstantRpRetentionRangeInDays;

                                        SetLongTermRetentionPolicyProperties((LongTermRetentionPolicy) properties.RetentionPolicy, newPolicy);
                                        SetSimpleSchedulePolicyProperties((SimpleSchedulePolicy) properties.SchedulePolicy, newPolicy);

                                        inventory.Add(newPolicy);
                                    }
                                    else if (policy.Properties.GetType().Name == "AzureVmWorkloadProtectionPolicy") {
                                        var properties = (AzureVmWorkloadProtectionPolicy) policy.Properties;

                                        if (properties.WorkLoadType != "SQLDataBase") {
                                            throw new Exception($"Encountered unsupported WorkLoadType: {properties.WorkLoadType}. " +
                                                                "Please review and add support in Azure dashboard.");
                                        }

                                        if (properties.SubProtectionPolicy.Count != 2) {
                                            throw new Exception($"Encountered {properties.SubProtectionPolicy.Count} SubProtection policies. Expected only two. " +
                                                                "Please review and add support in Azure dashboard.");
                                        }

                                        if (properties.SubProtectionPolicy[0].PolicyType != "Full") {
                                            throw new Exception($"Encountered '{properties.SubProtectionPolicy[0].PolicyType}' as the first SubProtectionPolicy type. " +
                                                                "Expected 'Full'. Please review and add support in Azure dashboard.");
                                        }

                                        if (properties.SubProtectionPolicy[1].PolicyType != "Log") {
                                            throw new Exception($"Encountered '{properties.SubProtectionPolicy[1].PolicyType}' as the second SubProtectionPolicy type. " +
                                                                "Expected 'Log'. Please review and add support in Azure dashboard.");
                                        }

                                        newPolicy.TimeZone = properties.Settings.TimeZone;
                                        newPolicy.IsCompression = properties.Settings.IsCompression;
                                        newPolicy.IsSqlCompression = properties.Settings.Issqlcompression;
                                        newPolicy.WorkloadType = properties.WorkLoadType;

                                        var sqlFullPolicy = properties.SubProtectionPolicy[0];
                                        SetLongTermRetentionPolicyProperties((LongTermRetentionPolicy) sqlFullPolicy.RetentionPolicy, newPolicy);
                                        SetSimpleSchedulePolicyProperties((SimpleSchedulePolicy) sqlFullPolicy.SchedulePolicy, newPolicy);

                                        var sqlLogPolicy = properties.SubProtectionPolicy[1];
                                        SetSimpleRetentionPolicyProperties((SimpleRetentionPolicy) sqlLogPolicy.RetentionPolicy, newPolicy);
                                        SetLogSchedulePolicyProperties((LogSchedulePolicy) sqlLogPolicy.SchedulePolicy, newPolicy);

                                        inventory.Add(newPolicy);
                                    }
                                    else {
                                        throw new Exception($"Encountered unsupported policy properties type: {policy.Properties.GetType().FullName}. " +
                                                            "Please add support in Azure dashboard.");
                                    }
                                }
                                catch (Exception ex) {
                                    Log.Error($"An exception occurred getting details for policy with id {policy.Id}", ex);
                                }
                            }
                        }
                        catch (Exception ex) {
                            Log.Error($"An exception occurred getting details for vault with id {vault.Id}", ex);
                        }
                    }
                }
                catch (Exception ex) {
                    Log.Error($"An exception occurred getting {InventoryType} inventory for subscription: {subscription.DisplayName}", ex);
                }
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s finished at: {DateTime.Now}");
            return inventory;
        }

        public override async Task<IEnumerable<VaultPolicyModel>> ProcessInventoryAsync(
            IEnumerable<VaultPolicyModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var itemsToInsert = new List<VaultPolicyModel>();
            var itemsToUpdate = new List<VaultPolicyModel>();

            var existingInventory = (await Repository.GetCollectionAsync()).ToList();

            Log.Debug($"Processing Azure inventory for {inventory.Count} {InventoryType}s started at: {DateTime.Now}");
            foreach (var item in inventory) {
                try {
                    if (existingInventory.Count.Equals(0)) {
                        itemsToInsert.Add(item);
                        continue;
                    }

                    var existingitem =
                        existingInventory.FirstOrDefault(x => x.AzureId.Equals(item.AzureId));

                    if (existingitem == null) {
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
                        item.Id = existingitem.Id;
                        item.CreatedBy = existingitem.CreatedBy;
                        item.CreatedDtUtc = existingitem.CreatedDtUtc;

                        if (!item.IsEqual(existingitem)) {
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

            Log.Debug($"  Records inserted: {recordsInserted}");
            Log.Debug($"  Records updated: {recordsUpdated}");
            Log.Debug($"  Records deleted: {recordsDeleted}");

            Log.Debug($"Processing Azure inventory for {inventory.Count} {InventoryType}s finished at: {DateTime.Now}");
            return inventory;
        }

        private void SetLongTermRetentionPolicyProperties(LongTermRetentionPolicy retentionPolicy, VaultPolicyModel newPolicy)
        {
            if (retentionPolicy == null) {
                return;
            }

            if (retentionPolicy.DailySchedule?.RetentionTimes?.Count > 1) {
                throw new Exception($"Encountered {retentionPolicy.DailySchedule.RetentionTimes.Count} daily schedule retention times. " +
                                    $"Expected there to be only one. Please review and add support in Azure dashboard.");
            }

            if (retentionPolicy.WeeklySchedule?.RetentionTimes?.Count > 1) {
                throw new Exception($"Encountered {retentionPolicy.WeeklySchedule.RetentionTimes.Count} weekly schedule retention times. " +
                                    $"Expected there to be only one. Please review and add support in Azure dashboard.");
            }

            if (retentionPolicy.MonthlySchedule?.RetentionTimes?.Count > 1) {
                throw new Exception($"Encountered {retentionPolicy.MonthlySchedule.RetentionTimes.Count} monthly schedule retention times. " +
                                    $"Expected there to be only one. Please review and add support in Azure dashboard.");
            }

            if (retentionPolicy.YearlySchedule?.RetentionTimes?.Count > 1) {
                throw new Exception($"Encountered {retentionPolicy.YearlySchedule.RetentionTimes.Count} yearly schedule retention times. " +
                                    $"Expected there to be only one. Please review and add support in Azure dashboard.");
            }

            if (retentionPolicy.DailySchedule != null) {
                newPolicy.DailyRetentionDurationType = retentionPolicy.DailySchedule.RetentionDuration?.DurationType;
                newPolicy.DailyRetentionDurationCount = retentionPolicy.DailySchedule.RetentionDuration?.Count;
                newPolicy.DailyRetentionDurationTime = retentionPolicy.DailySchedule.RetentionTimes?.FirstOrDefault();
            }

            if (retentionPolicy.WeeklySchedule != null) {
                newPolicy.WeeklyRetentionDurationType = retentionPolicy.WeeklySchedule.RetentionDuration?.DurationType;
                newPolicy.WeeklyRetentionDurationCount = retentionPolicy.WeeklySchedule.RetentionDuration?.Count;
                newPolicy.WeeklyRetentionDaysOfTheWeek = string.Join(", ", retentionPolicy.WeeklySchedule.DaysOfTheWeek);
                newPolicy.WeeklyRetentionDurationTime = retentionPolicy.WeeklySchedule.RetentionTimes?.FirstOrDefault();
            }

            if (retentionPolicy.MonthlySchedule != null) {
                newPolicy.MonthlyRetentionFormatType = retentionPolicy.MonthlySchedule.RetentionScheduleFormatType;
                newPolicy.MonthlyRetentionDurationType = retentionPolicy.MonthlySchedule.RetentionDuration?.DurationType;
                newPolicy.MonthlyRetentionDurationCount = retentionPolicy.MonthlySchedule.RetentionDuration?.Count;
                if (retentionPolicy.MonthlySchedule?.RetentionScheduleWeekly != null) {
                    newPolicy.MonthlyRetentionDaysOfTheWeek = string.Join(", ", retentionPolicy.MonthlySchedule.RetentionScheduleWeekly.DaysOfTheWeek);
                    newPolicy.MonthlyRetentionWeeksOfTheMonth = string.Join(", ", retentionPolicy.MonthlySchedule.RetentionScheduleWeekly.WeeksOfTheMonth);
                }

                if (retentionPolicy.MonthlySchedule?.RetentionScheduleDaily != null) {
                    if (retentionPolicy.MonthlySchedule.RetentionScheduleDaily.DaysOfTheMonth.Count > 1) {
                        throw new Exception($"Encountered {retentionPolicy.MonthlySchedule.RetentionScheduleDaily.DaysOfTheMonth.Count} monthly schedule days of the month. " +
                                            $"Expected there to be only one. Please review and add support in Azure dashboard.");
                    }

                    newPolicy.MonthlyRetentionDaysOfTheMonth = retentionPolicy.MonthlySchedule.RetentionScheduleDaily.DaysOfTheMonth.FirstOrDefault()?.Date?.ToString();
                }

                newPolicy.MonthlyRetentionDurationTime = retentionPolicy.MonthlySchedule.RetentionTimes?.FirstOrDefault();
            }

            if (retentionPolicy.YearlySchedule != null) {
                newPolicy.YearlyRetentionFormatType = retentionPolicy.YearlySchedule.RetentionScheduleFormatType;
                newPolicy.YearlyRetentionDurationType = retentionPolicy.YearlySchedule.RetentionDuration?.DurationType;
                newPolicy.YearlyRetentionDurationCount = retentionPolicy.YearlySchedule.RetentionDuration?.Count;
                if (retentionPolicy.YearlySchedule.RetentionScheduleWeekly != null) {
                    newPolicy.YearlyRetentionDaysOfTheWeek = string.Join(", ", retentionPolicy.YearlySchedule.RetentionScheduleWeekly.DaysOfTheWeek);
                    newPolicy.YearlyRetentionWeeksOfTheMonth = string.Join(", ", retentionPolicy.YearlySchedule.RetentionScheduleWeekly.WeeksOfTheMonth);
                }

                if (retentionPolicy.YearlySchedule?.RetentionScheduleDaily != null) {
                    newPolicy.YearlyRetentionDaysOfTheMonth = string.Join(", ", retentionPolicy.YearlySchedule.RetentionScheduleDaily.DaysOfTheMonth);
                }

                newPolicy.YearlyRetentionMonthsOfTheYear = string.Join(", ", retentionPolicy.YearlySchedule.MonthsOfYear);
                newPolicy.YearlyRetentionDurationTime = retentionPolicy.YearlySchedule.RetentionTimes?.FirstOrDefault();
            }
        }

        private void SetSimpleRetentionPolicyProperties(SimpleRetentionPolicy retentionPolicy, VaultPolicyModel newPolicy)
        {
            if (retentionPolicy == null) {
                return;
            }

            newPolicy.LogRetentionDurationType = retentionPolicy.RetentionDuration.DurationType;
            newPolicy.LogRetentionDurationCount = retentionPolicy.RetentionDuration.Count;
        }

        private void SetSimpleSchedulePolicyProperties(SimpleSchedulePolicy schedulePolicy, VaultPolicyModel newPolicy)
        {
            if (schedulePolicy == null) {
                return;
            }

            if (schedulePolicy.ScheduleRunDays != null) {
                newPolicy.ScheduleRunDays = string.Join(", ", schedulePolicy.ScheduleRunDays);
            }

            newPolicy.ScheduleRunFrequency = schedulePolicy.ScheduleRunFrequency;
            newPolicy.ScheduleRunTime = schedulePolicy.ScheduleRunTimes?.FirstOrDefault();
            newPolicy.ScheduleWeeklyFrequency = schedulePolicy.ScheduleWeeklyFrequency;
        }

        private void SetLogSchedulePolicyProperties(LogSchedulePolicy schedulePolicy, VaultPolicyModel newPolicy)
        {
            if (schedulePolicy == null) {
                return;
            }

            newPolicy.LogScheduleFrequencyInMins = schedulePolicy.ScheduleFrequencyInMins;
        }
    }
}