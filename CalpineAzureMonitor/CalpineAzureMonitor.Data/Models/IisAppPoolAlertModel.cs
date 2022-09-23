namespace CalpineAzureMonitor.Data.Models
{
    [Table("IisAppPoolAlert")]
    public class IisAppPoolAlertModel
    {
        public int Id { get; set; }
        public string Vm { get; set; }
        public string AppPool { get; set; }
        public string Status { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
