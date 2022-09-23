using System;
using System.Collections.Generic;

namespace CalpineAzureDashboard.Models.PowerShellModels
{
    public class AsrReplicatedItemDeserializer
    {
        public string ActiveLocation { get; set; }
        public List<string> AllowedOperations { get; set; }
        public CurrentScenarioDeserializer CurrentScenario { get; set; }
        public string FailoverRecoveryPointId { get; set; }
        public string FriendlyName { get; set; }
        public string ID { get; set; }
        public DateTime? LastSuccessfulFailoverTime { get; set; }
        public DateTime? LastSuccessfulTestFailoverTime { get; set; }
        public string Name { get; set; }
        public List<NicDetailsListDeserializer> NicDetailsList { get; set; }
        public string PolicyFriendlyName { get; set; }
        public string PolicyID { get; set; }
        public string PrimaryFabricFriendlyName { get; set; }
        public string PrimaryProtectionContainerFriendlyName { get; set; }
        public string ProtectableItemId { get; set; }
        public string ProtectedItemType { get; set; }
        public string ProtectionState { get; set; }
        public string ProtectionStateDescription { get; set; }
        public ProviderSpecificDetailsDeserializer ProviderSpecificDetails { get; set; }
        public string RecoveryAzureStorageAccount { get; set; }
        public string RecoveryAzureVMName { get; set; }
        public string RecoveryAzureVMSize { get; set; }
        public string RecoveryFabricFriendlyName { get; set; }
        public string RecoveryFabricId { get; set; }
        public string RecoveryProtectionContainerFriendlyName { get; set; }
        public string RecoveryResourceGroupId { get; set; }
        public string RecoveryServicesProviderId { get; set; }
        public string ReplicationHealth { get; set; }
        public List<ReplicationHealthErrorsDeserializer> ReplicationHealthErrors { get; set; }
        public string ReplicationProvider { get; set; }
        public string SelectedRecoveryAzureNetworkId { get; set; }
        public string TestFailoverState { get; set; }
        public string TestFailoverStateDescription { get; set; }
        public string Type { get; set; }
    }

    public class CurrentScenarioDeserializer
    {
        public string scenarioName { get; set; }
        public string jobId { get; set; }
        public DateTime? startTime { get; set; }
    }

    public class NicDetailsListDeserializer
    {
        public string PrimaryNicStaticIPAddress { get; set; }
        public string RecoveryNicIpAddressType { get; set; }
        public string ReplicaNicId { get; set; }
        public string SourceNicArmId { get; set; }
        public string IpAddressType { get; set; }
        public string NicId { get; set; }
        public string RecoveryVMNetworkId { get; set; }
        public string RecoveryVMSubnetName { get; set; }
        public string ReplicaNicStaticIpAddress { get; set; }
        public string SelectionType { get; set; }
        public string VMNetworkName { get; set; }
        public string VMSubnetName { get; set; }
    }

    public class ProviderSpecificDetailsDeserializer
    {
        public string AgentVersion { get; set; }
        public DateTime? LastRpoCalculatedTime { get; set; }
        public long? RpoInSeconds { get; set; }
        public bool? IsReplicationAgentUpdateRequired { get; set; }
        public string FabricObjectId { get; set; }
        public string MultiVmGroupId { get; set; }
        public string MultiVmGroupName { get; set; }
        public string OSType { get; set; }
        public string PrimaryFabricLocation { get; set; }
        public string RecoveryAzureResourceGroupId { get; set; }
        public string RecoveryAzureCloudService { get; set; }
        public string RecoveryFabricLocation { get; set; }
        public string RecoveryAvailabilitySet { get; set; }
        public VmSyncedConfigDetailsDeserializer VmSyncedConfigDetails { get; set; }
        public string MonitoringJobType { get; set; }
        public int? MonitoringPercentageCompletion { get; set; }
        public DateTime? LastHeartbeat { get; set; }
        public List<A2ADiskDetailsDeserializer> A2ADiskDetails { get; set; }
        public string RecoveryFabricObjectId { get; set; }
        public string TestFailoverRecoveryFabricObjectId { get; set; }
    }

    public class VmSyncedConfigDetailsDeserializer
    {
        public Dictionary<string, string> Tags { get; set; }
        public string RoleAssignments { get; set; }
        public string InputEndpoints { get; set; }
    }

    public class A2ADiskDetailsDeserializer
    {
        public bool? Managed { get; set; }
        public string RecoveryReplicaDiskAccountType { get; set; }
        public string RecoveryReplicaDiskId { get; set; }
        public string RecoveryResourceGroupId { get; set; }
        public string RecoveryTargetDiskAccountType { get; set; }
        public string RecoveryTargetDiskId { get; set; }
        public string DiskUri { get; set; }
        public string PrimaryDiskAzureStorageAccountId { get; set; }
        public string PrimaryStagingAzureStorageAccountId { get; set; }
        public string RecoveryAzureStorageAccountId { get; set; }
        public string DiskCapacityInBytes { get; set; }
        public string DiskName { get; set; }
        public string DiskType { get; set; }
        public string RecoveryDiskUri { get; set; }
        public bool? ResyncRequired { get; set; }
        public string MonitoringJobType { get; set; }
        public int? MonitoringPercentageCompletion { get; set; }
        public double? DataPendingInStagingStorageAccountInMB { get; set; }
        public double? DataPendingAtSourceAgentInMB { get; set; }
    }

    public class ReplicationHealthErrorsDeserializer
    {
        public string ErrorSource { get; set; }
        public string ErrorType { get; set; }
        public List <ReplicationHealthErrorsDeserializer> childError { get; set; }
        public DateTime? CreationTimeUtc { get; set; }
        public string EntityId { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorLevel { get; set; }
        public string ErrorMessage { get; set; }
        public string PossibleCauses { get; set; }
        public string RecommendedAction { get; set; }
        public string RecoveryProviderErrorMessage { get; set; }
    }
}
