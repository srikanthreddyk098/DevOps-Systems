using System;
using CalpineAzureDashboard.Data;

namespace CalpineAzureDashboard.DataCollector.LogAnalytics
{
    [Table("LogAnalyticsQuery")]
    public class LogAnalyticsQueryModel
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public string WorkspaceId  { get; set; }
        public string Query { get; set; }
        public int RunFrequencyInMin { get; set; }
        public bool TruncateEveryRun { get; set; }
        public bool IsDisabled { get; set; }

        public bool IsEqual(LogAnalyticsQueryModel query)
        {
            return Id == query.Id &&
                   string.Equals(TableName, query.TableName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(WorkspaceId, query.WorkspaceId, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Query, query.Query, StringComparison.OrdinalIgnoreCase) &&
                   RunFrequencyInMin == query.RunFrequencyInMin;
        }
    }
}