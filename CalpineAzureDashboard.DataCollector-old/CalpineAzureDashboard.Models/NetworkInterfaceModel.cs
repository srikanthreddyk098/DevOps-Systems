using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models
{
    [Table("NetworkInterface")]
    public class NetworkInterfaceModel : AzureModel
    {
        public string SubscriptionId { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string Region { get; set; }
        public string Name { get; set; }
        public int? NumberOfDnsServers { get; set; }
        public int? NumberOfAppliedDnsServers { get; set; }
        public string InternalDnsNameLabel {get; set; }
        public string InternalDomainNameSuffix { get; set; }
        public string InternalFqdn {get; set; }
        public bool? IsAcceleratedNetworkingEnabled { get; set; }
        public bool? IsIpForwardingEnabled { get; set; }
        public string MacAddress { get; set; }
        public string NetworkSecurityGroupId { get; set; }
        public string PrimaryPrivateIp { get; set; }
        public string PrimaryPrivateIpAllocationMethod { get; set; }
        public string VirtualMachineAzureId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDtUtc { get; set; }
        public IEnumerable<IpConfigurationModel> IpConfigurations { get; set; }

        public bool IsEqual(NetworkInterfaceModel networkInterface)
        {
            return string.Equals(AzureId, networkInterface.AzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SubscriptionId, networkInterface.SubscriptionId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Subscription, networkInterface.Subscription, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ResourceGroup, networkInterface.ResourceGroup, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Region, networkInterface.Region, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Name, networkInterface.Name, StringComparison.OrdinalIgnoreCase) &&
                   NumberOfDnsServers == networkInterface.NumberOfDnsServers &&
                   NumberOfAppliedDnsServers == networkInterface.NumberOfAppliedDnsServers &&
                   string.Equals(InternalDnsNameLabel, networkInterface.InternalDnsNameLabel, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(InternalDomainNameSuffix, networkInterface.InternalDomainNameSuffix, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(InternalFqdn, networkInterface.InternalFqdn, StringComparison.OrdinalIgnoreCase) &&
                   IsAcceleratedNetworkingEnabled == networkInterface.IsAcceleratedNetworkingEnabled &&
                   IsIpForwardingEnabled == networkInterface.IsIpForwardingEnabled &&
                   string.Equals(MacAddress, networkInterface.MacAddress, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(NetworkSecurityGroupId, networkInterface.NetworkSecurityGroupId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(PrimaryPrivateIp, networkInterface.PrimaryPrivateIp, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(PrimaryPrivateIpAllocationMethod, networkInterface.PrimaryPrivateIpAllocationMethod, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(VirtualMachineAzureId, networkInterface.VirtualMachineAzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(CreatedBy, networkInterface.CreatedBy, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(CreatedDtUtc, networkInterface.CreatedDtUtc);
        }

        [Table("NetworkInterfaceIpConfiguration")]
        public class IpConfigurationModel : AzureModel
        {
            public string Name { get; set; }
            public int? NetworkInterfaceId { get; set; }
            public bool? IsPrimary { get; set; }
            public string VirtualNetworkId { get; set; }
            public string SubnetId { get; set; }
            public string PrivateIpAddress { get; set; }
            public string PrivateIpAddressVersion { get; set; }
            public string PrivateIpAllocationMethod { get; set; }
            public string PublicIpAddressId { get; set; }

            public bool IsEqual(IpConfigurationModel ipConfiguration)
            {
                return string.Equals(AzureId, ipConfiguration.AzureId, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(Name, ipConfiguration.Name, StringComparison.OrdinalIgnoreCase) &&
                       IsPrimary == ipConfiguration.IsPrimary &&
                       string.Equals(VirtualNetworkId, ipConfiguration.VirtualNetworkId, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(SubnetId, ipConfiguration.SubnetId, StringComparison.OrdinalIgnoreCase) &&
                       NetworkInterfaceId == ipConfiguration.NetworkInterfaceId &&
                       string.Equals(PrivateIpAddress, ipConfiguration.PrivateIpAddress, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(PrivateIpAddressVersion, ipConfiguration.PrivateIpAddressVersion, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(PrivateIpAllocationMethod, ipConfiguration.PrivateIpAllocationMethod, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(PublicIpAddressId, ipConfiguration.PublicIpAddressId, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}