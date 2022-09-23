using System.Collections.Generic;
using System.Threading.Tasks;
using CalpineAzureMonitor.Data.Models;

namespace CalpineAzureMonitor.Data.Repositories
{
    public class WindowsServiceRepository : BaseRepository<WindowsServiceModel>
    {
        public WindowsServiceRepository(string conn) : base(conn)
        {
        }

        public async Task<IEnumerable<int>> GetFrequenciesAsync()
        {
            const string query = "SELECT DISTINCT [Frequency] FROM [vw_WindowsServiceMapping] WHERE [IsDisabled] = 0";

            using (var db = DbConnection) {
                return await db.GetCollectionAsync<int>(query);
            }
        }

        public async Task<IEnumerable<WindowsServiceModel>> GetServicesForFrequencyAsync(int frequency)
        {
            const string query = "SELECT * FROM [vw_WindowsServiceMapping] WHERE [Frequency] = @frequency AND [IsDisabled] = 0";

            using (var db = DbConnection) {
                return await db.GetCollectionAsync<WindowsServiceModel>(query, new {frequency});
            }
        }

        public async Task UpdateWindowsServiceMapping()
        {
            using (var db = DbConnection) {
                await db.ExecuteStoredProcedureAsync("sp_UpdateWindowsServiceMapping");
            }
        }
    }
}
