using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models
{
    [Table("AdGroup")]
    public class AdGroupModel : AzureModel
    {
        public string AdditionalProperties { get; set; }
        public DateTime? DeletionTimestamp { get; set; }
        public string DisplayName { get; set; }
        public string Mail { get; set; }
        public string ObjectId { get; set; }
        public bool? SecurityEnabled { get; set; }
        public IEnumerable<string> UserObjectIds { get; set; }
        public IEnumerable<string> GroupObjectIds { get; set; }
        public IEnumerable<string> ServicePrincipalObjectIds { get; set; }

        public bool IsEqual(AdGroupModel adGroup)
        {
            return string.Equals(AzureId, adGroup.AzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(AdditionalProperties, adGroup.AdditionalProperties, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(DeletionTimestamp, adGroup.DeletionTimestamp) &&
                   string.Equals(DisplayName, adGroup.DisplayName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Mail, adGroup.Mail, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ObjectId, adGroup.ObjectId, StringComparison.OrdinalIgnoreCase) &&
                   SecurityEnabled == adGroup.SecurityEnabled;
        }
    }
}