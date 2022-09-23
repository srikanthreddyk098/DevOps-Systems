using System.Collections.Generic;
using System.Threading.Tasks;
using CalpineAzureMonitor.Data.Models;

namespace CalpineAzureMonitor.Data.Repositories
{
    public class IisAppPoolRepository : BaseRepository<IisAppPoolModel>
    {
        public IisAppPoolRepository(string conn) : base(conn)
        {
        }

        public async Task<IEnumerable<int>> GetFrequenciesAsync()
        {
            const string query = "SELECT DISTINCT [Frequency] FROM [vw_IisAppPoolMapping] WHERE [IsDisabled] = 0";

            using (var db = DbConnection) {
                return await db.GetCollectionAsync<int>(query);
            }
        }

        public async Task<IEnumerable<IisAppPoolModel>> GetServicesForFrequencyAsync(int frequency)
        {
            const string query = "SELECT * FROM [vw_IisAppPoolMapping] WHERE [Frequency] = @frequency AND [IsDisabled] = 0";

            using (var db = DbConnection) {
                return await db.GetCollectionAsync<IisAppPoolModel>(query, new {frequency});
            }
        }
    }
}
