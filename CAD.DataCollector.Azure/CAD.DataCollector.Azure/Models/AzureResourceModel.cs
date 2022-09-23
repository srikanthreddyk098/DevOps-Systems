using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAD.DataCollector.Azure.Models
{
    [Table("AzureResource")]
    public class AzureResourceModel
    {
        public int Id { get; set; }
        public string ResourceType { get; set; }
        public string TenantId { get; set; }
        public string SubscriptionId { get; set; }
        public string ResourceGroup { get; set; }
        public string Name { get; set; }
        public string AzureId { get; set; }
        public string ParentAzureId { get; set; }
        public string Data { get; set; }

        public bool IsEquals(AzureResourceModel resource)
        {
            return string.Equals(ResourceType, resource.ResourceType, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(TenantId, resource.TenantId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(SubscriptionId, resource.SubscriptionId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ResourceGroup, resource.ResourceGroup, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(AzureId, resource.AzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ParentAzureId, resource.ParentAzureId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Name, resource.Name, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Data, resource.Data, StringComparison.OrdinalIgnoreCase);
        }
    }
}
