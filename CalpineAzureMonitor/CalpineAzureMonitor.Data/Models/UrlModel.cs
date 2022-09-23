namespace CalpineAzureMonitor.Data.Models
{
    [Table("UrlMapping")]
    public class UrlModel
    {
        public int Id { get; set; }
        public string Application { get; set; }
        public string Url { get; set; }
        public int FrequencyInMin { get; set; }
        public string ExpectedResponseCode { get; set; }
        public string ExpectedResponseBody { get; set; }
        public string Email { get; set; }
    }
}
