using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.DataCollector.Inventory;
using CalpineAzureDashboard.Models;
using CalpineAzureDashboard.Models.PowerShellModels;
using log4net;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;

namespace CalpineAzureDashboard.DataCollector
{
    internal class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
        private static string _connectionString;
        private static AzureService _azureService;

        static void Main(string[] args)
        {
            Log.Debug("Starting processing");
            var keyVaultUri = ConfigurationManager.AppSettings["keyVaultUri"];
            _connectionString = GetKeyVaultSecretAsync(keyVaultUri, "connectionString").GetAwaiter().GetResult();

            string clientId = GetKeyVaultSecretAsync(keyVaultUri, "clientId").GetAwaiter().GetResult();
            string clientSecret = GetKeyVaultSecretAsync(keyVaultUri, "clientSecret").GetAwaiter().GetResult();
            string tenantId = GetKeyVaultSecretAsync(keyVaultUri, "tenantId").GetAwaiter().GetResult();
            _azureService = new AzureService(clientId, clientSecret, tenantId);

            try {
                ProcessInventoryAsync().Wait();
            }
            catch (Exception ex) {
                Log.Error("An uncaught exception occurred:", ex);
            }

            Log.Debug("Finished processing");
        }

        private static async Task<string> GetKeyVaultSecretAsync(string keyVaultUrl, string secretName)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            var secret = await keyVaultClient.GetSecretAsync($"{keyVaultUrl}/secrets/{secretName}").ConfigureAwait(false);
            return secret.Value;
        }

        static async Task ProcessInventoryAsync()
        {
            try {
                var resourceGroupRepository = new ResourceGroupRepository<ResourceGroupModel>(_connectionString);
                var resourceGroup = new ResourceGroupInventory(_azureService, resourceGroupRepository);
                var resourceGroupInventory = await resourceGroup.ProcessInventoryAsync(await resourceGroup.GetInventoryAsync());
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing resource groups. Stopping processing...", ex);
            }

            try {
                var availabilitySetRepository = new AvailabilitySetRepository<AvailabilitySetModel>(_connectionString);
                var availabilitySet = new AvailabilitySetInventory(_azureService, availabilitySetRepository);
                var availabilitySetInventory = await availabilitySet.ProcessInventoryAsync(await availabilitySet.GetInventoryAsync());
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing availability sets. Stopping processing...", ex);
            }

            try {
                var dataDiskRepository = new DataDiskRepository<DataDiskModel>(_connectionString);
                var dataDisk = new DataDiskInventory(_azureService, dataDiskRepository);
                var dataDiskInventory = await dataDisk.ProcessInventoryAsync(await dataDisk.GetInventoryAsync());
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing data disks. Stopping processing...", ex);
            }

            try {
                var loadBalancerRepository = new LoadBalancerRepository<LoadBalancerModel>(_connectionString);
                var loadBalancer = new LoadBalancerInventory(_azureService, loadBalancerRepository);
                var loadBalancerInventory = (await loadBalancer.ProcessInventoryAsync(await loadBalancer.GetInventoryAsync())).ToList();

                try {
                    var loadBalancerFrontendRepository = new LoadBalancerFrontendRepository<LoadBalancerModel.FrontendModel>(_connectionString);
                    var loadBalancerFrontend = new LoadBalancerFrontendInventory(_azureService, loadBalancerFrontendRepository);
                    var frontendInventory = await loadBalancerFrontend.ProcessInventoryAsync(loadBalancerInventory.SelectMany(x => x.Frontends));
                }
                catch (Exception ex) {
                    Log.Error("An exception occurred processing load balancer front ends. Stopping processing...", ex);
                }

                try {
                    var loadBalancerBackendRepository = new LoadBalancerBackendRepository<LoadBalancerModel.BackendModel>(_connectionString);
                    var loadBalancerBackend = new LoadBalancerBackendInventory(_azureService, loadBalancerBackendRepository);
                    var backendInventory = await loadBalancerBackend.ProcessInventoryAsync(loadBalancerInventory.SelectMany(x => x.Backends));
                }
                catch (Exception ex) {
                    Log.Error("An exception occurred processing load balancer back ends. Stopping processing...", ex);
                }
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing load balancers. Stopping processing...", ex);
            }

            try {
                var networkInterfaceRepository = new NetworkInterfaceRepository<NetworkInterfaceModel>(_connectionString);
                var networkInterface = new NetworkInterfaceInventory(_azureService, networkInterfaceRepository);
                var networkInterfaceInventory = await networkInterface.ProcessInventoryAsync(await networkInterface.GetInventoryAsync());

                try {
                    var networkInterfaceIpConfigurationRepository =
                        new NetworkInterfaceIpConfigurationRepository<NetworkInterfaceModel.IpConfigurationModel>(_connectionString);
                    var networkInterfaceIpConfiguration =
                        new NetworkInterfaceIpConfigurationInventory(_azureService, networkInterfaceIpConfigurationRepository);
                    var ipConfigurationInventory =
                        await networkInterfaceIpConfiguration.ProcessInventoryAsync(networkInterfaceInventory.SelectMany(x => x.IpConfigurations));
                }
                catch (Exception ex) {
                    Log.Error("An exception occurred processing network interface ip configurations. Stopping processing...", ex);
                }
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing network interfaces. Stopping processing...", ex);
            }

            try {
                var publicIpRepository = new PublicIpRepository<PublicIpModel>(_connectionString);
                var publicIp = new PublicIpInventory(_azureService, publicIpRepository);
                var publicIpInventory = await publicIp.ProcessInventoryAsync(await publicIp.GetInventoryAsync());
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing public ips. Stopping processing...", ex);
            }

            try {
                var virtualNetworkRepository = new VirtualNetworkRepository<VirtualNetworkModel>(_connectionString);
                var virtualNetwork = new VirtualNetworkInventory(_azureService, virtualNetworkRepository);
                var virtualNetworkInventory = (await virtualNetwork.ProcessInventoryAsync(await virtualNetwork.GetInventoryAsync())).ToList();

                try {
                    var virtualNetworkPeeringRepository =
                        new VirtualNetworkPeeringRepository<VirtualNetworkModel.VirtualNetworkPeeringModel>(_connectionString);
                    var virtualNetworkPeering = new VirtualNetworkPeeringInventory(_azureService, virtualNetworkPeeringRepository);
                    var virtualNetworkPeeringInventory = await virtualNetworkPeering.ProcessInventoryAsync(virtualNetworkInventory.SelectMany(x => x.Peerings));
                }
                catch (Exception ex) {
                    Log.Error("An exception occurred processing virtual network peerings. Stopping processing...", ex);
                }

                try {
                    var subnetRepository = new SubnetRepository<SubnetModel>(_connectionString);
                    var subnet = new SubnetInventory(_azureService, subnetRepository);
                    var subnetInventory = (await subnet.ProcessInventoryAsync(virtualNetworkInventory.SelectMany(x => x.Subnets))).ToList();

                    try {
                        var subnetIpConfigurationRepository = new SubnetIpConfigurationRepository<SubnetModel.IpConfigurationModel>(_connectionString);
                        var subnetIpConfiguration = new SubnetIpConfigurationInventory(_azureService, subnetIpConfigurationRepository);
                        var subnetIpConfigurationsInventory =
                            await subnetIpConfiguration.ProcessInventoryAsync(subnetInventory.SelectMany(x => x.IpConfigurations));
                    }
                    catch (Exception ex) {
                        Log.Error("An exception occurred processing subnet ip configurations. Stopping processing...", ex);
                    }

                    try {
                        var subnetServiceEndpointRepository = new SubnetServiceEndpointRepository<SubnetModel.SubnetServiceEndpointModel>(_connectionString);
                        var subnetServiceEndpoint = new SubnetServiceEndpointInventory(_azureService, subnetServiceEndpointRepository);
                        var subnetServiceEndpointsInventory =
                            await subnetServiceEndpoint.ProcessInventoryAsync(subnetInventory.SelectMany(x => x.ServiceEndpoints));
                    }
                    catch (Exception ex) {
                        Log.Error("An exception occurred processing subnet service endpoints. Stopping processing...", ex);
                    }
                }
                catch (Exception ex) {
                    Log.Error("An exception occurred processing subnets. Stopping processing...", ex);
                }
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing virtual networks. Stopping processing...", ex);
            }

            try {
                var networkSecurityGroupRepository = new NetworkSecurityGroupRepository<NetworkSecurityGroupModel>(_connectionString);
                var networkSecurityGroup = new NetworkSecurityGroupInventory(_azureService, networkSecurityGroupRepository);
                var networkSecurityGroupInventory = await networkSecurityGroup.ProcessInventoryAsync(await networkSecurityGroup.GetInventoryAsync());

                try {
                    var securityRuleRepository = new SecurityRuleRepository<NetworkSecurityGroupModel.SecurityRuleModel>(_connectionString);
                    var securityRule = new SecurityRuleInventory(_azureService, securityRuleRepository);
                    var securityRuleInventory = await securityRule.ProcessInventoryAsync(networkSecurityGroupInventory.SelectMany(x => x.SecurityRules));
                }
                catch (Exception ex) {
                    Log.Error("An exception occurred processing network security group security rules. Stopping processing...", ex);
                }
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing network security groups. Stopping processing...", ex);
            }

            try {
                var virtualMachineRepository = new VirtualMachineRepository<VirtualMachineModel>(_connectionString);
                var virtualMachine = new VirtualMachineInventory(_azureService, virtualMachineRepository);
                var virtualMachineInventory = await virtualMachine.ProcessInventoryAsync(await virtualMachine.GetInventoryAsync());

                try {
                    var virtualMachineExtensionRepository =
                        new VirtualMachineExtensionRepository<VirtualMachineModel.VirtualMachineExtensionModel>(_connectionString);
                    var virtualMachineExtension = new VirtualMachineExtensionInventory(_azureService, virtualMachineExtensionRepository);
                    var virtualMachineExtensionInventory =
                        await virtualMachineExtension.ProcessInventoryAsync(virtualMachineInventory.SelectMany(x => x.Extensions));
                }
                catch (Exception ex) {
                    Log.Error("An exception occurred processing virtual machine extensions. Stopping processing...", ex);
                }
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing virtual machines. Stopping processing...", ex);
            }

            try {
                var storageAccountRepository = new StorageAccountRepository<StorageAccountModel>(_connectionString);
                var storageAccount = new StorageAccountInventory(_azureService, storageAccountRepository);
                var storageInventory = await storageAccount.ProcessInventoryAsync(await storageAccount.GetInventoryAsync());
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing storage accounts. Stopping processing...", ex);
            }

            try {
                var webAppRepository = new WebAppRepository<WebAppModel>(_connectionString);
                var webApp = new WebAppInventory(_azureService, webAppRepository);
                var webappInventory = await webApp.ProcessInventoryAsync(await webApp.GetInventoryAsync());
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing web apps. Stopping processing...", ex);
            }

            try {
                var activityLogRepository = new ActivityLogRepository<ActivityLogModel>(_connectionString);
                var activityLog = new ActivityLogInventory(_azureService, activityLogRepository);
                var activityLogInventory = await activityLog.ProcessInventoryAsync(await activityLog.GetInventoryAsync());
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing activity logs. Stopping processing...", ex);
            }

            try {
                var vaultBackupPolicyRepository = new VaultBackupPolicyRepository<VaultBackupPolicyModel>(_connectionString);
                var vaultBackupPolicy = new VaultBackupPolicyInventory(_azureService, vaultBackupPolicyRepository);
                var vaultPolicyInventory = await vaultBackupPolicy.ProcessInventoryAsync(await vaultBackupPolicy.GetInventoryAsync());
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing vault backup policies. Stopping processing...", ex);
            }

            try {
                var vaultBackupRepository = new VaultBackupRepository<VaultBackupModel>(_connectionString);
                var vaultBackup = new VaultBackupInventory(_azureService, vaultBackupRepository);
                var vaultBackupInventory = await vaultBackup.ProcessInventoryAsync(await vaultBackup.GetInventoryAsync());
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing vault backup. Stopping processing...", ex);
            }

            try {
                var vaultBackupJobRepository = new VaultBackupJobRepository<VaultBackupJobModel>(_connectionString);
                var vaultBackupJob = new VaultBackupJobInventory(_azureService, vaultBackupJobRepository);
                var vaultBackupJobInventory = await vaultBackupJob.ProcessInventoryAsync(await vaultBackupJob.GetInventoryAsync());
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing vault backup jobs. Stopping processing...", ex);
            }

            try {
                var asrReplicationItemRepository = new AsrReplicatedItemRepository<AsrReplicatedItemModel>(_connectionString);
                var asrReplicatedItem = new AsrReplicatedItemInventory(_azureService, asrReplicationItemRepository);
                var asrReplicatedItemInventory = (await asrReplicatedItem.ProcessInventoryAsync(await asrReplicatedItem.GetInventoryAsync())).ToList();

                try {
                    var asrReplicatedItemNicRepository = new AsrReplicatedItemNicRepository<AsrReplicatedItemModel.NicDetailModel>(_connectionString);
                    var asrReplicatedItemNic = new AsrReplicatedItemNicInventory(_azureService, asrReplicatedItemNicRepository);
                    var nicDetails = asrReplicatedItemInventory.Where(x => x.NicDetailsList != null).SelectMany(x => x.NicDetailsList);
                    var asrReplicatedItemNicInventory = await asrReplicatedItemNic.ProcessInventoryAsync(nicDetails);
                }
                catch (Exception ex) {
                    Log.Error("An exception occurred processing ASR replicated item nics. Stopping processing...", ex);
                }

                try {
                    var asrReplicatedItemDiskRepository = new AsrReplicatedItemDiskRepository<AsrReplicatedItemModel.DiskDetailModel>(_connectionString);
                    var asrReplicatedItemDisk = new AsrReplicatedItemDiskInventory(_azureService, asrReplicatedItemDiskRepository);
                    var diskDetails = asrReplicatedItemInventory.Where(x => x.A2ADiskDetails != null).SelectMany(x => x.A2ADiskDetails);
                    var asrReplicatedItemDiskInventory = await asrReplicatedItemDisk.ProcessInventoryAsync(diskDetails);
                }
                catch (Exception ex) {
                    Log.Error("An exception occurred processing ASR replicated item disks. Stopping processing...", ex);
                }

                try {
                    var asrReplicatedItemHealthErrorRepository =
                        new AsrReplicatedItemHealthErrorRepository<AsrReplicatedItemModel.HealthErrorModel>(_connectionString);
                    var asrReplicatedItemHealthError = new AsrReplicatedItemHealthErrorInventory(_azureService, asrReplicatedItemHealthErrorRepository);
                    var healthErrors = asrReplicatedItemInventory.Where(x => x.ReplicationHealthErrors != null).SelectMany(x => x.ReplicationHealthErrors);
                    var asrReplicatedItemHealthErrorInventory = (await asrReplicatedItemHealthError.ProcessInventoryAsync(healthErrors)).ToList();

                    foreach (var healthError in asrReplicatedItemHealthErrorInventory) {
                        if (healthError.ChildErrors != null) {
                            foreach (var childHealthError in healthError.ChildErrors) {
                                childHealthError.ParentErrorId = healthError.Id;
                            }
                        }
                    }

                    try {
                        var asrReplicatedItemChildHealthErrorRepository =
                            new AsrReplicatedItemHealthErrorRepository<AsrReplicatedItemModel.HealthErrorModel>(_connectionString);
                        var asrReplicatedItemChildHealthError =
                            new AsrReplicatedItemHealthErrorInventory(_azureService, asrReplicatedItemHealthErrorRepository);
                        var childHealthErrors = asrReplicatedItemHealthErrorInventory.Where(x => x.ChildErrors != null).SelectMany(x => x.ChildErrors);
                        var asrReplicatedItemChildHealthErrorInventory = await asrReplicatedItemHealthError.ProcessInventoryAsync(childHealthErrors);
                    }
                    catch (Exception ex) {
                        Log.Error("An exception occurred processing ASR replicated item health errors. Stopping processing...", ex);
                    }
                }
                catch (Exception ex) {
                    Log.Error("An exception occurred processing ASR replicated item health errors. Stopping processing...", ex);
                }
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing ASR replicated items. Stopping processing...", ex);
            }

            try {
                var asrPolicyRepository = new AsrPolicyRepository<AsrPolicyModel>(_connectionString);
                var asrPolicy = new AsrPolicyInventory(_azureService, asrPolicyRepository);
                var asrPolicyInventory = await asrPolicy.ProcessInventoryAsync(await asrPolicy.GetInventoryAsync());
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing ASR policies. Stopping processing...", ex);
            }

            try {
                var adUserRepository = new AdUserRepository<AdUserModel>(_connectionString);
                var adUser = new AdUserInventory(_azureService, adUserRepository);
                var adUserInventory = await adUser.ProcessInventoryAsync(await adUser.GetInventoryAsync());
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing AAD users. Stopping processing...", ex);
            }

            try {
                var adGroupRepository = new AdGroupRepository<AdGroupModel>(_connectionString);
                var adGroup = new AdGroupInventory(_azureService, adGroupRepository);
                var adGroupInventory = (await adGroup.ProcessInventoryAsync(await adGroup.GetInventoryAsync())).ToList();

                try {
                    var adGroupMembershipRepository = new AdGroupMembershipRepository<AdGroupMembershipModel>(_connectionString);
                    var adGroupMembership = new AdGroupMembershipInventory(_azureService, adGroupMembershipRepository);
                    var adGroupMemberships = new List<AdGroupMembershipModel>();

                    foreach (var group in adGroupInventory) {
                        foreach (var objectId in group.UserObjectIds) {
                            adGroupMemberships.Add(new AdGroupMembershipModel {AdGroupId = group.Id, AdUserObjectId = objectId});
                        }

                        foreach (var objectId in group.GroupObjectIds) {
                            adGroupMemberships.Add(new AdGroupMembershipModel {AdGroupId = group.Id, ChildAdGroupObjectId = objectId});
                        }

                        foreach (var objectId in group.ServicePrincipalObjectIds) {
                            adGroupMemberships.Add(new AdGroupMembershipModel {AdGroupId = group.Id, ServicePrincipalObjectId = objectId});
                        }
                    }

                    var adGroupMembershipInventory = await adGroupMembership.ProcessInventoryAsync(adGroupMemberships);
                }
                catch (Exception ex) {
                    Log.Error("An exception occurred processing AAD group memberships. Stopping processing...", ex);
                }
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing AAD groups. Stopping processing...", ex);
            }

            try {
                var adApplicationRepository = new AdApplicationRepository<AdApplicationModel>(_connectionString);
                var adApplication = new AdApplicationInventory(_azureService, adApplicationRepository);
                var adApplicationInventory = await adApplication.ProcessInventoryAsync(await adApplication.GetInventoryAsync());
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing AAD app registrations. Stopping processing...", ex);
            }

            try {
                var adRoleDefinitionRepository = new AdRoleDefinitionRepository<AdRoleDefinitionModel>(_connectionString);
                var adRoleDefinition = new AdRoleDefinitionInventory(_azureService, adRoleDefinitionRepository);
                var roleDefinitionInventory = await adRoleDefinition.ProcessInventoryAsync(await adRoleDefinition.GetInventoryAsync());
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing AAD role definitions. Stopping processing...", ex);
            }

            try {
                var adRoleAssignmentRepository = new AdRoleAssignmentRepository<AdRoleAssignmentModel>(_connectionString);
                var adRoleAssignment = new AdRoleAssignmentInventory(_azureService, adRoleAssignmentRepository);
                var roleAssignmentInventory = await adRoleAssignment.ProcessInventoryAsync(await adRoleAssignment.GetInventoryAsync());
            }
            catch (Exception ex) {
                Log.Error("An exception occurred processing AAD role assignments. Stopping processing...", ex);
            }
        }
    }
}
