using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models
{
    [Table("ResourceGroup")]
    public class ResourceGroupModel : AzureModel
    {
        public string SubscriptionId { get; set; }
        public string Subscription { get; set; }
        public string Region { get; set; }
        public string Name { get; set; }
        public string ProvisioningState { get; set; }
        public string ManagedBy { get; set; }
        public string Tags { get; set; }
        public string BusinessUnit { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDtUtc { get; set; }

        public bool IsEqual(ResourceGroupModel resourceGroup)
        {
            return string.Equals(AzureId, resourceGroup.AzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SubscriptionId, resourceGroup.SubscriptionId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Subscription, resourceGroup.Subscription, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Region, resourceGroup.Region, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Name, resourceGroup.Name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ProvisioningState, resourceGroup.ProvisioningState, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ManagedBy, resourceGroup.ManagedBy, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Tags, resourceGroup.Tags, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(CreatedBy, resourceGroup.CreatedBy, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(CreatedDtUtc, resourceGroup.CreatedDtUtc);
        }
    }
}