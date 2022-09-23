using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models
{
    [Table("VaultPolicy")]
    public class VaultPolicyModel : AzureModel
    {
        public string SubscriptionId { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string VaultName { get; set; }
        public string Name { get; set; }
        public int? ProtectedItemsCount { get; set; }
        public string TimeZone { get; set; }
        public int? InstantRpRetentionRangeInDays { get; set; }
        public bool? IsCompression { get; set; }
        public bool? IsSqlCompression { get; set; }
        public string WorkloadType { get; set; }

        public string DailyRetentionDurationType { get; set; }
        public int? DailyRetentionDurationCount { get; set; }
        public DateTime? DailyRetentionDurationTime { get; set; }

        public string WeeklyRetentionDurationType { get; set; }
        public int? WeeklyRetentionDurationCount { get; set; }
        public string WeeklyRetentionDaysOfTheWeek { get; set; }
        public DateTime? WeeklyRetentionDurationTime { get; set; }

        public string MonthlyRetentionFormatType { get; set; }
        public string MonthlyRetentionDurationType { get; set; }
        public int? MonthlyRetentionDurationCount { get; set; }
        public string MonthlyRetentionDaysOfTheWeek { get; set; }
        public string MonthlyRetentionWeeksOfTheMonth { get; set; }
        public string MonthlyRetentionDaysOfTheMonth { get; set; }
        public DateTime? MonthlyRetentionDurationTime { get; set; }

        public string YearlyRetentionFormatType { get; set; }
        public string YearlyRetentionDurationType { get; set; }
        public int? YearlyRetentionDurationCount { get; set; }
        public string YearlyRetentionDaysOfTheWeek { get; set; }
        public string YearlyRetentionWeeksOfTheMonth { get; set; }
        public string YearlyRetentionDaysOfTheMonth { get; set; }
        public string YearlyRetentionMonthsOfTheYear { get; set; }
        public DateTime? YearlyRetentionDurationTime { get; set; }

        public string ScheduleRunDays { get; set; }
        public string ScheduleRunFrequency { get; set; }
        public DateTime? ScheduleRunTime { get; set; }
        public int? ScheduleWeeklyFrequency { get; set; }

        public string LogRetentionDurationType { get; set; }
        public int? LogRetentionDurationCount { get; set; }

        public int? LogScheduleFrequencyInMins { get; set; }

        public string CreatedBy { get; set; }
        public DateTime? CreatedDtUtc { get; set; }

        public bool IsEqual(VaultPolicyModel policy)
        {
            return string.Equals(AzureId, policy.AzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SubscriptionId, policy.SubscriptionId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Subscription, policy.Subscription, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ResourceGroup, policy.ResourceGroup, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(VaultName, policy.VaultName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Name, policy.Name, StringComparison.OrdinalIgnoreCase) &&
                   ProtectedItemsCount == policy.ProtectedItemsCount &&
                   string.Equals(TimeZone, policy.TimeZone, StringComparison.OrdinalIgnoreCase) &&
                   InstantRpRetentionRangeInDays == policy.InstantRpRetentionRangeInDays &&
                   IsCompression == policy.IsCompression &&
                   IsSqlCompression == policy.IsSqlCompression &&
                   string.Equals(WorkloadType, policy.WorkloadType, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(DailyRetentionDurationType, policy.DailyRetentionDurationType, StringComparison.OrdinalIgnoreCase) &&
                   DailyRetentionDurationCount == policy.DailyRetentionDurationCount &&
                   IsDateEqual(DailyRetentionDurationTime, policy.DailyRetentionDurationTime) &&
                   string.Equals(WeeklyRetentionDurationType, policy.WeeklyRetentionDurationType, StringComparison.OrdinalIgnoreCase) &&
                   WeeklyRetentionDurationCount == policy.WeeklyRetentionDurationCount &&
                   string.Equals(WeeklyRetentionDaysOfTheWeek, policy.WeeklyRetentionDaysOfTheWeek, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(WeeklyRetentionDurationTime, policy.WeeklyRetentionDurationTime) &&
                   string.Equals(MonthlyRetentionFormatType, policy.MonthlyRetentionFormatType, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(MonthlyRetentionDurationType, policy.MonthlyRetentionDurationType, StringComparison.OrdinalIgnoreCase) &&
                   MonthlyRetentionDurationCount == policy.MonthlyRetentionDurationCount &&
                   string.Equals(MonthlyRetentionDaysOfTheWeek, policy.MonthlyRetentionDaysOfTheWeek, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(MonthlyRetentionWeeksOfTheMonth, policy.MonthlyRetentionWeeksOfTheMonth, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(MonthlyRetentionDaysOfTheMonth, policy.MonthlyRetentionDaysOfTheMonth, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(MonthlyRetentionDurationTime, policy.MonthlyRetentionDurationTime) &&
                   string.Equals(YearlyRetentionFormatType, policy.YearlyRetentionFormatType, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(YearlyRetentionDurationType, policy.YearlyRetentionDurationType, StringComparison.OrdinalIgnoreCase) &&
                   YearlyRetentionDurationCount == policy.YearlyRetentionDurationCount &&
                   string.Equals(YearlyRetentionDaysOfTheWeek, policy.YearlyRetentionDaysOfTheWeek, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(YearlyRetentionWeeksOfTheMonth, policy.YearlyRetentionWeeksOfTheMonth, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(YearlyRetentionDaysOfTheMonth, policy.YearlyRetentionDaysOfTheMonth, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(YearlyRetentionMonthsOfTheYear, policy.YearlyRetentionMonthsOfTheYear, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(YearlyRetentionDurationTime, policy.YearlyRetentionDurationTime) &&
                   string.Equals(ScheduleRunDays, policy.ScheduleRunDays, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ScheduleRunFrequency, policy.ScheduleRunFrequency, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(ScheduleRunTime, policy.ScheduleRunTime) &&
                   ScheduleWeeklyFrequency == policy.ScheduleWeeklyFrequency &&
                   string.Equals(LogRetentionDurationType, policy.LogRetentionDurationType, StringComparison.OrdinalIgnoreCase) &&
                   LogRetentionDurationCount == policy.LogRetentionDurationCount &&
                   LogScheduleFrequencyInMins == policy.LogScheduleFrequencyInMins &&
                   string.Equals(CreatedBy, policy.CreatedBy, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(CreatedDtUtc, policy.CreatedDtUtc);
        }
    }
}