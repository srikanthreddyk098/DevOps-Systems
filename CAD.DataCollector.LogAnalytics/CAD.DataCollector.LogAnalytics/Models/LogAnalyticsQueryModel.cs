using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAD.DataCollector.LogAnalytics.Models
{
    [Table("vw_LogAnalyticsQuery")]
    public class LogAnalyticsQueryModel
    {
        public string TenantId { get; set; }
        public string Domain { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TableName { get; set; }
        public string WorkspaceId { get; set; }
        public string Query { get; set; }
        public int RunFrequencyInMin { get; set; }
        public bool TruncateEveryRun { get; set; }

        public bool IsEqual(LogAnalyticsQueryModel query)
        {
            return string.Equals(TenantId, query.TenantId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Domain, query.Domain, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ClientId, query.ClientId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ClientSecret, query.ClientSecret, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(TableName, query.TableName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(WorkspaceId, query.WorkspaceId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Query, query.Query, StringComparison.OrdinalIgnoreCase) &&
                   RunFrequencyInMin == query.RunFrequencyInMin &&
                   TruncateEveryRun == query.TruncateEveryRun;
        }
    }
}