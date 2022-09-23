using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models
{
    [Table("VirtualNetwork")]
    public class VirtualNetworkModel : AzureModel
    {
        public string SubscriptionId { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string Region { get; set; }
        public string Name { get; set; }
        public string AddressSpace { get; set; }
        public string DdosProtectionPlan { get; set; }
        public string DnsServers { get; set; }
        public bool EnableDdosProtection { get; set; }
        public bool EnableVmProtection { get; set; }
        public IEnumerable<SubnetModel> Subnets { get; set; }
        public string Tags { get; set; }
        public IEnumerable<VirtualNetworkPeeringModel> Peerings { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDtUtc { get; set; }

        public bool IsEqual(VirtualNetworkModel virtualNetwork)
        {
            return string.Equals(AzureId, virtualNetwork.AzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SubscriptionId, virtualNetwork.SubscriptionId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Subscription, virtualNetwork.Subscription, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ResourceGroup, virtualNetwork.ResourceGroup, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Region, virtualNetwork.Region, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Name, virtualNetwork.Name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(AddressSpace, virtualNetwork.AddressSpace, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(DdosProtectionPlan, virtualNetwork.DdosProtectionPlan, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(DnsServers, virtualNetwork.DnsServers, StringComparison.OrdinalIgnoreCase) &&
                   EnableDdosProtection == virtualNetwork.EnableDdosProtection &&
                   EnableVmProtection == virtualNetwork.EnableVmProtection &&
                   string.Equals(Tags, virtualNetwork.Tags, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(CreatedBy, virtualNetwork.CreatedBy, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(CreatedDtUtc, virtualNetwork.CreatedDtUtc);
        }

        [Table("VirtualNetworkPeering")]
        public class VirtualNetworkPeeringModel : AzureModel
        {
            public string Name { get; set; }
            public int? VirtualNetworkId { get; set; }
            public bool? AllowForwardedTraffic { get; set; }
            public bool? AllowGatewayTransit { get; set; }
            public bool? AllowVirtualNetworkAccess { get; set; }
            public string PeeringState { get; set; }
            public string ProvisioningState { get; set; }
            public string RemoteAddressSpace { get; set; }
            public string RemoteVirtualNetwork { get; set; }
            public bool? UseRemoteGateways { get; set; }
            public string CreatedBy { get; set; }
            public DateTime? CreatedDtUtc { get; set; }

            public bool IsEqual(VirtualNetworkPeeringModel peering)
            {
                return string.Equals(AzureId, peering.AzureId, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(Name, peering.Name, StringComparison.OrdinalIgnoreCase) &&
                       VirtualNetworkId == peering.VirtualNetworkId &&
                       AllowForwardedTraffic == peering.AllowForwardedTraffic &&
                       AllowGatewayTransit == peering.AllowGatewayTransit &&
                       AllowVirtualNetworkAccess == peering.AllowVirtualNetworkAccess &&
                       string.Equals(PeeringState, peering.PeeringState, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(ProvisioningState, peering.ProvisioningState, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(RemoteAddressSpace, peering.RemoteAddressSpace, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(RemoteVirtualNetwork, peering.RemoteVirtualNetwork, StringComparison.OrdinalIgnoreCase) &&
                       UseRemoteGateways == peering.UseRemoteGateways &&
                       string.Equals(CreatedBy, peering.CreatedBy) &&
                       IsDateEqual(CreatedDtUtc, peering.CreatedDtUtc);
            }
        }
    }
}