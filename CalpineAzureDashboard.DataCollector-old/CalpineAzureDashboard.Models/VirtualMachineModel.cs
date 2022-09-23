using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models
{
    [Table("VirtualMachine")]
    public class VirtualMachineModel : AzureModel
    {
        public string SubscriptionId { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string Region { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Os { get; set; }
        public string OsSku { get; set; }
        public string OsPublisher { get; set; }
        public string OsType { get; set; }
        public string OsVersion { get; set; }
        public string LicenseType { get; set; }
        public string Size { get; set; }
        public int? Cores { get; set; }
        public int? Memory { get; set; }
        public string AvailabilitySetId { get; set; }
        public int? NumberOfNics { get; set; }
        public string PrimaryNicId { get; set; }
        public string OsDisk { get; set; }
        public string OsDiskSku { get; set; }
        public int? OsDiskSize { get; set; }
        public bool? IsOsDiskEncrypted { get; set; }
        public bool? IsManagedDiskEnabled { get; set; }
        public int? NumberOfDataDisks { get; set; }
        public string AzureAgentProvisioningState { get; set; }
        public string AzureAgentVersion { get; set; }
        public string Tags { get; set; }
        public string TagApplicationName { get; set; }
        public string TagProjectCode { get; set; }
        public string TagProjectDurationStart { get; set; }
        public string TagProjectDurationEnd { get; set; }
        public string TagBackupPolicy { get; set; }
        public string TagBackupFrequency { get; set; }
        public string TagReservedInstance { get; set; }
        public string TagServerType { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDtUtc { get; set; }
        public IEnumerable<VirtualMachineExtensionModel> Extensions { get; set; }

        public bool IsEqual(VirtualMachineModel vm)
        {
            return string.Equals(AzureId, vm.AzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SubscriptionId, vm.SubscriptionId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Subscription, vm.Subscription, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ResourceGroup, vm.ResourceGroup, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Region, vm.Region, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Name, vm.Name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Status, vm.Status, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Os, vm.Os, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(OsSku, vm.OsSku, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(OsPublisher, vm.OsPublisher, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(OsType, vm.OsType, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(OsVersion, vm.OsVersion, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(LicenseType, vm.LicenseType, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Size, vm.Size, StringComparison.OrdinalIgnoreCase) &&
                   Cores == vm.Cores &&
                   Memory == vm.Memory &&
                   string.Equals(AvailabilitySetId, vm.AvailabilitySetId, StringComparison.OrdinalIgnoreCase) &&
                   NumberOfNics == vm.NumberOfNics &&
                   string.Equals(PrimaryNicId, vm.PrimaryNicId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(OsDisk, vm.OsDisk, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(OsDiskSku, vm.OsDiskSku, StringComparison.OrdinalIgnoreCase) &&
                   OsDiskSize == vm.OsDiskSize &&
                   IsOsDiskEncrypted == vm.IsOsDiskEncrypted &&
                   IsManagedDiskEnabled == vm.IsManagedDiskEnabled &&
                   NumberOfDataDisks == vm.NumberOfDataDisks &&
                   string.Equals(AzureAgentProvisioningState, vm.AzureAgentProvisioningState, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(AzureAgentVersion, vm.AzureAgentVersion, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Tags, vm.Tags, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(TagApplicationName, vm.TagApplicationName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(TagProjectCode, vm.TagProjectCode, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(TagProjectDurationStart, vm.TagProjectDurationStart, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(TagProjectDurationEnd, vm.TagProjectDurationEnd, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(TagBackupPolicy, vm.TagBackupPolicy, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(TagBackupFrequency, vm.TagBackupFrequency, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(TagReservedInstance, vm.TagReservedInstance, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(TagServerType, vm.TagServerType, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(CreatedBy, vm.CreatedBy, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(CreatedDtUtc, vm.CreatedDtUtc);
        }

        [Table("VirtualMachineExtension")]
        public class VirtualMachineExtensionModel : AzureModel
        {
            public string Name { get; set; }
            public int? VirtualMachineId { get; set; }
            public string Publisher { get; set; }
            public string ImageName { get; set; }
            public string ProvisioningState { get; set; }
            public bool AutoUpgradeMinorVersion { get; set; }
            public string Version { get; set; }
            public string PublicSettings { get; set; }
            public string CreatedBy { get; set; }
            public DateTime? CreatedDtUtc { get; set; }

            public bool IsEqual(VirtualMachineExtensionModel extension)
            {
                return string.Equals(AzureId, extension.AzureId, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(Name, extension.Name, StringComparison.OrdinalIgnoreCase) &&
                       VirtualMachineId == extension.VirtualMachineId &&
                       string.Equals(Publisher, extension.Publisher, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(ImageName, extension.ImageName, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(ProvisioningState, extension.ProvisioningState, StringComparison.OrdinalIgnoreCase) &&
                       AutoUpgradeMinorVersion == extension.AutoUpgradeMinorVersion &&
                       string.Equals(Version, extension.Version, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(PublicSettings, extension.PublicSettings, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(CreatedBy, extension.CreatedBy, StringComparison.OrdinalIgnoreCase) &&
                       IsDateEqual(CreatedDtUtc, extension.CreatedDtUtc);
            }
        }
    }
}