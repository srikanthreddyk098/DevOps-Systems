using System.ComponentModel.DataAnnotations.Schema;

namespace AzureAutomation.Models
{
    [Table("PingMapping")]
    public class PingMappingModel
    {
        public int Id { get; set; }
        public string ResourceId { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string VmName { get; set; }
        public string Region { get; set; }
        public string Hostname{ get; set; }
        public string PrivateIp { get; set; }
        public string Frequency { get; set; }
        public string Email { get; set; }
        public string IsDisabled { get; set; }
        public string DisabledUntilDt { get; set; }
        public string Comment { get; set; }
    }
}