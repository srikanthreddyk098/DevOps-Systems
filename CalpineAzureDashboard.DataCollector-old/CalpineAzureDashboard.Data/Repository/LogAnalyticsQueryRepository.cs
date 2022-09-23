using System.Collections.Generic;
using System.Threading.Tasks;

namespace CalpineAzureDashboard.Data.Repository
{
    public class LogAnalyticsQueryRepository<T> : BaseRepository<T>
    {
        public LogAnalyticsQueryRepository(string conn) : base(conn)
        {
        }
    }
}
