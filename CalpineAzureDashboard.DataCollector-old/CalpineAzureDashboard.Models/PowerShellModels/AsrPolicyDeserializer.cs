namespace CalpineAzureDashboard.Models.PowerShellModels
{
    public class AsrPolicyDeserializer
    {
        public string FriendlyName { get; set; }
        public string Name { get; set; }
        public string ID { get; set; }
        public string Type { get; set; }
        public string ReplicationProvider { get; set; }
        public AsrAzureToAzurePolicyDetailsDeserializer ReplicationProviderSettings { get; set; }
    }

    public class AsrAzureToAzurePolicyDetailsDeserializer
    {
        public int? AppConsistentFrequencyInMinutes { get; set; }
        public int? CrashConsistentFrequencyInMinutes { get; set; }
        public string MultiVmSyncStatus { get; set; }
        public int? RecoveryPointHistory { get; set; }
        public int? RecoveryPointThresholdInMinutes { get; set; }
    }
}
