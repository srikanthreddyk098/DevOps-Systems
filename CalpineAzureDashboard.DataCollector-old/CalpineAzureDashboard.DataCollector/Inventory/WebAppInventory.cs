using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.Models;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Newtonsoft.Json;

namespace CalpineAzureDashboard.DataCollector.Inventory
{
    public class WebAppInventory : AzureInventory<WebAppModel>
    {
        public WebAppInventory(AzureService azureService, IRepository<WebAppModel> repository) : base(azureService, repository, "web app", "Create or Update Web App") { }

        public override async Task<IEnumerable<WebAppModel>> GetInventoryAsync(IEnumerable<WebAppModel> inventoryParam = null)
        {
            if (inventoryParam != null) {
                return inventoryParam;
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s started at: {DateTime.Now}");
            var inventory = new List<WebAppModel>();

            foreach (var subscription in Subscriptions) {
                try {
                    var webApps = await AzureService.GetWebAppsAsync(subscription.SubscriptionId);

                    foreach (var webApp in webApps) {
                        try {
                            inventory.Add(GetWebAppModel(webApp, subscription));

                            var slots = await webApp.DeploymentSlots.ListAsync();
                            foreach (var slot in slots) {
                                var newWebAppSlot = GetWebAppModel(slot, subscription);
                                newWebAppSlot.ParentId = slot.Parent.Id;

                                inventory.Add(newWebAppSlot);
                            }
                        }
                        catch (Exception ex) {
                            Log.Error($"An exception occurred getting details for {InventoryType}: {webApp.Id}", ex);
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

        private WebAppModel GetWebAppModel(IWebAppBase webApp, ISubscription subscription)
        {
            var newWebApp = new WebAppModel();
            newWebApp.AzureId = webApp.Id;
            newWebApp.SubscriptionId = subscription.SubscriptionId;
            newWebApp.Subscription = subscription.DisplayName;
            newWebApp.ResourceGroup = webApp.ResourceGroupName;
            newWebApp.Region = webApp.RegionName;
            newWebApp.Name = webApp.Name;
            newWebApp.AlwaysOn = webApp.AlwaysOn;
            newWebApp.AppServicePlanId = webApp.AppServicePlanId;
            newWebApp.AutoSwapSlotName = webApp.AutoSwapSlotName;
            newWebApp.AvailabilityState = webApp.AvailabilityState.ToString();
            newWebApp.ClientAffinityEnabled = webApp.ClientAffinityEnabled;
            newWebApp.ClientCertEnabled = webApp.ClientCertEnabled;
            newWebApp.CloningInfo = JsonConvert.SerializeObject(webApp.CloningInfo);
            newWebApp.ContainerSize = webApp.ContainerSize;
            newWebApp.DailyMemoryTimeQuota = webApp.Inner.DailyMemoryTimeQuota;
            newWebApp.DefaultDocuments = JsonConvert.SerializeObject(webApp.DefaultDocuments);
            newWebApp.DefaultHostName = webApp.DefaultHostName;
            newWebApp.DiagnosticLogsConfig = JsonConvert.SerializeObject(webApp.DiagnosticLogsConfig);
            newWebApp.DocumentRoot = webApp.DocumentRoot;
            newWebApp.Enabled = webApp.Enabled;
            newWebApp.EnabledHostNames = webApp.EnabledHostNames?.Count > 0 ? string.Join(";", webApp.EnabledHostNames) : null;
            newWebApp.FtpsState = JsonConvert.SerializeObject(webApp.FtpsState);
            newWebApp.HostingEnvironmentProfile = JsonConvert.SerializeObject(webApp.Inner.HostingEnvironmentProfile);
            newWebApp.HostNames = webApp.HostNames?.Count > 0 ? string.Join(";", webApp.HostNames) : null;
            newWebApp.HostNamesDisabled = webApp.HostNamesDisabled;
            newWebApp.HostNameSslStates = webApp.HostNameSslStates?.Count > 0 ? string.Join(";", webApp.HostNameSslStates) : null;
            newWebApp.Http20Enabled = webApp.Http20Enabled;
            newWebApp.HttpsOnly = webApp.HttpsOnly;
            newWebApp.IsDefaultContainer = webApp.IsDefaultContainer;
            newWebApp.IsXenon = webApp.Inner.IsXenon;
            newWebApp.JavaContainer = webApp.JavaContainer;
            newWebApp.JavaContainerVersion = webApp.JavaContainerVersion;
            newWebApp.JavaVersion = webApp.JavaVersion?.Value;
            newWebApp.Key = webApp.Key;
            newWebApp.Kind = webApp.Inner.Kind;
            newWebApp.LastModifiedTimeUtc = webApp.Inner.LastModifiedTimeUtc;
            newWebApp.LastSwapDestination = webApp.Inner.SlotSwapStatus?.DestinationSlotName;
            newWebApp.LastSwapSource = webApp.Inner.SlotSwapStatus?.SourceSlotName;
            newWebApp.LastSwapTimestampUtc = webApp.Inner.SlotSwapStatus?.TimestampUtc;
            newWebApp.LinuxFxVersion = webApp.LinuxFxVersion;
            newWebApp.LocalMySqlEnabled = webApp.LocalMySqlEnabled;
            newWebApp.ManagedPipelineMode = webApp.ManagedPipelineMode.ToString();
            newWebApp.MaxNumberOfWorkers = webApp.Inner.MaxNumberOfWorkers;
            newWebApp.NetFrameworkVersion = webApp.NetFrameworkVersion?.Value;
            newWebApp.NodeVersion = webApp.NodeVersion;
            newWebApp.OperatingSystem = webApp.OperatingSystem.ToString();
            newWebApp.OutboundIpAddresses = webApp.OutboundIPAddresses?.Count > 0 ? string.Join(";", webApp.OutboundIPAddresses) : null;
            newWebApp.PhpVersion = webApp.PhpVersion?.Value;
            newWebApp.PossibleOutboundIpAddresses = webApp.Inner.PossibleOutboundIpAddresses;
            newWebApp.PlatformArchitecture = webApp.PlatformArchitecture.ToString();
            newWebApp.PythonVersion = webApp.PythonVersion?.Value;
            newWebApp.RemoteDebuggingEnabled = webApp.RemoteDebuggingEnabled;
            newWebApp.RemoteDebuggingVersion = webApp.RemoteDebuggingVersion?.Value;
            newWebApp.RepositorySiteName = webApp.RepositorySiteName;
            newWebApp.Reserved = webApp.Inner.Reserved;
            newWebApp.ScmSiteAlsoStopped = webApp.ScmSiteAlsoStopped;
            newWebApp.ScmType = webApp.ScmType?.Value;
            newWebApp.ServerFarmId = webApp.Inner.ServerFarmId;
            newWebApp.SiteConfig = JsonConvert.SerializeObject(webApp.Inner.SiteConfig);
            //newWebApp.SnapshotInfo = JsonConvert.SerializeObject(webApp.Inner.SnapshotInfo);
            newWebApp.State = webApp.State;
            newWebApp.SuspendedTill = webApp.Inner.SuspendedTill;
            newWebApp.SystemAssignedManagedServiceIdentityPrincipalId = webApp.SystemAssignedManagedServiceIdentityPrincipalId;
            newWebApp.SystemAssignedManagedServiceIdentityTenantId = webApp.SystemAssignedManagedServiceIdentityTenantId;
            newWebApp.TargetSwapSlot = webApp.TargetSwapSlot;
            newWebApp.TrafficManagerHostNames = webApp.TrafficManagerHostNames?.Count > 0 ? string.Join(";", webApp.TrafficManagerHostNames) : null;
            newWebApp.Type = webApp.Type;
            newWebApp.UsageState = webApp.UsageState.ToString();
            newWebApp.VirtualApplications = JsonConvert.SerializeObject(webApp.VirtualApplications);
            newWebApp.WebSocketsEnabled = webApp.WebSocketsEnabled;

            if (webApp.Inner.Tags != null && webApp.Inner.Tags.Any()) {
                newWebApp.Tags = JsonConvert.SerializeObject(webApp.Inner.Tags);
            }

            return newWebApp;
        }

        public override async Task<IEnumerable<WebAppModel>> ProcessInventoryAsync(IEnumerable<WebAppModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var itemsToInsert = new List<WebAppModel>();
            var itemsToUpdate = new List<WebAppModel>();

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
                        //get the first "Create or Update" event to determine who created the resource and when
                        try {
                            var createdEvent = GetCreatedEvent(item.SubscriptionId, item.ResourceGroup, item.AzureId, item.CreatedDtUtc,
                                item.CreatedDtUtc?.Add(new TimeSpan(0, 0, 1)));
                            item.CreatedBy = createdEvent?.Caller;
                            item.CreatedDtUtc = createdEvent?.EventTimestamp;
                        }
                        catch (Exception ex) {
                            Log.Error($"Something went wrong getting created event from the activity logs for {InventoryType} with Azure id: {item.AzureId}.", ex);
                        }

                        itemsToInsert.Add(item);
                    }
                    else {
                        item.Id = existingItem.Id;
                        item.CreatedBy = existingItem.CreatedBy;
                        item.CreatedDtUtc = existingItem.CreatedDtUtc;

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