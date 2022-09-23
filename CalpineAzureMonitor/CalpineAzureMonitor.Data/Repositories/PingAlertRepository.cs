using CalpineAzureMonitor.Data.Models;

namespace CalpineAzureMonitor.Data.Repositories
{
    public class PingAlertRepository : BaseRepository<PingAlertModel>
    {
        public PingAlertRepository(string conn) : base(conn)
        {
        }
    }
}
