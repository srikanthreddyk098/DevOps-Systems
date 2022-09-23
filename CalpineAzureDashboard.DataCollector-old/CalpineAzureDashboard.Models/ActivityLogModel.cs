using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CalpineAzureDashboard.Models
{
    [Table("ActivityLog")]
    public class ActivityLogModel : AzureModel
    {
        public string SubscriptionId { get; set; }
        public string Subscription { get; set; }
        public string Action { get; set; }
        public string Caller { get; set; }
        public string Category { get; set; }
        public string ClientIpAddress { get; set; }
        public string ClientRequestId { get; set; }
        public string CorrelationId { get; set; }
        public string Description { get; set; }
        public string Event { get; set; }
        public string EventDataId { get; set; }
        public DateTime? EventTimestampUtc { get; set; }
        public string HttpMethod { get; set; }
        public string Level { get; set; }
        public string Operation { get; set; }
        public string OperationId { get; set; }
        public string Properties { get; set; }
        public string ResourceGroup { get; set; }
        public string ResourceId { get; set; }
        public string ResourceProvider { get; set; }
        public string ResourceType { get; set; }
        public string Role { get; set; }
        public string Scope { get; set; }
        public string Status { get; set; }
        public string SubStatus { get; set; }
        public DateTime? SubmissionTimeStampUtc { get; set; }
        public string Uri { get; set; }
    }
}