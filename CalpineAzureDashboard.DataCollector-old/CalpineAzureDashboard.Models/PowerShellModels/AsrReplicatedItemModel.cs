using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models.PowerShellModels
{
    [Table("AsrReplicatedItem")]
    public class AsrReplicatedItemModel : AzureModel
    {
        public string SubscriptionId { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string VaultName { get; set; }
        public string Name { get; set; }
        public string ActiveLocation { get; set; }
        public string AllowedOperations { get; set; }
        public string CurrentScenarioName { get; set; }
        public string CurrentScenarioJobId { get; set; }
        public DateTime? CurrentScenarioStartTime { get; set; }
        public string FailoverRecoveryPointId { get; set; }
        public string FriendlyName { get; set; }
        public DateTime? LastSuccessfulFailoverTime { get; set; }
        public DateTime? LastSuccessfulTestFailoverTime { get; set; }
        public List<NicDetailModel> NicDetailsList { get; set; }
        public string PolicyFriendlyName { get; set; }
        public string PolicyID { get; set; }
        public string PrimaryFabricFriendlyName { get; set; }
        public string PrimaryProtectionContainerFriendlyName { get; set; }
        public string ProtectableItemId { get; set; }
        public string ProtectedItemType { get; set; }
        public string ProtectionState { get; set; }
        public string ProtectionStateDescription { get; set; }
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
        public string Tags { get; set; }
        public string RoleAssignments { get; set; }
        public string InputEndpoints { get; set; }
        public string MonitoringJobType { get; set; }
        public int? MonitoringPercentageCompletion { get; set; }
        public DateTime? LastHeartbeat { get; set; }
        public List<DiskDetailModel> A2ADiskDetails { get; set; }
        public string RecoveryFabricObjectId { get; set; }
        public string TestFailoverRecoveryFabricObjectId { get; set; }
        public string RecoveryAzureStorageAccount { get; set; }
        public string RecoveryAzureVMName { get; set; }
        public string RecoveryAzureVMSize { get; set; }
        public string RecoveryFabricFriendlyName { get; set; }
        public string RecoveryFabricId { get; set; }
        public string RecoveryProtectionContainerFriendlyName { get; set; }
        public string RecoveryResourceGroupId { get; set; }
        public string RecoveryServicesProviderId { get; set; }
        public string ReplicationHealth { get; set; }
        public List<HealthErrorModel> ReplicationHealthErrors { get; set; }
        public string ReplicationProvider { get; set; }
        public string SelectedRecoveryAzureNetworkId { get; set; }
        public string TestFailoverState { get; set; }
        public string TestFailoverStateDescription { get; set; }
        public string Type { get; set; }

        public bool IsEqual(AsrReplicatedItemModel asrReplicatedItem)
        {
            return string.Equals(AzureId, asrReplicatedItem.AzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SubscriptionId, asrReplicatedItem.SubscriptionId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Subscription, asrReplicatedItem.Subscription, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ResourceGroup, asrReplicatedItem.ResourceGroup, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(VaultName, asrReplicatedItem.VaultName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Name, asrReplicatedItem.Name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ActiveLocation, asrReplicatedItem.ActiveLocation, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(AllowedOperations, asrReplicatedItem.AllowedOperations, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(CurrentScenarioName, asrReplicatedItem.CurrentScenarioName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(CurrentScenarioJobId, asrReplicatedItem.CurrentScenarioJobId, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(CurrentScenarioStartTime, asrReplicatedItem.CurrentScenarioStartTime) &&
                   string.Equals(FailoverRecoveryPointId, asrReplicatedItem.FailoverRecoveryPointId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(FriendlyName, asrReplicatedItem.FriendlyName, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(LastSuccessfulFailoverTime, asrReplicatedItem.LastSuccessfulFailoverTime) &&
                   IsDateEqual(LastSuccessfulTestFailoverTime, asrReplicatedItem.LastSuccessfulTestFailoverTime) &&
                   string.Equals(PolicyFriendlyName, asrReplicatedItem.PolicyFriendlyName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(PolicyID, asrReplicatedItem.PolicyID, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(PrimaryFabricFriendlyName, asrReplicatedItem.PrimaryFabricFriendlyName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(PrimaryProtectionContainerFriendlyName, asrReplicatedItem.PrimaryProtectionContainerFriendlyName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ProtectableItemId, asrReplicatedItem.ProtectableItemId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ProtectedItemType, asrReplicatedItem.ProtectedItemType, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ProtectionState, asrReplicatedItem.ProtectionState, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ProtectionStateDescription, asrReplicatedItem.ProtectionStateDescription, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(AgentVersion, asrReplicatedItem.AgentVersion, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(LastRpoCalculatedTime, asrReplicatedItem.LastRpoCalculatedTime) &&
                   RpoInSeconds == asrReplicatedItem.RpoInSeconds &&
                   IsReplicationAgentUpdateRequired == asrReplicatedItem.IsReplicationAgentUpdateRequired &&
                   string.Equals(FabricObjectId, asrReplicatedItem.FabricObjectId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(MultiVmGroupId, asrReplicatedItem.MultiVmGroupId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(MultiVmGroupName, asrReplicatedItem.MultiVmGroupName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(OSType, asrReplicatedItem.OSType, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(PrimaryFabricLocation, asrReplicatedItem.PrimaryFabricLocation, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(RecoveryAzureResourceGroupId, asrReplicatedItem.RecoveryAzureResourceGroupId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(RecoveryAzureCloudService, asrReplicatedItem.RecoveryAzureCloudService, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(RecoveryFabricLocation, asrReplicatedItem.RecoveryFabricLocation, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(RecoveryAvailabilitySet, asrReplicatedItem.RecoveryAvailabilitySet, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Tags, asrReplicatedItem.Tags, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(RoleAssignments, asrReplicatedItem.RoleAssignments, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(InputEndpoints, asrReplicatedItem.InputEndpoints, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(MonitoringJobType, asrReplicatedItem.MonitoringJobType, StringComparison.OrdinalIgnoreCase) &&
                   MonitoringPercentageCompletion == asrReplicatedItem.MonitoringPercentageCompletion &&
                   IsDateEqual(LastHeartbeat, asrReplicatedItem.LastHeartbeat) &&
                   string.Equals(RecoveryFabricObjectId, asrReplicatedItem.RecoveryFabricObjectId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(TestFailoverRecoveryFabricObjectId, asrReplicatedItem.TestFailoverRecoveryFabricObjectId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(RecoveryAzureStorageAccount, asrReplicatedItem.RecoveryAzureStorageAccount, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(RecoveryAzureVMName, asrReplicatedItem.RecoveryAzureVMName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(RecoveryAzureVMSize, asrReplicatedItem.RecoveryAzureVMSize, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(RecoveryFabricFriendlyName, asrReplicatedItem.RecoveryFabricFriendlyName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(RecoveryFabricId, asrReplicatedItem.RecoveryFabricId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(RecoveryProtectionContainerFriendlyName, asrReplicatedItem.RecoveryProtectionContainerFriendlyName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(RecoveryResourceGroupId, asrReplicatedItem.RecoveryResourceGroupId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(RecoveryServicesProviderId, asrReplicatedItem.RecoveryServicesProviderId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ReplicationHealth, asrReplicatedItem.ReplicationHealth, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ReplicationProvider, asrReplicatedItem.ReplicationProvider, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SelectedRecoveryAzureNetworkId, asrReplicatedItem.SelectedRecoveryAzureNetworkId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(TestFailoverState, asrReplicatedItem.TestFailoverState, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(TestFailoverStateDescription, asrReplicatedItem.TestFailoverStateDescription, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Type, asrReplicatedItem.Type, StringComparison.OrdinalIgnoreCase);
        }
        
        [Table("AsrReplicatedItemNic")]
        public class NicDetailModel : AzureModel
        {
            [Column("AsrReplicatedItemAzureId")]
            public override string AzureId { get; set; }
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

            public bool IsEqual(NicDetailModel nicDetails)
            {
                return string.Equals(PrimaryNicStaticIPAddress, nicDetails.PrimaryNicStaticIPAddress, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(RecoveryNicIpAddressType, nicDetails.RecoveryNicIpAddressType, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(ReplicaNicId, nicDetails.ReplicaNicId, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(SourceNicArmId, nicDetails.SourceNicArmId, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(IpAddressType, nicDetails.IpAddressType, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(NicId, nicDetails.NicId, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(RecoveryVMNetworkId, nicDetails.RecoveryVMNetworkId, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(RecoveryVMSubnetName, nicDetails.RecoveryVMSubnetName, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(ReplicaNicStaticIpAddress, nicDetails.ReplicaNicStaticIpAddress, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(SelectionType, nicDetails.SelectionType, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(VMNetworkName, nicDetails.VMNetworkName, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(VMSubnetName, nicDetails.VMSubnetName, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Table("AsrReplicatedItemDisk")]
        public class DiskDetailModel : AzureModel
        {
            [Column("AsrReplicatedItemAzureId")]
            public override string AzureId { get; set; }
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

            public bool IsEqual(DiskDetailModel a2ADiskDetail)
            {
                return Managed == a2ADiskDetail.Managed &&
                       string.Equals(RecoveryReplicaDiskAccountType, a2ADiskDetail.RecoveryReplicaDiskAccountType, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(RecoveryReplicaDiskId, a2ADiskDetail.RecoveryReplicaDiskId, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(RecoveryResourceGroupId, a2ADiskDetail.RecoveryResourceGroupId, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(RecoveryTargetDiskAccountType, a2ADiskDetail.RecoveryTargetDiskAccountType, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(RecoveryTargetDiskId, a2ADiskDetail.RecoveryTargetDiskId, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(DiskUri, a2ADiskDetail.DiskUri, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(PrimaryDiskAzureStorageAccountId, a2ADiskDetail.PrimaryDiskAzureStorageAccountId, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(PrimaryStagingAzureStorageAccountId, a2ADiskDetail.PrimaryStagingAzureStorageAccountId, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(RecoveryAzureStorageAccountId, a2ADiskDetail.RecoveryAzureStorageAccountId, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(DiskCapacityInBytes, a2ADiskDetail.DiskCapacityInBytes, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(DiskName, a2ADiskDetail.DiskName, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(DiskType, a2ADiskDetail.DiskType, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(RecoveryDiskUri, a2ADiskDetail.RecoveryDiskUri, StringComparison.OrdinalIgnoreCase) &&
                       ResyncRequired == a2ADiskDetail.ResyncRequired &&
                       string.Equals(MonitoringJobType, a2ADiskDetail.MonitoringJobType, StringComparison.OrdinalIgnoreCase) &&
                       MonitoringPercentageCompletion == a2ADiskDetail.MonitoringPercentageCompletion &&
                       IsDoubleEqual(DataPendingInStagingStorageAccountInMB, a2ADiskDetail.DataPendingInStagingStorageAccountInMB) &&
                       IsDoubleEqual(DataPendingAtSourceAgentInMB, a2ADiskDetail.DataPendingAtSourceAgentInMB);
            }
        }

        [Table("AsrReplicatedItemHealthError")]
        public class HealthErrorModel : AzureModel
        {
            [Column("AsrReplicatedItemAzureId")]
            public override string AzureId { get; set; }
            public int? ParentErrorId { get; set; }
            public string ErrorSource { get; set; }
            public string ErrorType { get; set; }
            public List<HealthErrorModel> ChildErrors { get; set; }
            public DateTime? CreationTimeUtc { get; set; }
            public string EntityId { get; set; }
            public string ErrorCode { get; set; }
            public string ErrorLevel { get; set; }
            public string ErrorMessage { get; set; }
            public string PossibleCauses { get; set; }
            public string RecommendedAction { get; set; }
            public string RecoveryProviderErrorMessage { get; set; }

            public bool IsEqual(HealthErrorModel replicationHealthError)
            {
                return string.Equals(AzureId, replicationHealthError.AzureId, StringComparison.OrdinalIgnoreCase) &&
                       ParentErrorId == replicationHealthError.ParentErrorId &&
                       string.Equals(ErrorSource, replicationHealthError.ErrorSource, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(ErrorType, replicationHealthError.ErrorType, StringComparison.OrdinalIgnoreCase) &&
                       IsDateEqual(CreationTimeUtc, replicationHealthError.CreationTimeUtc) &&
                       string.Equals(EntityId, replicationHealthError.EntityId, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(ErrorCode, replicationHealthError.ErrorCode, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(ErrorLevel, replicationHealthError.ErrorLevel, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(ErrorMessage, replicationHealthError.ErrorMessage, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(PossibleCauses, replicationHealthError.PossibleCauses, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(RecommendedAction, replicationHealthError.RecommendedAction, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(RecoveryProviderErrorMessage, replicationHealthError.RecoveryProviderErrorMessage, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
