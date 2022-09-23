using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models
{
    [Table("Subnet")]
    public class SubnetModel : AzureModel
    {
        public string Name { get; set; }
        public int? VirtualNetworkId { get; set; }
        public string AddressPrefix { get; set; }
        public string NetworkSecurityGroupAzureId { get; set; }
        public string ProvisioningState { get; set; }
        public string ResourceNavigationLinks { get; set; }
        public string RouteTableAzureId { get; set; }
        public IEnumerable<IpConfigurationModel> IpConfigurations { get; set; }
        public IEnumerable<SubnetServiceEndpointModel> ServiceEndpoints { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDtUtc { get; set; }

        public bool IsEqual(SubnetModel subnet)
        {
            return string.Equals(AzureId, subnet.AzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Name, subnet.Name, StringComparison.OrdinalIgnoreCase) &&
                   VirtualNetworkId == subnet.VirtualNetworkId &&
                   string.Equals(AddressPrefix, subnet.AddressPrefix, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(NetworkSecurityGroupAzureId, subnet.NetworkSecurityGroupAzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ProvisioningState, subnet.ProvisioningState, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ResourceNavigationLinks, subnet.ResourceNavigationLinks, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(RouteTableAzureId, subnet.RouteTableAzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(CreatedBy, subnet.CreatedBy, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(CreatedDtUtc, subnet.CreatedDtUtc);
        }

        [Table("SubnetIpConfiguration")]
        public class IpConfigurationModel : AzureModel
        {
            public string Name { get; set; }
            public int? SubnetId { get; set; }
            public string PrivateIpAddress { get; set; }
            public string PrivateIpAllocationMethod { get; set; }
            public string ProvisioningState { get; set; }
            public string PublicIpAddressId { get; set; }

            public bool IsEqual(IpConfigurationModel ipConfiguration)
            {
                return string.Equals(AzureId, ipConfiguration.AzureId, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(Name, ipConfiguration.Name, StringComparison.OrdinalIgnoreCase) &&
                       SubnetId == ipConfiguration.SubnetId &&
                       string.Equals(PrivateIpAddress, ipConfiguration.PrivateIpAddress, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(PrivateIpAllocationMethod, ipConfiguration.PrivateIpAllocationMethod, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(ProvisioningState, ipConfiguration.ProvisioningState, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(PublicIpAddressId, ipConfiguration.PublicIpAddressId, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Table("SubnetServiceEndpoint")]
        public class SubnetServiceEndpointModel : AzureModel
        {
            public int? SubnetId { get; set; }
            public string Locations { get; set; }
            public string ProvisioningState { get; set; }
            public string Service { get; set; }

            public bool IsEqual(SubnetServiceEndpointModel serviceEndpoint)
            {
                return SubnetId == serviceEndpoint.SubnetId &&
                       string.Equals(Locations, serviceEndpoint.Locations, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(ProvisioningState, serviceEndpoint.ProvisioningState, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(Service, serviceEndpoint.Service, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}