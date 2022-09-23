using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models
{
    [Table("VaultBackup")]
    public class VaultBackupModel : AzureModel
    {
        public string SubscriptionId { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string VaultName { get; set; }
        public string Name { get; set; }
        public string BackupManagementType { get; set; }
        public string BackupSetName { get; set; }
        public string ComputerName { get; set; }
        public string ContainerName { get; set; }
        public string CreateMode { get; set; }
        public DateTime? DeferredDeleteSyncTimeInUtc { get; set; }
        public string ExtendedInfo { get; set; }
        public string FriendlyName { get; set; }
        public bool? IsScheduledForDeferredDelete { get; set; }
        public int? HealthDetailsCount { get; set; }
        public int? HealthCode { get; set; }
        public string HealthMessage { get; set; }
        public int? HealthRecommendationsCount { get; set; }
        public string HealthStatus { get; set; }
        public string LastBackupStatus { get; set; }
        public DateTime? LastBackupTimeUtc { get; set; }
        public DateTime? LastRecoveryPointUtc { get; set; }
        public string PolicyId { get; set; }
        public string ProtectedItemDataId { get; set; }
        public string ProtectionState { get; set; }
        public string ProtectionStatus { get; set; }
        public string SourceResourceId { get; set; }
        public string WorkloadType { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDtUtc { get; set; }

        public bool IsEqual(VaultBackupModel backup)
        {
            return string.Equals(AzureId, backup.AzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SubscriptionId, backup.SubscriptionId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Subscription, backup.Subscription, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ResourceGroup, backup.ResourceGroup, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(VaultName, backup.VaultName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Name, backup.Name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(BackupManagementType, backup.BackupManagementType, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(BackupSetName, backup.BackupSetName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ComputerName, backup.ComputerName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ContainerName, backup.ContainerName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(CreateMode, backup.CreateMode, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(DeferredDeleteSyncTimeInUtc, backup.DeferredDeleteSyncTimeInUtc) &&
                   string.Equals(ExtendedInfo, backup.ExtendedInfo, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(FriendlyName, backup.FriendlyName, StringComparison.OrdinalIgnoreCase) &&
                   IsScheduledForDeferredDelete == backup.IsScheduledForDeferredDelete &&
                   HealthDetailsCount == backup.HealthDetailsCount &&
                   HealthCode == backup.HealthCode &&
                   string.Equals(HealthMessage, backup.HealthMessage, StringComparison.OrdinalIgnoreCase) &&
                   HealthRecommendationsCount == backup.HealthRecommendationsCount &&
                   string.Equals(HealthStatus, backup.HealthStatus, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(LastBackupStatus, backup.LastBackupStatus, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(LastBackupTimeUtc, backup.LastBackupTimeUtc) &&
                   IsDateEqual(LastRecoveryPointUtc, backup.LastRecoveryPointUtc) &&
                   string.Equals(PolicyId, backup.PolicyId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ProtectedItemDataId, backup.ProtectedItemDataId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ProtectionState, backup.ProtectionState, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ProtectionStatus, backup.ProtectionStatus, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SourceResourceId, backup.SourceResourceId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(WorkloadType, backup.WorkloadType, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(CreatedBy, backup.CreatedBy, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(CreatedDtUtc, backup.CreatedDtUtc);
        }
    }
}