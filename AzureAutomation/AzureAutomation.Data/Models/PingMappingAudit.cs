using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureAutomation.Data.Models
{
    [Table("PingMappingAudit")]
    public class PingMappingAudit
    {
        public int Id { get; set; }
        public string ResourceId { get; set; }
        public string Type { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string ChangedBy { get; set; }
        public DateTime ChangedDt { get; set; }
    }
}