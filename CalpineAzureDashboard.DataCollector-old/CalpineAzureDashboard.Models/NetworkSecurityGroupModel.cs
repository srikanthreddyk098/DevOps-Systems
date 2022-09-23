using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models
{
    [Table("NetworkSecurityGroup")]
    public class NetworkSecurityGroupModel : AzureModel
    {
        public string SubscriptionId { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string Region { get; set; }
        public string Name { get; set; }
        public string NetworkInterfaceAzureIds { get; set; }
        public string ProvisioningState { get; set; }
        public string ResourceGuid { get; set; }
        public IEnumerable<SecurityRuleModel> SecurityRules { get; set; }
        public string SubnetAzureIds { get; set; }
        public string Tags { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDtUtc { get; set; }

        public bool IsEqual(NetworkSecurityGroupModel networkSecurityGroup)
        {
            return string.Equals(AzureId, networkSecurityGroup.AzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SubscriptionId, networkSecurityGroup.SubscriptionId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Subscription, networkSecurityGroup.Subscription, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ResourceGroup, networkSecurityGroup.ResourceGroup, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Region, networkSecurityGroup.Region, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Name, networkSecurityGroup.Name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(NetworkInterfaceAzureIds, networkSecurityGroup.NetworkInterfaceAzureIds, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ProvisioningState, networkSecurityGroup.ProvisioningState, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ResourceGuid, networkSecurityGroup.ResourceGuid, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SubnetAzureIds, networkSecurityGroup.SubnetAzureIds, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Tags, networkSecurityGroup.Tags, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(CreatedBy, networkSecurityGroup.CreatedBy, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(CreatedDtUtc, networkSecurityGroup.CreatedDtUtc);
        }

        [Table("NsgSecurityRule")]
        public class SecurityRuleModel : AzureModel
        {
            public int? NetworkSecurityGroupId { get; set; }
            public string Name { get; set; }
            public string Access { get; set; }
            public string Description { get; set; }
            public string DestinationAddressPrefix { get; set; }
            public string DestinationAddressPrefixes { get; set; }
            public string DestinationApplicationSecurityGroupAzureIds { get; set; }
            public string DestinationPortRange { get; set; }
            public string DestinationPortRanges { get; set; }
            public string Direction { get; set; }
            public bool IsDefaultSecurityRule { get; set; }
            public int? Priority { get; set; }
            public string Protocol { get; set; }
            public string ProvisioningState { get; set; }
            public string SourceAddressPrefix { get; set; }
            public string SourceAddressPrefixes { get; set; }
            public string SourceApplicationSecurityGroupAzureIds { get; set; }
            public string SourcePortRange { get; set; }
            public string SourcePortRanges { get; set; }

            public bool IsEqual(SecurityRuleModel securityRule)
            {
                return string.Equals(AzureId, securityRule.AzureId, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(Name, securityRule.Name, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(Access, securityRule.Access, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(Description, securityRule.Description, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(DestinationAddressPrefix, securityRule.DestinationAddressPrefix, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(DestinationAddressPrefixes, securityRule.DestinationAddressPrefixes, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(DestinationPortRange, securityRule.DestinationPortRange, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(Direction, securityRule.Direction, StringComparison.OrdinalIgnoreCase) &&
                       IsDefaultSecurityRule == securityRule.IsDefaultSecurityRule &&
                       Priority == securityRule.Priority &&
                       string.Equals(Protocol, securityRule.Protocol, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(ProvisioningState, securityRule.ProvisioningState, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(SourceAddressPrefix, securityRule.SourceAddressPrefix, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(SourceAddressPrefixes, securityRule.SourceAddressPrefixes, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(SourcePortRange, securityRule.SourcePortRange, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(SourcePortRanges, securityRule.SourcePortRanges, StringComparison.OrdinalIgnoreCase);
            }
        }

        public class ApplicationSecurityGroup : AzureModel
        {
            public string Name { get; set; }
            public string Location { get; set; }
            public string ProvisioningState { get; set; }
            public string ResourceGuid { get; set; }
            public string Tags { get; set; }
        }
    }
}
