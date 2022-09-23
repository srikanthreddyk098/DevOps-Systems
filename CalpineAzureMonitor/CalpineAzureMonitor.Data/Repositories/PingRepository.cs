using System.Collections.Generic;
using System.Threading.Tasks;
using CalpineAzureMonitor.Data.Models;

namespace CalpineAzureMonitor.Data.Repositories
{
    public class PingRepository : BaseRepository<PingModel>
    {
        public PingRepository(string conn) : base(conn)
        {
        }

        public async Task<IEnumerable<int>> GetFrequenciesAsync()
        {
            const string query = "SELECT DISTINCT [Frequency] FROM [vw_PingMapping] WHERE [IsDisabled] = 0";

            using (var db = DbConnection) {
                return await db.GetCollectionAsync<int>(query);
            }
        }

        public async Task<IEnumerable<PingModel>> GetMachinesForFrequencyAsync(int frequency)
        {
            const string query = "SELECT * FROM [vw_PingMapping] WHERE [Frequency] = @frequency AND [IsDisabled] = 0";

            using (var db = DbConnection) {
                return await db.GetCollectionAsync<PingModel>(query, new {frequency});
            }
        }

        public async Task UpdatePingMapping()
        {
            using (var db = DbConnection) {
                await db.ExecuteStoredProcedureAsync("sp_UpdatePingMapping");
            }
        }
    }
}
