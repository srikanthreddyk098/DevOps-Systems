using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models
{
    [Table("AdRoleDefinition")]
    public class AdRoleDefinitionModel : AzureModel
    {
        public string Name { get; set; }
        public string FriendlyName { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Scopes { get; set; }
        public string Actions { get; set; }
        public string NotActions { get; set; }
        public string DataActions { get; set; }
        public string NotDataActions { get; set; }

        public bool IsEqual(AdRoleDefinitionModel roleDefinition)
        {
            return string.Equals(AzureId, roleDefinition.AzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Name, roleDefinition.Name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(FriendlyName, roleDefinition.FriendlyName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Description, roleDefinition.Description, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Type, roleDefinition.Type, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Scopes, roleDefinition.Scopes, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(NotActions, roleDefinition.NotActions, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(DataActions, roleDefinition.DataActions, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(NotDataActions, roleDefinition.NotDataActions, StringComparison.OrdinalIgnoreCase);
        }
    }
}