namespace CalpineAzureMonitor.Data.Models
{
    [Table("UrlAlert")]
    public class UrlAlertModel
    {
        public string Url { get; set; }
        public string ResponseCode { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
