using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models
{
    [Table("ADApplication")]
    public class AdApplicationModel : AzureModel
    {
        public string Name { get; set; }
        public string AdditionalProperties { get; set; }
        public string ApplicationId { get; set; }
        public string ApplicationPermissions { get; set; }
        public bool? AvailableToOtherTenants { get; set; }
        public DateTime? DeletionTimestamp { get; set; }
        public string Homepage { get; set; }
        public string IdentifierUris { get; set; }
        public bool? Oauth2AllowImplicitFlow { get; set; }
        public string ObjectId { get; set; }
        public string ReplyUrls { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDtUtc { get; set; }

        public bool IsEqual(AdApplicationModel adApplication)
        {
            return string.Equals(AzureId, adApplication.AzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Name, adApplication.Name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(AdditionalProperties, adApplication.AdditionalProperties) &&
                   string.Equals(ApplicationId, adApplication.ApplicationId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ApplicationPermissions, adApplication.ApplicationPermissions) &&
                   AvailableToOtherTenants == adApplication.AvailableToOtherTenants &&
                   IsDateEqual(DeletionTimestamp, adApplication.DeletionTimestamp) &&
                   string.Equals(Homepage, adApplication.Homepage, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(IdentifierUris, adApplication.IdentifierUris, StringComparison.OrdinalIgnoreCase) &&
                   Oauth2AllowImplicitFlow == adApplication.Oauth2AllowImplicitFlow &&
                   string.Equals(ObjectId, adApplication.ObjectId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ReplyUrls, adApplication.ReplyUrls, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(CreatedBy, adApplication.CreatedBy, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(CreatedDtUtc, adApplication.CreatedDtUtc);
        }
    }
}