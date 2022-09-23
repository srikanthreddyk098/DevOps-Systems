using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureAutomation.Data.Models
{
    [Table("WindowsServiceMappingAudit")]
    public class WindowsServiceMappingAudit
    {
        public int Id { get; set; }
        public string Vm { get; set; }
        public string Service { get; set; }
        public string Action { get; set; }
        public string Type { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string ChangedBy { get; set; }
        public DateTime ChangedDt { get; set; }
    }
}