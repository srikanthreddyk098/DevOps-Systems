using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models
{
    [Table("LoadBalancer")]
    public class LoadBalancerModel : AzureModel
    {
        public string SubscriptionId { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string Region { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDtUtc { get; set; }
        public IEnumerable<FrontendModel> Frontends { get; set; }
        public IEnumerable<BackendModel> Backends { get; set; }

        public bool IsEqual(LoadBalancerModel loadBalancer)
        {
            return string.Equals(AzureId, loadBalancer.AzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SubscriptionId, loadBalancer.SubscriptionId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Subscription, loadBalancer.Subscription, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ResourceGroup, loadBalancer.ResourceGroup, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Region, loadBalancer.Region, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Name, loadBalancer.Name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(CreatedBy, loadBalancer.CreatedBy, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(CreatedDtUtc, loadBalancer.CreatedDtUtc);
        }


        [Table("LoadBalancerFrontend")]
        public class FrontendModel : AzureModel
        {
            public string Name { get; set; }
            public int? LoadBalancerId { get; set; }
            public string PrivateIpAddress { get; set; }
            public string PrivateIpAllocationMethod { get; set; }
            public string PublicIpAddressId { get; set; }
            public string SubnetId { get; set; }

            public bool IsEqual(FrontendModel frontend)
            {
                return string.Equals(AzureId, frontend.AzureId, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(Name, frontend.Name, StringComparison.OrdinalIgnoreCase) &&
                       LoadBalancerId == frontend.LoadBalancerId &&
                       string.Equals(PrivateIpAddress, frontend.PrivateIpAddress, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(PrivateIpAllocationMethod, frontend.PrivateIpAllocationMethod, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(PublicIpAddressId, frontend.PublicIpAddressId, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(SubnetId, frontend.SubnetId, StringComparison.OrdinalIgnoreCase);
            }
        }


        [Table("LoadBalancerBackend")]
        public class BackendModel : AzureModel
        {
            public string Name { get; set; }
            public int? LoadBalancerId { get; set; }
            public string IpConfigurationName { get; set; }
            public string NetworkInterfaceName { get; set; }

            public bool IsEqual(BackendModel backend)
            {
                return string.Equals(AzureId, backend.AzureId, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(Name, backend.Name, StringComparison.OrdinalIgnoreCase) &&
                       LoadBalancerId == backend.LoadBalancerId &&
                       string.Equals(IpConfigurationName, backend.IpConfigurationName, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(NetworkInterfaceName, backend.NetworkInterfaceName, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}