using System.Threading.Tasks;
using CAD.Database;
using CAD.DataCollector.LogAnalytics.Models;

namespace CAD.DataCollector.LogAnalytics.Repositories
{
    public class LogAnalyticsQueryRepository : BaseRepository<LogAnalyticsQueryModel>
    {
        public LogAnalyticsQueryRepository(string conn) : base(conn)
        {
        }
    }
}