using System.ComponentModel.DataAnnotations.Schema;

namespace AzureAutomation.Models
{
    [Table("WindowsServiceMapping")]
    public class WindowsServiceMappingModel
    {
        public int? Id { get; set; }
        public string Vm { get; set; }
        public string Service { get; set; }
        public string Frequency { get; set; }
        public string Email { get; set; }
        public string IsDisabled { get; set; }
        public string DisabledUntilDt { get; set; }
        public string Comment { get; set; }
    }
}