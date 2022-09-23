using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models
{
    [Table("DataDisk")]
    public class DataDiskModel : AzureModel
    {
        public string SubscriptionId { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string Region { get; set; }
        public string Name { get; set; }
        public string DiskState { get; set; }
        public int? SizeInGb { get; set; }
        public long? DiskIOPSReadWrite { get; set; }
        public int? DiskMBpsReadWrite { get; set; }
        public string Os { get; set; }
        public string CreationMethod { get; set; }
        public string ImageId { get; set; }
        public string Sku { get; set; }
        public bool? IsAttachedToVm { get; set; }
        public string VirtualMachineAzureId { get; set; }
        public bool? IsEncryptionEnabled { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDtUtc { get; set; }

        public bool IsEqual(DataDiskModel dataDisk)
        {
            return string.Equals(AzureId, dataDisk.AzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SubscriptionId, dataDisk.SubscriptionId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Subscription, dataDisk.Subscription, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ResourceGroup, dataDisk.ResourceGroup, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Region, dataDisk.Region, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Name, dataDisk.Name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(DiskState, dataDisk.DiskState, StringComparison.OrdinalIgnoreCase) &&
                   SizeInGb == dataDisk.SizeInGb &&
                   DiskIOPSReadWrite == dataDisk.DiskIOPSReadWrite &&
                   DiskMBpsReadWrite == dataDisk.DiskMBpsReadWrite &&
                   string.Equals(CreationMethod, dataDisk.CreationMethod, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ImageId, dataDisk.ImageId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Sku, dataDisk.Sku, StringComparison.OrdinalIgnoreCase) &&
                   IsAttachedToVm == dataDisk.IsAttachedToVm &&
                   string.Equals(VirtualMachineAzureId, dataDisk.VirtualMachineAzureId, StringComparison.OrdinalIgnoreCase) &&
                   IsEncryptionEnabled == dataDisk.IsEncryptionEnabled &&
                   string.Equals(CreatedBy, dataDisk.CreatedBy, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(CreatedDtUtc, dataDisk.CreatedDtUtc);
        }
    }
}