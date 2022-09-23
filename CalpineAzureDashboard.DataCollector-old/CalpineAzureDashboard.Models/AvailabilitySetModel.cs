using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models
{
    [Table("AvailabilitySet")]
    public class AvailabilitySetModel : AzureModel
    {
        public string SubscriptionId { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string Region { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public int? FaultDomainCount { get; set; }
        public int? UpdateDomainCount { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDtUtc { get; set; }

        public bool IsEqual(AvailabilitySetModel availabilitySet)
        {
            return string.Equals(AzureId, availabilitySet.AzureId, StringComparison.OrdinalIgnoreCase) && 
                   string.Equals(SubscriptionId, availabilitySet.SubscriptionId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Subscription, availabilitySet.Subscription, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ResourceGroup, availabilitySet.ResourceGroup, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Region, availabilitySet.Region, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Name, availabilitySet.Name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Sku, availabilitySet.Sku, StringComparison.OrdinalIgnoreCase) &&
                   FaultDomainCount == availabilitySet.FaultDomainCount &&
                   UpdateDomainCount == availabilitySet.UpdateDomainCount &&
                   string.Equals(CreatedBy, availabilitySet.CreatedBy, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(CreatedDtUtc, availabilitySet.CreatedDtUtc);
        }
    }
}