using System.ComponentModel.DataAnnotations.Schema;

namespace AzureAutomation.Models
{
    [Table("PermissionMapping")]
    public class PermissionMappingModel
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string VmName{ get; set; }
    }
}