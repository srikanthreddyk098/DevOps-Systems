using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using CalpineAzureDashboard.Models.PowerShellModels;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Graph.RBAC.Fluent;
using Microsoft.Azure.Management.Monitor.Fluent;
using Microsoft.Azure.Management.Monitor.Fluent.Models;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.RecoveryServices;
using Microsoft.Azure.Management.RecoveryServices.Backup;
using Microsoft.Azure.Management.RecoveryServices.Backup.Models;
using Microsoft.Azure.Management.RecoveryServices.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Rest.Azure;
using Microsoft.Rest.Azure.OData;
using Newtonsoft.Json;

namespace CalpineAzureDashboard.Azure
{
    public class AzureService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _tenantId;

        public AzureService(string clientId, string clientSecret, string tenantId)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _tenantId = tenantId;
        }

        public async Task<IPagedCollection<ISubscription>> GetAllSubscriptionsAsync()
        {
            return await Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).Subscriptions.ListAsync();
        }

        public async Task<IPagedCollection<IResourceGroup>> GetResourceGroupsInSubscriptionAsync(string subscriptionId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.ResourceGroups.ListAsync();
        }

        public async Task<IPagedCollection<IVirtualMachine>> GetVirtualMachinesInSubscriptionAsync(string subscriptionId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.VirtualMachines.ListAsync();
        }

        public async Task<IVirtualMachine> GetVirtualMachineByIdAsync(string subscriptionId, string vmId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.VirtualMachines.GetByIdAsync(vmId);
        }

        public IVirtualMachine GetVirtualMachineById(string subscriptionId, string vmId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return azure.VirtualMachines.GetById(vmId);
        }

        public async Task<IVirtualMachine> GetVirtualMachineByResourceGroupAsync(string subscriptionId, string resourceGroupName,
            string vmName)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.VirtualMachines.GetByResourceGroupAsync(resourceGroupName, vmName);
        }

        public async Task StartVm(IVirtualMachine azureVm)
        {
            await azureVm.StartAsync();
        }

        public async Task StopVm(IVirtualMachine azureVm)
        {
            await azureVm.DeallocateAsync();
        }

        public async Task<IPagedCollection<IActiveDirectoryUser>> GetAzureAdUsersAsync()
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials());
            return await azure.ActiveDirectoryUsers.ListAsync();
        }

        public async Task<IPagedCollection<IActiveDirectoryGroup>> GetAzureAdGroupsAsync()
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials());
            return await azure.ActiveDirectoryGroups.ListAsync();
        }

        public async Task<IPagedCollection<IActiveDirectoryApplication>> GetAzureAdApplicationsAsync()
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials());
            return await azure.ActiveDirectoryApplications.ListAsync();
        }

        public async Task<IPagedCollection<IRoleAssignment>> GetAzureAdSubscriptionRoleAssignmentsAsync(string subscriptionId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials());
            //return await azure.RoleAssignments.ListByScopeAsync()
            return await azure.RoleAssignments.ListByScopeAsync($"subscriptions/{subscriptionId}");
        }

        public async Task<IRoleAssignment> GetAzureAdRoleAssignmentAsync(string roleAssignmentId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials());
            //return await azure.RoleAssignments.ListByScopeAsync()
            return await azure.RoleAssignments.GetByIdAsync(roleAssignmentId);
        }

        public async Task<IEnumerable<IRoleDefinition>> GetAzureAdSubscriptionRoleDefinitionsAsync(string subscriptionId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials());
            //return await azure.RoleAssignments.ListByScopeAsync()
            return await azure.RoleDefinitions.ListByScopeAsync($"subscriptions/{subscriptionId}");
        }

        public async Task<IRoleDefinition> GetAzureAdRoleDefinitionAsync(string roleDefinitionId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials());
            //return await azure.RoleAssignments.ListByScopeAsync()
            return await azure.RoleDefinitions.GetByIdAsync(roleDefinitionId);
        }

        public PowerState GetVmStatus(IVirtualMachine azureVm)
        {
            return azureVm.PowerState;
        }

        public async Task<IPagedCollection<IVirtualMachineSize>> GetVirtualMachineSizesByRegionAsync(string region)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithDefaultSubscription();
            return await azure.VirtualMachines.Sizes.ListByRegionAsync(region);
        }

        public async Task<IEnumerable<IVirtualMachineSize>> GetAllVirtualMachineSizesAsync(string subscriptionId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            var regions = GetAllRegions().Where(x => x.Name.ToUpper().EndsWith("US") || x.Name.ToUpper().EndsWith("US2"));

            var sizes = new List<IVirtualMachineSize>();
            foreach (var region in regions) {
                if (sizes.Count > 0) {
                    var newSizes = await azure.VirtualMachines.Sizes.ListByRegionAsync(region.Name);
                    sizes.AddRange(newSizes.Where(x => !sizes.Any(y => x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase))));
                }
                else {
                    sizes.AddRange(await azure.VirtualMachines.Sizes.ListByRegionAsync(region.Name));
                }
            }

            return sizes;
        }

        public IReadOnlyCollection<Region> GetAllRegions()
        {
            return Region.Values;
        }

        public async Task<IPagedCollection<INetwork>> GetVirtualNetworksAsync(string subscriptionId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.Networks.ListAsync();
        }

        public async Task<INetwork> GetVirtualNetworkAsync(string subscriptionId, string virtualNetworkId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.Networks.GetByIdAsync(virtualNetworkId);
        }

        public async Task<IPagedCollection<INetworkInterface>> GetNetworkInterfacesAsync(string subscriptionId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.NetworkInterfaces.ListAsync();
        }

        public async Task<INetworkInterface> GetNetworkInterfaceAsync(string subscriptionId, string networkInterfaceId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.NetworkInterfaces.GetByIdAsync(networkInterfaceId);
        }

        public async Task<IPagedCollection<INetworkSecurityGroup>> GetNetworkSecurityGroupsAsync(string subscriptionId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.NetworkSecurityGroups.ListAsync();
        }

        public async Task<INetworkSecurityGroup> GetNetworkSecurityGroupAsync(string subscriptionId,
            string networkSecurityGroupId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.NetworkSecurityGroups.GetByIdAsync(networkSecurityGroupId);
        }

        public async Task<IPagedCollection<INetwork>> GetNetworksAsync(string subscriptionId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.Networks.ListAsync();
        }

        public async Task<IPagedCollection<IPublicIPAddress>> GetPublicIpAddressesAsync(string subscriptionId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.PublicIPAddresses.ListAsync();
        }

        public async Task<IPublicIPAddress> GetPublicIpAddressAsync(string subscriptionId, string publicIpAddressId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.PublicIPAddresses.GetByIdAsync(publicIpAddressId);
        }

        public async Task<IPagedCollection<ILoadBalancer>> GetLoadBalancersAsync(string subscriptionId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.LoadBalancers.ListAsync();
        }

        public async Task<ILoadBalancer> GetLoadBalancerAsync(string subscriptionId, string loadBalancerId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.LoadBalancers.GetByIdAsync(loadBalancerId);
        }

        public async Task<IPagedCollection<IApplicationGateway>> GetApplicationGatewaysAsync(string subscriptionId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials())
                .WithSubscription(subscriptionId);
            return await azure.ApplicationGateways.ListAsync();
        }

        public async Task<IApplicationGateway> GetApplicationGatewayAsync(string subscriptionId,
            string applicationGatewayId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.ApplicationGateways.GetByIdAsync(applicationGatewayId);
        }

        public async Task<IPagedCollection<IAvailabilitySet>> GetAvailabilitySetsAsync(string subscriptionId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.AvailabilitySets.ListAsync();
        }

        public Task<IAvailabilitySet> GetAvailabilitySetAsync(string subscriptionId, string availabilitySetId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return azure.AvailabilitySets.GetByIdAsync(availabilitySetId);
        }

        /// <summary>
        /// This ListAsync call throws a "nextPageLink cannot be null" exception for certain subscriptions. use the non-async method instead
        /// </summary>
        public Task<IPagedCollection<IDisk>> GetDataDisksAsync(string subscriptionId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return azure.Disks.ListAsync();
        }

        public IEnumerable<IDisk> GetDataDisks(string subscriptionId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return azure.Disks.List();
        }

        public Task<IDisk> GetDataDiskByIdAsync(string subscriptionId, string dataDiskId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return azure.Disks.GetByIdAsync(dataDiskId);
        }

        public Task<IPagedCollection<IStorageAccount>> GetStorageAccountsAsync(string subscriptionId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return azure.StorageAccounts.ListAsync();
        }

        public Task<IStorageAccount> GetStorageAccountByIdAsync(string subscriptionId, string storageAccountId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return azure.StorageAccounts.GetByIdAsync(storageAccountId);
        }

        public Task<IStorageAccount> GetStorageAccountByResourceGroupNameAsync(string subscriptionId, string resourceGroupName, string storageAccountName)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return azure.StorageAccounts.GetByResourceGroupAsync(resourceGroupName, storageAccountName);
        }

        public Task<IPagedCollection<IWebApp>> GetWebAppsAsync(string subscriptionId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return azure.WebApps.ListAsync();
        }

        public async Task<IEnumerable<IEventData>> GetActivityLogsByResourceIdAsync(string subscriptionId, string resourceGroup, string resourceId,
            DateTime? startDateTimeUtc = null, DateTime? endDateTimeUtc = null)
        {
            if (startDateTimeUtc != null && startDateTimeUtc < DateTime.UtcNow.AddDays(-89)) {
                startDateTimeUtc = DateTime.UtcNow.AddDays(-85);
            }

            if (endDateTimeUtc == null || endDateTimeUtc > DateTime.UtcNow) {
                endDateTimeUtc = DateTime.UtcNow.AddDays(-1);
            }

            endDateTimeUtc = new DateTime(endDateTimeUtc.Value.Year, endDateTimeUtc.Value.Month, endDateTimeUtc.Value.Day, 23, 59, 59);

            if (startDateTimeUtc == null) {
                startDateTimeUtc = endDateTimeUtc.Value.AddDays(-85);
            }

            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.ActivityLogs.DefineQuery().StartingFrom(startDateTimeUtc.Value).EndsBefore(endDateTimeUtc.Value).WithAllPropertiesInResponse()
                .FilterByResource(resourceId).ExecuteAsync();
        }

        public IEnumerable<IEventData> GetActivityLogsByResourceId(string subscriptionId, string resourceGroup, string resourceId, DateTime? startDateTimeUtc = null,
            DateTime? endDateTimeUtc = null)
        {
            if (startDateTimeUtc != null && startDateTimeUtc < DateTime.UtcNow.AddDays(-89)) {
                startDateTimeUtc = DateTime.UtcNow.AddDays(-85);
            }

            if (endDateTimeUtc == null || endDateTimeUtc > DateTime.UtcNow) {
                endDateTimeUtc = DateTime.UtcNow.AddDays(-1);
            }

            endDateTimeUtc = new DateTime(endDateTimeUtc.Value.Year, endDateTimeUtc.Value.Month, endDateTimeUtc.Value.Day, 23, 59, 59);

            if (startDateTimeUtc == null) {
                startDateTimeUtc = endDateTimeUtc.Value.AddDays(-85);
            }

            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return azure.ActivityLogs.DefineQuery().StartingFrom(startDateTimeUtc.Value).EndsBefore(endDateTimeUtc.Value).WithAllPropertiesInResponse()
                .FilterByResource(resourceId).Execute();
        }

        public async Task<IEnumerable<IEventData>> GetActivityLogsByResourceProviderAsync(string subscriptionId,
            string resourceProvider, DateTime? startDateTimeUtc = null, DateTime? endDateTimeUtc = null)
        {
            if (startDateTimeUtc!= null && startDateTimeUtc < DateTime.UtcNow.AddDays(-89)) {
                startDateTimeUtc = DateTime.UtcNow.AddDays(-85);
            }

            if (endDateTimeUtc == null || endDateTimeUtc > DateTime.UtcNow) {
                endDateTimeUtc = DateTime.UtcNow.AddDays(-1);
            }

            endDateTimeUtc = new DateTime(endDateTimeUtc.Value.Year, endDateTimeUtc.Value.Month, endDateTimeUtc.Value.Day, 23, 59, 59);

            if (startDateTimeUtc == null) {
                startDateTimeUtc = endDateTimeUtc.Value.AddDays(-85);
            }
            
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.ActivityLogs.DefineQuery().StartingFrom(startDateTimeUtc.Value).EndsBefore(endDateTimeUtc.Value).WithAllPropertiesInResponse()
                .FilterByResourceProvider(resourceProvider).ExecuteAsync();
        }

        public async Task<IEnumerable<Vault>> GetRecoveryServicesVaultsAsync(string subscriptionId)
        {
            var vaultClient = new RecoveryServicesClient(GetAzureCredentials()) {SubscriptionId = subscriptionId};
            return await vaultClient.Vaults.ListBySubscriptionIdAsync();
        }

        public async Task<IPage<ProtectedItemResource>> GetVaultProtectedItemsAsync(string subscriptionId, string vaultName, string resourceGroupName)
        {
            var backupClient = new RecoveryServicesBackupClient(GetAzureCredentials()) {SubscriptionId = subscriptionId};
            return await backupClient.BackupProtectedItems.ListAsync(vaultName, resourceGroupName);
        }

        public async Task<PowerShellModel<AsrReplicatedItemDeserializer>> GetAsrReplicatedItemsAsync(string subscriptionId)
        {
            return await RunPowerShellScript<AsrReplicatedItemDeserializer>(subscriptionId);
        }

        public async Task<PowerShellModel<AsrPolicyDeserializer>> GetAsrPoliciesAsync(string subscriptionId)
        {
            return await RunPowerShellScript<AsrPolicyDeserializer>(subscriptionId);
        }

        private async Task<PowerShellModel<T>> RunPowerShellScript<T>(string subscriptionId)
        {
            var items = new List<T>();
            var scriptName = typeof(T).Name.Replace("Deserializer", "") + ".ps1";

            using (var powerShell = PowerShell.Create()) {
                powerShell.AddCommand("Set-ExecutionPolicy").AddArgument("RemoteSigned").AddArgument("CurrentUser");
                var args = $"-username {_clientId} -password {_clientSecret} -tenantId {_tenantId} -subscriptionId {subscriptionId}";
                powerShell.AddScript(System.IO.Path.Combine(Environment.CurrentDirectory, $"PowerShellScripts\\{scriptName}") + $" {args}");
                var asyncResult = powerShell.BeginInvoke();
                while (!asyncResult.IsCompleted) {
                    await Task.Delay(5000);
                }

                var results = powerShell.EndInvoke(asyncResult);
                foreach (var obj in results) {
                    items.Add(JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(obj.BaseObject)));
                }

                var model = new PowerShellModel<T>(items, powerShell.HadErrors, powerShell.Streams.Error);
                return model;
            }
        }

        public async Task<IEnumerable<ReplicationUsage>> GetVaultReplicatedItemsAsync(string subscriptionId, string vaultName, string resourceGroupName)
        {
            var backupClient = new RecoveryServicesClient(GetAzureCredentials()) { SubscriptionId = subscriptionId };
            return await backupClient.ReplicationUsages.ListAsync(resourceGroupName, vaultName);
        }

        public async Task<IPage<ProtectionPolicyResource>> GetBackupPoliciesAsync(string subscriptionId, string vaultName, string resourceGroupName)
        {
            var backupClient = new RecoveryServicesBackupClient(GetAzureCredentials()) {SubscriptionId = subscriptionId};
            return await backupClient.BackupPolicies.ListAsync(vaultName, resourceGroupName);
        }

        public async Task<IPage<JobResource>> GetBackupJobsAsync(string subscriptionId, string vaultName, string resourceGroupName, DateTime startTime, DateTime endTime)
        {
            var backupClient = new RecoveryServicesBackupClient(GetAzureCredentials()) {SubscriptionId = subscriptionId};
            var oDataQuery = new ODataQuery<JobQueryObject> {Filter = $"startTime eq '{startTime:yyyy-MM-dd hh:mm:ss tt}' and endTime eq '{endTime:yyyy-MM-dd hh:mm:ss tt}'"};
            return await backupClient.BackupJobs.ListAsync(vaultName, resourceGroupName, oDataQuery);
        }

        public async Task<IPagedCollection<IMetricAlert>> GetMetricAlertsAsync(string subscriptionId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.AlertRules.MetricAlerts.ListAsync();
        }

        public async Task<IPagedCollection<IActivityLogAlert>> GetLogAnalyticsAlertsAsync(string subscriptionId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.AlertRules.ActivityLogAlerts.ListAsync();
        }

        public async Task<IDiagnosticSetting> GetDiagnosticSettingsByResourceIdAsync(string subscriptionId, string resourceId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.DiagnosticSettings.GetByIdAsync(resourceId);
        }

        private AzureCredentials GetAzureCredentials()
        {
            return SdkContext.AzureCredentialsFactory.FromServicePrincipal(_clientId, _clientSecret, _tenantId, AzureEnvironment.AzureGlobalCloud);
        }
    }
}