using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.Models.PowerShellModels;
using Newtonsoft.Json;

namespace CalpineAzureDashboard.DataCollector.Inventory
{
    public class AsrReplicatedItemInventory : AzureInventory<AsrReplicatedItemModel>
    {
        public AsrReplicatedItemInventory(AzureService azureService, IRepository<AsrReplicatedItemModel> repository) : 
            base(azureService, repository, "ASR replicated item", null) { }

        public override async Task<IEnumerable<AsrReplicatedItemModel>> GetInventoryAsync(IEnumerable<AsrReplicatedItemModel> inventoryParam = null)
        {
            if (inventoryParam != null) {
                return inventoryParam;
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s started at: {DateTime.Now}");
            var inventory = new List<AsrReplicatedItemModel>();

            foreach (var subscription in Subscriptions) {
                try {
                    var asrReplicatedItems = await AzureService.GetAsrReplicatedItemsAsync(subscription.SubscriptionId);

                    if (asrReplicatedItems.HadErrors) {
                        Log.Error($"An error occurred getting details for {InventoryType}: {Environment.NewLine} {asrReplicatedItems.GetErrors()}");
                        //throw new Exception($"Unable to get details for {InventoryType}. Check logs for details.");
                    }

                    foreach (var asrReplicatedItem in asrReplicatedItems.Items) {
                        try {
                            var newAsrReplicatedItem = new AsrReplicatedItemModel();
                            newAsrReplicatedItem.AzureId = asrReplicatedItem.ID;
                            newAsrReplicatedItem.SubscriptionId = subscription.SubscriptionId;
                            newAsrReplicatedItem.Subscription = subscription.DisplayName;
                            newAsrReplicatedItem.ResourceGroup = asrReplicatedItem.ID.Split('/')[4];
                            newAsrReplicatedItem.VaultName = asrReplicatedItem.ID.Split('/')[8];
                            newAsrReplicatedItem.Name = asrReplicatedItem.Name;
                            newAsrReplicatedItem.ActiveLocation = asrReplicatedItem.ActiveLocation;
                            newAsrReplicatedItem.AllowedOperations = string.Join(", ", asrReplicatedItem.AllowedOperations);
                            newAsrReplicatedItem.CurrentScenarioName = asrReplicatedItem.CurrentScenario.scenarioName;
                            newAsrReplicatedItem.CurrentScenarioJobId = asrReplicatedItem.CurrentScenario.jobId;
                            newAsrReplicatedItem.CurrentScenarioStartTime = asrReplicatedItem.CurrentScenario.startTime;
                            newAsrReplicatedItem.FailoverRecoveryPointId = asrReplicatedItem.FailoverRecoveryPointId;
                            newAsrReplicatedItem.FriendlyName = asrReplicatedItem.FriendlyName;
                            newAsrReplicatedItem.LastSuccessfulFailoverTime = asrReplicatedItem.LastSuccessfulFailoverTime;
                            newAsrReplicatedItem.LastSuccessfulTestFailoverTime = asrReplicatedItem.LastSuccessfulTestFailoverTime;
                            newAsrReplicatedItem.PolicyFriendlyName = asrReplicatedItem.PolicyFriendlyName;
                            newAsrReplicatedItem.PolicyID = asrReplicatedItem.PolicyID;
                            newAsrReplicatedItem.PrimaryFabricFriendlyName = asrReplicatedItem.PrimaryFabricFriendlyName;
                            newAsrReplicatedItem.PrimaryProtectionContainerFriendlyName = asrReplicatedItem.PrimaryProtectionContainerFriendlyName;
                            newAsrReplicatedItem.ProtectableItemId = asrReplicatedItem.ProtectableItemId;
                            newAsrReplicatedItem.ProtectedItemType = asrReplicatedItem.ProtectedItemType;
                            newAsrReplicatedItem.ProtectionState = asrReplicatedItem.ProtectionState;
                            newAsrReplicatedItem.ProtectionStateDescription = asrReplicatedItem.ProtectionStateDescription;
                            newAsrReplicatedItem.AgentVersion = asrReplicatedItem.ProviderSpecificDetails.AgentVersion;
                            newAsrReplicatedItem.LastRpoCalculatedTime = asrReplicatedItem.ProviderSpecificDetails.LastRpoCalculatedTime;
                            newAsrReplicatedItem.RpoInSeconds = asrReplicatedItem.ProviderSpecificDetails.RpoInSeconds;
                            newAsrReplicatedItem.IsReplicationAgentUpdateRequired = asrReplicatedItem.ProviderSpecificDetails.IsReplicationAgentUpdateRequired;
                            newAsrReplicatedItem.FabricObjectId = asrReplicatedItem.ProviderSpecificDetails.FabricObjectId;
                            newAsrReplicatedItem.MultiVmGroupId = asrReplicatedItem.ProviderSpecificDetails.MultiVmGroupId;
                            newAsrReplicatedItem.MultiVmGroupName = asrReplicatedItem.ProviderSpecificDetails.MultiVmGroupName;
                            newAsrReplicatedItem.OSType = asrReplicatedItem.ProviderSpecificDetails.OSType;
                            newAsrReplicatedItem.PrimaryFabricLocation = asrReplicatedItem.ProviderSpecificDetails.PrimaryFabricLocation;
                            newAsrReplicatedItem.RecoveryAzureResourceGroupId = asrReplicatedItem.ProviderSpecificDetails.RecoveryAzureResourceGroupId;
                            newAsrReplicatedItem.RecoveryAzureCloudService = asrReplicatedItem.ProviderSpecificDetails.RecoveryAzureCloudService;
                            newAsrReplicatedItem.RecoveryFabricLocation = asrReplicatedItem.ProviderSpecificDetails.RecoveryFabricLocation;
                            newAsrReplicatedItem.RecoveryAvailabilitySet = asrReplicatedItem.ProviderSpecificDetails.RecoveryAvailabilitySet;
                            newAsrReplicatedItem.Tags =  JsonConvert.SerializeObject(asrReplicatedItem.ProviderSpecificDetails.VmSyncedConfigDetails?.Tags);
                            newAsrReplicatedItem.RoleAssignments = asrReplicatedItem.ProviderSpecificDetails.VmSyncedConfigDetails?.RoleAssignments;
                            newAsrReplicatedItem.InputEndpoints = asrReplicatedItem.ProviderSpecificDetails.VmSyncedConfigDetails?.InputEndpoints;
                            newAsrReplicatedItem.MonitoringJobType = asrReplicatedItem.ProviderSpecificDetails.MonitoringJobType;
                            newAsrReplicatedItem.MonitoringPercentageCompletion = asrReplicatedItem.ProviderSpecificDetails.MonitoringPercentageCompletion;
                            newAsrReplicatedItem.LastHeartbeat = asrReplicatedItem.ProviderSpecificDetails.LastHeartbeat;
                            newAsrReplicatedItem.RecoveryFabricObjectId = asrReplicatedItem.ProviderSpecificDetails.RecoveryFabricObjectId;
                            newAsrReplicatedItem.TestFailoverRecoveryFabricObjectId = asrReplicatedItem.ProviderSpecificDetails.TestFailoverRecoveryFabricObjectId;
                            newAsrReplicatedItem.RecoveryAzureStorageAccount = asrReplicatedItem.RecoveryAzureStorageAccount;
                            newAsrReplicatedItem.RecoveryAzureVMName = asrReplicatedItem.RecoveryAzureVMName;
                            newAsrReplicatedItem.RecoveryAzureVMSize = asrReplicatedItem.RecoveryAzureVMSize;
                            newAsrReplicatedItem.RecoveryFabricFriendlyName = asrReplicatedItem.RecoveryFabricFriendlyName;
                            newAsrReplicatedItem.RecoveryFabricId = asrReplicatedItem.RecoveryFabricId;
                            newAsrReplicatedItem.RecoveryProtectionContainerFriendlyName = asrReplicatedItem.RecoveryProtectionContainerFriendlyName;
                            newAsrReplicatedItem.RecoveryResourceGroupId = asrReplicatedItem.RecoveryResourceGroupId;
                            newAsrReplicatedItem.RecoveryServicesProviderId = asrReplicatedItem.RecoveryServicesProviderId;
                            newAsrReplicatedItem.ReplicationHealth = asrReplicatedItem.ReplicationHealth;
                            newAsrReplicatedItem.ReplicationProvider = asrReplicatedItem.ReplicationProvider;
                            newAsrReplicatedItem.SelectedRecoveryAzureNetworkId = asrReplicatedItem.SelectedRecoveryAzureNetworkId;
                            newAsrReplicatedItem.TestFailoverState = asrReplicatedItem.TestFailoverState;
                            newAsrReplicatedItem.TestFailoverStateDescription = asrReplicatedItem.TestFailoverStateDescription;
                            newAsrReplicatedItem.Type = asrReplicatedItem.Type;

                            var nicDetailsList = new List<AsrReplicatedItemModel.NicDetailModel>();
                            foreach (var nicDetail in asrReplicatedItem.NicDetailsList) {
                                var newNicDetail = new AsrReplicatedItemModel.NicDetailModel();
                                newNicDetail.AzureId = asrReplicatedItem.ID;
                                newNicDetail.PrimaryNicStaticIPAddress = nicDetail.PrimaryNicStaticIPAddress;
                                newNicDetail.RecoveryNicIpAddressType = nicDetail.RecoveryNicIpAddressType;
                                newNicDetail.ReplicaNicId = nicDetail.ReplicaNicId;
                                newNicDetail.SourceNicArmId = nicDetail.SourceNicArmId;
                                newNicDetail.IpAddressType = nicDetail.IpAddressType;
                                newNicDetail.NicId = nicDetail.NicId;
                                newNicDetail.RecoveryVMNetworkId = nicDetail.RecoveryVMNetworkId;
                                newNicDetail.RecoveryVMSubnetName = nicDetail.RecoveryVMSubnetName;
                                newNicDetail.ReplicaNicStaticIpAddress = nicDetail.ReplicaNicStaticIpAddress;
                                newNicDetail.SelectionType = nicDetail.SelectionType;
                                newNicDetail.VMNetworkName = nicDetail.VMNetworkName;
                                newNicDetail.VMSubnetName = nicDetail.VMSubnetName;

                                nicDetailsList.Add(newNicDetail);
                            }
                            newAsrReplicatedItem.NicDetailsList = nicDetailsList;

                            var a2ADiskDetailModels = new List<AsrReplicatedItemModel.DiskDetailModel>();

                            if (asrReplicatedItem.ProviderSpecificDetails.A2ADiskDetails != null) {
                                foreach (var a2ADiskDetail in asrReplicatedItem.ProviderSpecificDetails.A2ADiskDetails) {
                                    var newA2ADiskDetail = new AsrReplicatedItemModel.DiskDetailModel();
                                    newA2ADiskDetail.AzureId = asrReplicatedItem.ID;
                                    newA2ADiskDetail.Managed = a2ADiskDetail.Managed;
                                    newA2ADiskDetail.RecoveryReplicaDiskAccountType = a2ADiskDetail.RecoveryReplicaDiskAccountType;
                                    newA2ADiskDetail.RecoveryReplicaDiskId = a2ADiskDetail.RecoveryReplicaDiskId;
                                    newA2ADiskDetail.RecoveryResourceGroupId = a2ADiskDetail.RecoveryResourceGroupId;
                                    newA2ADiskDetail.RecoveryTargetDiskAccountType = a2ADiskDetail.RecoveryTargetDiskAccountType;
                                    newA2ADiskDetail.RecoveryTargetDiskId = a2ADiskDetail.RecoveryTargetDiskId;
                                    newA2ADiskDetail.DiskUri = a2ADiskDetail.DiskUri;
                                    newA2ADiskDetail.PrimaryDiskAzureStorageAccountId = a2ADiskDetail.PrimaryDiskAzureStorageAccountId;
                                    newA2ADiskDetail.PrimaryStagingAzureStorageAccountId = a2ADiskDetail.PrimaryStagingAzureStorageAccountId;
                                    newA2ADiskDetail.RecoveryAzureStorageAccountId = a2ADiskDetail.RecoveryAzureStorageAccountId;
                                    newA2ADiskDetail.DiskCapacityInBytes = a2ADiskDetail.DiskCapacityInBytes;
                                    newA2ADiskDetail.DiskName = a2ADiskDetail.DiskName;
                                    newA2ADiskDetail.DiskType = a2ADiskDetail.DiskType;
                                    newA2ADiskDetail.RecoveryDiskUri = a2ADiskDetail.RecoveryDiskUri;
                                    newA2ADiskDetail.ResyncRequired = a2ADiskDetail.ResyncRequired;
                                    newA2ADiskDetail.MonitoringJobType = a2ADiskDetail.MonitoringJobType;
                                    newA2ADiskDetail.MonitoringPercentageCompletion = a2ADiskDetail.MonitoringPercentageCompletion;
                                    newA2ADiskDetail.DataPendingInStagingStorageAccountInMB = a2ADiskDetail.DataPendingInStagingStorageAccountInMB;
                                    newA2ADiskDetail.DataPendingAtSourceAgentInMB = a2ADiskDetail.DataPendingAtSourceAgentInMB;

                                    a2ADiskDetailModels.Add(newA2ADiskDetail);
                                }
                                newAsrReplicatedItem.A2ADiskDetails = a2ADiskDetailModels;
                            }

                            var healthErrors = new List<AsrReplicatedItemModel.HealthErrorModel>();
                            foreach (var healthError in asrReplicatedItem.ReplicationHealthErrors)
                            {
                                var newHealthError = new AsrReplicatedItemModel.HealthErrorModel();
                                newHealthError.AzureId = asrReplicatedItem.ID;
                                newHealthError.ErrorSource = healthError.ErrorSource;
                                newHealthError.ErrorType = healthError.ErrorType;
                                newHealthError.CreationTimeUtc = healthError.CreationTimeUtc.Equals(DateTime.MinValue) ? null : healthError.CreationTimeUtc;
                                newHealthError.EntityId = healthError.EntityId;
                                newHealthError.ErrorCode = healthError.ErrorCode;
                                newHealthError.ErrorLevel = healthError.ErrorLevel;
                                newHealthError.ErrorMessage = healthError.ErrorMessage;
                                newHealthError.PossibleCauses = healthError.PossibleCauses;
                                newHealthError.RecommendedAction = healthError.RecommendedAction;
                                newHealthError.RecoveryProviderErrorMessage = healthError.RecoveryProviderErrorMessage;

                                healthErrors.Add(newHealthError);

                                foreach (var childError in healthError.childError) {
                                    var newChildHealthError = new AsrReplicatedItemModel.HealthErrorModel();
                                    newChildHealthError.AzureId = asrReplicatedItem.ID;
                                    newChildHealthError.ErrorSource = healthError.ErrorSource;
                                    newChildHealthError.ErrorType = healthError.ErrorType;
                                    newChildHealthError.CreationTimeUtc = healthError.CreationTimeUtc;
                                    newChildHealthError.EntityId = healthError.EntityId;
                                    newChildHealthError.ErrorCode = healthError.ErrorCode;
                                    newChildHealthError.ErrorLevel = healthError.ErrorLevel;
                                    newChildHealthError.ErrorMessage = healthError.ErrorMessage;
                                    newChildHealthError.PossibleCauses = healthError.PossibleCauses;
                                    newChildHealthError.RecommendedAction = healthError.RecommendedAction;
                                    newChildHealthError.RecoveryProviderErrorMessage = healthError.RecoveryProviderErrorMessage;

                                    healthErrors.Add(newChildHealthError);
                                }

                            }
                            newAsrReplicatedItem.ReplicationHealthErrors = healthErrors;

                            inventory.Add(newAsrReplicatedItem);
                        }
                        catch (Exception ex) {
                            throw new Exception($"An exception occurred getting details for {InventoryType}: {asrReplicatedItem.ID}", ex);
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

        public override async Task<IEnumerable<AsrReplicatedItemModel>> ProcessInventoryAsync(IEnumerable<AsrReplicatedItemModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var itemsToInsert = new List<AsrReplicatedItemModel>();
            var itemsToUpdate = new List<AsrReplicatedItemModel>();

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