namespace CalpineAzureMonitor.Data.Models
{
    [Table("WindowsServiceMapping")]
    public class WindowsServiceModel
    {
        public string Vm { get; set; }
        public string Service { get; set; }
        public string Email { get; set; }
    }
}
