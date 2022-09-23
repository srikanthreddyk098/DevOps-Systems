using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models
{
    [Table("StorageAccount")]
    public class StorageAccountModel : AzureModel
    {
        public string SubscriptionId { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string Region { get; set; }
        public string Name { get; set; }
        public string SkuName { get; set; }
        public string SkuTier { get; set; }
        public string Kind { get; set; }
        public string AccessTier { get; set; }
        public DateTime? CreationTime { get; set; }
        public int? BlobSizeInGb { get; set; }
        public int? NumberOfBlobContainers { get; set; }
        public int? NumberOfBlobs { get; set; }
        public int? FileShareSizeInGb { get; set; }
        public int? NumberOfFileShares { get; set; }
        public string CustomDomain { get; set; }
        public bool? UseSubDomain { get; set; }
        public bool? EnableHttpsTrafficOnly { get; set; }
        public string KeySource { get; set; }
        public string KeyVaultName { get; set; }
        public string KeyVaultUri { get; set; }
        public string KeyVaultVersion { get; set; }
        public bool? BlobEncryptionEnabled { get; set; }
        public DateTime? BlobEncryptionLastEnabledTime { get; set; }
        public bool? FileEncryptionEnabled { get; set; }
        public DateTime? FileEncryptionLastEnabledTime { get; set; }
        public bool? QueueEncryptionEnabled { get; set; }
        public DateTime? QueueEncryptionLastEnabledTime { get; set; }
        public bool? TableEncryptionEnabled { get; set; }
        public DateTime? TableEncryptionLastEnabledTime { get; set; }
        public string SystemAssignedManagedServiceIdentityPrincipalId { get; set; }
        public string SystemAssignedManagedServiceIdentityTenantId { get; set; }
        public DateTime? LastGeoFailoverTime { get; set; }
        public bool? IsAccessAllowedFromAllNetworks { get; set; }
        public bool? CanAccessFromAzureServices { get; set; }
        public bool? CanReadMetricsFromAnyNetwork { get; set; }
        public bool? CanReadLogEntriesFromAnyNetwork { get; set; }
        public string IpAddressesWithAccess { get; set; }
        public string IpAddressRangesWithAccess { get; set; }
        public string NetworkSubnetsWithAccess { get; set; }
        public string ProvisioningState { get; set; }
        public string PrimaryLocation { get; set; }
        public string SecondaryLocation { get; set; }
        public string PrimaryStatus { get; set; }
        public string SecondaryStatus { get; set; }
        public string PrimaryBlobEndpoint { get; set; }
        public string PrimaryFileEndpoint { get; set; }
        public string PrimaryQueueEndpoint { get; set; }
        public string PrimaryTableEndpoint { get; set; }
        public string SecondaryBlobEndpoint { get; set; }
        public string SecondaryFileEndpoint { get; set; }
        public string SecondaryQueueEndpoint { get; set; }
        public string SecondaryTableEndpoint { get; set; }
        public string Tags { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDtUtc { get; set; }

        public bool IsEqual(StorageAccountModel storageAccount)
        {
            return string.Equals(AzureId, storageAccount.AzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SubscriptionId, storageAccount.SubscriptionId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Subscription, storageAccount.Subscription, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ResourceGroup, storageAccount.ResourceGroup, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Region, storageAccount.Region, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Name, storageAccount.Name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SkuName, storageAccount.SkuName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SkuTier, storageAccount.SkuTier, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Kind, storageAccount.Kind, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(AccessTier, storageAccount.AccessTier, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(CreationTime, storageAccount.CreationTime) &&
                   BlobSizeInGb == storageAccount.BlobSizeInGb &&
                   NumberOfBlobContainers == storageAccount.NumberOfBlobContainers &&
                   NumberOfBlobs == storageAccount.NumberOfBlobs &&
                   FileShareSizeInGb == storageAccount.FileShareSizeInGb &&
                   NumberOfFileShares == storageAccount.NumberOfFileShares &&
                   string.Equals(CustomDomain, storageAccount.CustomDomain, StringComparison.OrdinalIgnoreCase) &&
                   UseSubDomain == storageAccount.UseSubDomain &&
                   EnableHttpsTrafficOnly == storageAccount.EnableHttpsTrafficOnly &&
                   string.Equals(KeySource, storageAccount.KeySource, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(KeyVaultName, storageAccount.KeyVaultName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(KeyVaultUri, storageAccount.KeyVaultUri, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(KeyVaultVersion, storageAccount.KeyVaultVersion, StringComparison.OrdinalIgnoreCase) &&
                   BlobEncryptionEnabled == storageAccount.BlobEncryptionEnabled &&
                   IsDateEqual(BlobEncryptionLastEnabledTime, storageAccount.BlobEncryptionLastEnabledTime) &&
                   FileEncryptionEnabled == storageAccount.FileEncryptionEnabled &&
                   IsDateEqual(FileEncryptionLastEnabledTime, storageAccount.FileEncryptionLastEnabledTime) &&
                   QueueEncryptionEnabled == storageAccount.QueueEncryptionEnabled &&
                   IsDateEqual(QueueEncryptionLastEnabledTime, storageAccount.QueueEncryptionLastEnabledTime) &&
                   TableEncryptionEnabled == storageAccount.TableEncryptionEnabled &&
                   IsDateEqual(TableEncryptionLastEnabledTime, storageAccount.TableEncryptionLastEnabledTime) &&
                   string.Equals(SystemAssignedManagedServiceIdentityPrincipalId, storageAccount.SystemAssignedManagedServiceIdentityPrincipalId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SystemAssignedManagedServiceIdentityTenantId, storageAccount.SystemAssignedManagedServiceIdentityTenantId, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(LastGeoFailoverTime, storageAccount.LastGeoFailoverTime) &&
                   IsAccessAllowedFromAllNetworks == storageAccount.IsAccessAllowedFromAllNetworks &&
                   CanAccessFromAzureServices == storageAccount.CanAccessFromAzureServices &&
                   CanReadMetricsFromAnyNetwork == storageAccount.CanReadMetricsFromAnyNetwork &&
                   CanReadLogEntriesFromAnyNetwork == storageAccount.CanReadLogEntriesFromAnyNetwork &&
                   string.Equals(IpAddressesWithAccess, storageAccount.IpAddressesWithAccess, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(IpAddressRangesWithAccess, storageAccount.IpAddressRangesWithAccess, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(NetworkSubnetsWithAccess, storageAccount.NetworkSubnetsWithAccess, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ProvisioningState, storageAccount.ProvisioningState, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(PrimaryLocation, storageAccount.PrimaryLocation, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SecondaryLocation, storageAccount.SecondaryLocation, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(PrimaryStatus, storageAccount.PrimaryStatus, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SecondaryStatus, storageAccount.SecondaryStatus, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(PrimaryBlobEndpoint, storageAccount.PrimaryBlobEndpoint, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(PrimaryFileEndpoint, storageAccount.PrimaryFileEndpoint, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(PrimaryQueueEndpoint, storageAccount.PrimaryQueueEndpoint, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(PrimaryTableEndpoint, storageAccount.PrimaryTableEndpoint, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SecondaryBlobEndpoint, storageAccount.SecondaryBlobEndpoint, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SecondaryFileEndpoint, storageAccount.SecondaryFileEndpoint, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SecondaryQueueEndpoint, storageAccount.SecondaryQueueEndpoint, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SecondaryTableEndpoint, storageAccount.SecondaryTableEndpoint, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Tags, storageAccount.Tags, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(CreatedBy, storageAccount.CreatedBy, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(CreatedDtUtc, storageAccount.CreatedDtUtc);
        }

        public StorageAccountModel ShallowCopy()
        {
            return (StorageAccountModel) MemberwiseClone();
        }
    }
}