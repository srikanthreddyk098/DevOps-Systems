using CalpineAzureMonitor.Data.Models;

namespace CalpineAzureMonitor.Data.Repositories
{
    public class UrlAlertRepository : BaseRepository<UrlAlertModel>
    {
        public UrlAlertRepository(string conn) : base(conn)
        {
        }
    }
}
