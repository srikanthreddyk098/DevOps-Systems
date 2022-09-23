using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models
{
    [Table("AdRoleAssignment")]
    public class AdRoleAssignmentModel : AzureModel
    {
        public string Name { get; set; }
        public string PrincipalId { get; set; }
        public string RoleDefinitionId { get; set; }
        public string Scope { get; set; }
        public bool? CanDelegate { get; set; }

        public bool IsEqual(AdRoleAssignmentModel roleAssignment)
        {
            return string.Equals(AzureId, roleAssignment.AzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Name, roleAssignment.Name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(PrincipalId, roleAssignment.PrincipalId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(RoleDefinitionId, roleAssignment.RoleDefinitionId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Scope, roleAssignment.Scope, StringComparison.OrdinalIgnoreCase) &&
                   CanDelegate == roleAssignment.CanDelegate;
        }
    }
}