using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models
{
    [Table("VaultBackupJob")]
    public class VaultBackupJobModel : AzureModel
    {
        public string SubscriptionId { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string VaultName { get; set; }
        public string Name { get; set; }
        public string ActivityId { get; set; }
        public string BackupManagementType { get; set; }
        public TimeSpan? Duration { get; set; }
        public DateTime? EndTimeUtc { get; set; }
        public string EntityFriendlyName { get; set; }
        public int? ErrorCode { get; set; }
        public string ErrorString { get; set; }
        public string ErrorTitle { get; set; }
        public string ErrorRecommendation { get; set; }
        public string MabServerName { get; set; }
        public string MabServerType { get; set; }
        public string Operation { get; set; }
        public DateTime? StartTimeUtc { get; set; }
        public string Status { get; set; }
        public string VirtualMachineVersion { get; set; }
        public string WorkloadType { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDtUtc { get; set; }

        public bool IsEqual(VaultBackupJobModel backupJob)
        {
            return string.Equals(AzureId, backupJob.AzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SubscriptionId, backupJob.SubscriptionId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Subscription, backupJob.Subscription, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ResourceGroup, backupJob.ResourceGroup, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(VaultName, backupJob.VaultName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Name, backupJob.Name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ActivityId, backupJob.ActivityId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(BackupManagementType, backupJob.BackupManagementType, StringComparison.OrdinalIgnoreCase) &&
                   //Duration == backupJob.Duration &&
                   IsDateEqual(EndTimeUtc, backupJob.EndTimeUtc) &&
                   string.Equals(EntityFriendlyName, backupJob.EntityFriendlyName, StringComparison.OrdinalIgnoreCase) &&
                   ErrorCode == backupJob.ErrorCode &&
                   string.Equals(ErrorString, backupJob.ErrorString, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ErrorTitle, backupJob.ErrorTitle, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ErrorRecommendation, backupJob.ErrorRecommendation, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(MabServerName, backupJob.MabServerName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(MabServerType, backupJob.MabServerType, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Operation, backupJob.Operation, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(StartTimeUtc, backupJob.StartTimeUtc) &&
                   string.Equals(Status, backupJob.Status, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(VirtualMachineVersion, backupJob.VirtualMachineVersion, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(WorkloadType, backupJob.WorkloadType, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(CreatedBy, backupJob.CreatedBy, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(CreatedDtUtc, backupJob.CreatedDtUtc);
        }
    }
}