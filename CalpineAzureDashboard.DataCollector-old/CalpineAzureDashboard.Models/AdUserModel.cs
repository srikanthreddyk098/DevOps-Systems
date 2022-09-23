using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models
{
    [Table("AdUser")]
    public class AdUserModel : AzureModel
    {
        public bool? AccountEnabled { get; set; }
        public string AdditionalProperties { get; set; }
        public DateTime? DeletionTimestamp { get; set; }
        public string DisplayName { get; set; }
        public string GivenName { get; set; }
        public string Mail { get; set; }
        public string MailNickname { get; set; }
        public string ObjectId { get; set; }
        public string SignInNames { get; set; }
        public string Surname { get; set; }
        public string UsageLocation { get; set; }
        public string UserPrincipalName { get; set; }
        public string UserType { get; set; }

        public bool IsEqual(AdUserModel adUser)
        {
            return string.Equals(AzureId, adUser.AzureId, StringComparison.OrdinalIgnoreCase) &&
                   AccountEnabled == adUser.AccountEnabled &&
                   string.Equals(AdditionalProperties, adUser.AdditionalProperties, StringComparison.OrdinalIgnoreCase) &&
                   IsDateEqual(DeletionTimestamp, DeletionTimestamp) &&
                   string.Equals(DisplayName, adUser.DisplayName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(GivenName, adUser.GivenName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Mail, adUser.Mail, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(MailNickname, adUser.MailNickname, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ObjectId, adUser.ObjectId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SignInNames, adUser.SignInNames, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Surname, adUser.Surname, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(UsageLocation, adUser.UsageLocation, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(UserPrincipalName, adUser.UserPrincipalName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(UserType, adUser.UserType, StringComparison.OrdinalIgnoreCase);
        }
    }
}