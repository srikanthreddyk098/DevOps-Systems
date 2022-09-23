using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models
{
    [Table("WebApp")]
    public class WebAppModel : AzureModel
    {
        public string SubscriptionId { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string Region { get; set; }
        public string Name { get; set; }
        public bool? AlwaysOn { get; set; }
        public string AppServicePlanId { get; set; }
        public string AutoSwapSlotName { get; set; }
        public string AvailabilityState { get; set; }
        public bool? ClientAffinityEnabled { get; set; }
        public bool? ClientCertEnabled { get; set; }
        public string CloningInfo { get; set; }
        public int? ContainerSize { get; set; }
        public int? DailyMemoryTimeQuota { get; set; }
        public string DefaultDocuments { get; set; }
        public string DefaultHostName { get; set; }
        public string DiagnosticLogsConfig { get; set; }
        public string DocumentRoot { get; set; }
        public bool? Enabled { get; set; }
        public string EnabledHostNames { get; set; }
        public string FtpsState { get; set; }
        public string HostingEnvironmentProfile { get; set; }
        public string HostNames { get; set; }
        public bool? HostNamesDisabled { get; set; }
        public string HostNameSslStates { get; set; }
        public bool? Http20Enabled { get; set; }
        public bool? HttpsOnly { get; set; }
        public bool? IsDefaultContainer { get; set; }
        public bool? IsXenon { get; set; }
        public string JavaContainer { get; set; }
        public string JavaContainerVersion { get; set; }
        public string JavaVersion { get; set; }
        public string Key { get; set; }
        public string Kind { get; set; }
        public DateTime? LastModifiedTimeUtc { get; set; }
        public string LastSwapDestination { get; set; }
        public string LastSwapSource { get; set; }
        public DateTime? LastSwapTimestampUtc { get; set; }
        public string LinuxFxVersion { get; set; }
        public bool? LocalMySqlEnabled { get; set; }
        public string ManagedPipelineMode { get; set; }
        public int? MaxNumberOfWorkers { get; set; }
        public string NetFrameworkVersion { get; set; }
        public string NodeVersion { get; set; }
        public string OperatingSystem { get; set; }
        public string OutboundIpAddresses { get; set; }
        public string ParentId { get; set; }
        public string PhpVersion { get; set; }
        public string PossibleOutboundIpAddresses { get; set; }
        public string PlatformArchitecture { get; set; }
        public string PythonVersion { get; set; }
        public bool? RemoteDebuggingEnabled { get; set; }
        public string RemoteDebuggingVersion { get; set; }
        public string RepositorySiteName { get; set; }
        public bool? Reserved { get; set; }
        public bool? ScmSiteAlsoStopped { get; set; }
        public string ScmType { get; set; }
        public string ServerFarmId { get; set; }
        public string SiteConfig { get; set; }
        public string SnapshotInfo { get; set; }
        public string State { get; set; }
        public DateTime? SuspendedTill { get; set; }
        public string SystemAssignedManagedServiceIdentityPrincipalId { get; set; }
        public string SystemAssignedManagedServiceIdentityTenantId { get; set; }
        public string Tags { get; set; }
        public string TargetSwapSlot { get; set; }
        public string TrafficManagerHostNames { get; set; }
        public string Type { get; set; }
        public string UsageState { get; set; }
        public string VirtualApplications { get; set; }
        public bool? WebSocketsEnabled { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDtUtc { get; set; }

        public bool IsEqual(WebAppModel webApp)
        {
            return string.Equals(AzureId, webApp.AzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SubscriptionId, webApp.SubscriptionId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Subscription, webApp.Subscription, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ResourceGroup, webApp.ResourceGroup, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Region, webApp.Region, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Name, webApp.Name, StringComparison.OrdinalIgnoreCase) &&
                   AlwaysOn == webApp.AlwaysOn &&
                   string.Equals(AppServicePlanId, webApp.AppServicePlanId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(AutoSwapSlotName, webApp.AutoSwapSlotName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(AvailabilityState, webApp.AvailabilityState, StringComparison.OrdinalIgnoreCase) &&
                   ClientAffinityEnabled == webApp.ClientAffinityEnabled &&
                   ClientCertEnabled == webApp.ClientCertEnabled &&
                   string.Equals(CloningInfo, webApp.CloningInfo, StringComparison.OrdinalIgnoreCase) &&
                   ContainerSize == webApp.ContainerSize &&
                   DailyMemoryTimeQuota == webApp.DailyMemoryTimeQuota &&
                   string.Equals(DefaultDocuments, webApp.DefaultDocuments, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(DefaultHostName, webApp.DefaultHostName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(DiagnosticLogsConfig, webApp.DiagnosticLogsConfig, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(DocumentRoot, webApp.DocumentRoot, StringComparison.OrdinalIgnoreCase) &&
                   Enabled == webApp.Enabled &&
                   EnabledHostNames == webApp.EnabledHostNames &&
                   string.Equals(FtpsState, webApp.FtpsState, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(HostingEnvironmentProfile, webApp.HostingEnvironmentProfile, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(HostNames, webApp.HostNames, StringComparison.OrdinalIgnoreCase) &&
                   HostNamesDisabled == webApp.HostNamesDisabled &&
                   string.Equals(HostNameSslStates, webApp.HostNameSslStates, StringComparison.OrdinalIgnoreCase) &&
                   Http20Enabled == webApp.Http20Enabled &&
                   HttpsOnly == webApp.HttpsOnly &&
                   IsDefaultContainer == webApp.IsDefaultContainer &&
                   IsXenon == webApp.IsXenon &&
                   string.Equals(JavaContainer, webApp.JavaContainer, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(JavaContainerVersion, webApp.JavaContainerVersion, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(JavaVersion, webApp.JavaVersion, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Key, webApp.Key, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Kind, webApp.Kind, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(LastModifiedTimeUtc, webApp.LastModifiedTimeUtc) &&
                   string.Equals(LastSwapDestination, webApp.LastSwapDestination, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(LastSwapSource, webApp.LastSwapSource, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(LastSwapTimestampUtc, webApp.LastSwapTimestampUtc) &&
                   string.Equals(LinuxFxVersion, webApp.LinuxFxVersion, StringComparison.OrdinalIgnoreCase) &&
                   LocalMySqlEnabled == webApp.LocalMySqlEnabled &&
                   string.Equals(ManagedPipelineMode, webApp.ManagedPipelineMode, StringComparison.OrdinalIgnoreCase) &&
                   MaxNumberOfWorkers == webApp.MaxNumberOfWorkers &&
                   string.Equals(NetFrameworkVersion, webApp.NetFrameworkVersion, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(NodeVersion, webApp.NodeVersion, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(OperatingSystem, webApp.OperatingSystem, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(OutboundIpAddresses, webApp.OutboundIpAddresses, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ParentId, webApp.ParentId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(PhpVersion, webApp.PhpVersion, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(PossibleOutboundIpAddresses, webApp.PossibleOutboundIpAddresses, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(PlatformArchitecture, webApp.PlatformArchitecture, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(PythonVersion, webApp.PythonVersion, StringComparison.OrdinalIgnoreCase) &&
                   RemoteDebuggingEnabled == webApp.RemoteDebuggingEnabled &&
                   string.Equals(RemoteDebuggingVersion, webApp.RemoteDebuggingVersion, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(RepositorySiteName, webApp.RepositorySiteName, StringComparison.OrdinalIgnoreCase) &&
                   Reserved == webApp.Reserved &&
                   ScmSiteAlsoStopped == webApp.ScmSiteAlsoStopped &&
                   string.Equals(ScmType, webApp.ScmType, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ServerFarmId, webApp.ServerFarmId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SiteConfig, webApp.SiteConfig, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SnapshotInfo, webApp.SnapshotInfo, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(State, webApp.State, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(SuspendedTill, webApp.SuspendedTill) &&
                   string.Equals(SystemAssignedManagedServiceIdentityPrincipalId, webApp.SystemAssignedManagedServiceIdentityPrincipalId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SystemAssignedManagedServiceIdentityTenantId, webApp.SystemAssignedManagedServiceIdentityTenantId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Tags, webApp.Tags, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(TargetSwapSlot, webApp.TargetSwapSlot, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(TrafficManagerHostNames, webApp.TrafficManagerHostNames, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Type, webApp.Type, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(UsageState, webApp.UsageState, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(VirtualApplications, webApp.VirtualApplications, StringComparison.OrdinalIgnoreCase) &&
                   WebSocketsEnabled == webApp.WebSocketsEnabled &&
                   string.Equals(CreatedBy, webApp.CreatedBy, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(CreatedDtUtc, webApp.CreatedDtUtc);
        }
    }
}