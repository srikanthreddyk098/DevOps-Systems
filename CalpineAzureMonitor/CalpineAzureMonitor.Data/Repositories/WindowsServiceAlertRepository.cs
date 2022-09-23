using CalpineAzureMonitor.Data.Models;

namespace CalpineAzureMonitor.Data.Repositories
{
    public class WindowsServiceAlertRepository : BaseRepository<WindowsServiceAlertModel>
    {
        public WindowsServiceAlertRepository(string conn) : base(conn)
        {
        }
    }
}
