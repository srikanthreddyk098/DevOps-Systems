using System.ComponentModel.DataAnnotations.Schema;

namespace AzureAutomation.Models
{
    [Table("UserVmMapping")]
    public class SubscriptionModel
    {
        public string SubscriptionId { get; set; }
        public string Subscription { get; set; }
    }
}