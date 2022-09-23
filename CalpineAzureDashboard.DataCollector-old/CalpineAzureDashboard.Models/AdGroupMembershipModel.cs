using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models
{
    [Table("AdGroupMembership")]
    public class AdGroupMembershipModel : AzureModel
    {
        [NotMapped]
        public new string AzureId { get; set; }
        public int? AdGroupId { get; set; }
        public string AdUserObjectId { get; set; }
        public string ChildAdGroupObjectId { get; set; }
        public string ServicePrincipalObjectId { get; set; }

        public bool IsEqual(AdGroupMembershipModel adGroupMembership)
        {
            return AdGroupId == adGroupMembership.AdGroupId &&
                   string.Equals(AdUserObjectId, adGroupMembership.AdUserObjectId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ChildAdGroupObjectId, adGroupMembership.ChildAdGroupObjectId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ServicePrincipalObjectId, adGroupMembership.ServicePrincipalObjectId, StringComparison.OrdinalIgnoreCase);
        }
    }
}