using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models
{
    [Table("PublicIp")]
    public class PublicIpModel : AzureModel
    {
        public string SubscriptionId { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string Region { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string IpAllocationMethod { get; set; }
        public string Version { get; set; }
        public string Fqdn { get; set; }
        public bool? HasAssignedLoadBalancer { get; set; }
        public bool? HasAssignedNetworkInterface { get; set; }
        public string AvailabilityZones { get; set; }
        public int? IdleTimeoutInMinutes { get; set; }
        public string LeafDomainLabel { get; set; }
        public string ReverseFqdn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDtUtc { get; set; }

        public bool IsEqual(PublicIpModel publicIp)
        {
            return string.Equals(AzureId, publicIp.AzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SubscriptionId, publicIp.SubscriptionId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Subscription, publicIp.Subscription, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ResourceGroup, publicIp.ResourceGroup, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Region, publicIp.Region, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Name, publicIp.Name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(IpAddress, publicIp.IpAddress, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(IpAllocationMethod, publicIp.IpAllocationMethod, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Version, publicIp.Version, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Fqdn, publicIp.Fqdn, StringComparison.OrdinalIgnoreCase) &&
                   HasAssignedLoadBalancer == publicIp.HasAssignedLoadBalancer &&
                   HasAssignedNetworkInterface == publicIp.HasAssignedNetworkInterface &&
                   string.Equals(AvailabilityZones, publicIp.AvailabilityZones, StringComparison.OrdinalIgnoreCase) &&
                   IdleTimeoutInMinutes == publicIp.IdleTimeoutInMinutes &&
                   string.Equals(LeafDomainLabel, publicIp.LeafDomainLabel, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ReverseFqdn, publicIp.ReverseFqdn, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(CreatedBy, publicIp.CreatedBy, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(CreatedDtUtc, publicIp.CreatedDtUtc);
        }
    }
}