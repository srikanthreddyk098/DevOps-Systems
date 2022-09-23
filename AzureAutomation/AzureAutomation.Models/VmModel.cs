using System.ComponentModel.DataAnnotations.Schema;

namespace AzureAutomation.Models
{
    public class VmModel
    {
        public int Id { get; set; }
        public string SubscriptionId { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Os { get; set; }
        public string Size { get; set; }
        public string Tags { get; set; }
        [NotMapped]
        public string Status { get; set; }
    }
}