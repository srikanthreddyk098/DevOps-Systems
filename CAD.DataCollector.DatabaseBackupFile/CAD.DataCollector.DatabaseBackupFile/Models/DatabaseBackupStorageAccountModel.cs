using System.ComponentModel.DataAnnotations.Schema;

namespace CAD.DataCollector.DatabaseBackupFile.Models
{
    [Table("vw_DatabaseBackupStorageAccount")]
    public class DatabaseBackupStorageAccountModel
    {
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string SubscriptionId { get; set; }
        public string Subscription { get; set; }
        public string ResourceGroup { get; set; }
        public string Name { get; set; }
    }
}
