using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models.PowerShellModels
{
    [Table("AsrPolicy")]
    public class AsrPolicyModel : AzureModel
    {
        public string SubscriptionId { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string VaultName { get; set; }
        public string Name { get; set; }
        public string FriendlyName { get; set; }
        public string ReplicationProvider { get; set; }
        public int? AppConsistentFrequencyInMinutes { get; set; }
        public int? CrashConsistentFrequencyInMinutes { get; set; }
        public string MultiVmSyncStatus { get; set; }
        public int? RecoveryPointHistory { get; set; }
        public int? RecoveryPointThresholdInMinutes { get; set; }
        public string Type { get; set; }
        
        public bool IsEqual(AsrPolicyModel asrPolicy)
        {
            return string.Equals(AzureId, asrPolicy.AzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SubscriptionId, asrPolicy.SubscriptionId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Subscription, asrPolicy.Subscription, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ResourceGroup, asrPolicy.ResourceGroup, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(VaultName, asrPolicy.VaultName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Name, asrPolicy.Name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(FriendlyName, asrPolicy.FriendlyName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ReplicationProvider, asrPolicy.ReplicationProvider, StringComparison.OrdinalIgnoreCase) &&
                   AppConsistentFrequencyInMinutes == asrPolicy.AppConsistentFrequencyInMinutes &&
                   CrashConsistentFrequencyInMinutes == asrPolicy.CrashConsistentFrequencyInMinutes &&
                   string.Equals(MultiVmSyncStatus, asrPolicy.MultiVmSyncStatus, StringComparison.OrdinalIgnoreCase) &&
                   RecoveryPointHistory == asrPolicy.RecoveryPointHistory &&
                   RecoveryPointThresholdInMinutes == asrPolicy.RecoveryPointThresholdInMinutes &&
                   string.Equals(Type, asrPolicy.Type, StringComparison.OrdinalIgnoreCase);
        }
    }
}
