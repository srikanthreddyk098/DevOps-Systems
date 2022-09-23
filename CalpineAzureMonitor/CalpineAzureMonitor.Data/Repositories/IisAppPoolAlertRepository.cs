using CalpineAzureMonitor.Data.Models;

namespace CalpineAzureMonitor.Data.Repositories
{
    public class IisAppPoolAlertRepository : BaseRepository<IisAppPoolAlertModel>
    {
        public IisAppPoolAlertRepository(string conn) : base(conn)
        {
        }
    }
}
