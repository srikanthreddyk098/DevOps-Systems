namespace CalpineAzureMonitor.Data.Models
{
    [Table("PingAlert")]
    public class PingAlertModel
    {
        public int Id { get; set; }
        public string Subscription { get; set; }
        public string Vm { get; set; }
        public string PrivateIp { get; set; }
        public string Status { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
