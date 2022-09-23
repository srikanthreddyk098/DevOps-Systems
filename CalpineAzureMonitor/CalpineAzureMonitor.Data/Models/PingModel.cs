namespace CalpineAzureMonitor.Data.Models
{
    [Table("PingMapping")]
    public class PingModel
    {
        public string Subscription { get; set; }
        public string Vm { get; set; }
        public string PrivateIp { get; set; }
        public string Email { get; set; }
    }
}
