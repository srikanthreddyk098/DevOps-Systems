using System.ComponentModel.DataAnnotations.Schema;

namespace CAD.DataCollector.StorageAccountSize.Models
{
    [Table("vw_ApiConfiguration")]
    public class ApiConfigurationModel
    {
        public int Id { get; set; }
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ResourceType { get; set; }
        public string AzureIdCustomPrefix { get; set; }
        public string AzureIdProperty { get; set; }
        public string NameProperty { get; set; }
        public int ProcessingOrder { get; set; }
        public string Frequency { get; set; }
        public string HttpMethod { get; set; }
        public string ResourceUrl { get; set; }
        public string Url { get; set; }
        public bool InsertOnly { get; set; }
        public string CustomTableName { get; set; }
        public string PostProcessingQuery { get; set; }
    }
}