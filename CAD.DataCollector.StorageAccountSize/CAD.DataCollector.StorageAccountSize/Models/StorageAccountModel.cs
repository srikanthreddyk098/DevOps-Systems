namespace CAD.DataCollector.StorageAccountSize.Models
{
    public class StorageAccountModel
    {
        public string TenantId { get; set; }
        public string SubscriptionId { get; set; }
        public string Name { get; set; }
        public string AzureId { get; set; }
        public string PrimaryFileEndpoint { get; set; }
    }
}