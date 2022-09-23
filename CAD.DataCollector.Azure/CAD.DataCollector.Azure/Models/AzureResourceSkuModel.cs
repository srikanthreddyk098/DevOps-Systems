using System.ComponentModel.DataAnnotations.Schema;

namespace CAD.DataCollector.Azure.Models
{
    [Table("AzureResourceSku")]
    public class AzureResourceSkuModel
    {
        public int Id { get; set; }
        public string TenantId { get; set; }
        public string SubscriptionId { get; set; }
        public string ResourceType { get; set; }
        public string Name { get; set; }
        public string AzureId { get; set; }
        public string Location { get; set; }
        public string Data { get; set; }
    }
}
