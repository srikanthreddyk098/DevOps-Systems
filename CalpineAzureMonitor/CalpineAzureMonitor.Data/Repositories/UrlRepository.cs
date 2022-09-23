using System.Collections.Generic;
using System.Threading.Tasks;
using CalpineAzureMonitor.Data.Models;

namespace CalpineAzureMonitor.Data.Repositories
{
    public class UrlRepository : BaseRepository<UrlModel>
    {
        public UrlRepository(string conn) : base(conn)
        {
        }

        public async Task<IEnumerable<int>> GetFrequenciesAsync()
        {
            const string query = "SELECT DISTINCT [FrequencyInMin] FROM [vw_UrlMapping] WHERE [IsDisabled] = 0";

            using (var db = DbConnection) {
                return await db.GetCollectionAsync<int>(query);
            }
        }

        public async Task<IEnumerable<UrlModel>> GetUrlsForFrequencyAsync(int frequencyInMin)
        {
            const string query = "SELECT * FROM [vw_UrlMapping] WHERE [FrequencyInMin] = @frequencyInMin AND [IsDisabled] = 0";

            using (var db = DbConnection) {
                return await db.GetCollectionAsync<UrlModel>(query, new {frequencyInMin});
            }
        }

        public async Task UpdateUrlMapping()
        {
            using (var db = DbConnection) {
                await db.ExecuteStoredProcedureAsync("sp_UpdateUrlMapping");
            }
        }
    }
}
