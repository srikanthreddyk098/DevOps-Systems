namespace CalpineAzureMonitor.Data.Models
{
    [Table("IisAppPoolMapping")]
    public class IisAppPoolModel
    {
        public string Vm { get; set; }
        public string AppPool { get; set; }
        public string Email { get; set; }
    }
}
