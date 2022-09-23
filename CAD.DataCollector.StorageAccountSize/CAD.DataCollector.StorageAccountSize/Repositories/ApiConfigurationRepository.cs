using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CAD.Database;
using CAD.DataCollector.StorageAccountSize.Models;
using Dapper;

namespace CAD.DataCollector.StorageAccountSize.Repositories
{
    public class ApiConfigurationRepository : BaseRepository<ApiConfigurationModel>
    {
        public ApiConfigurationRepository(string conn) : base(conn)
        {
        }

        public async Task<IEnumerable<ApiConfigurationModel>> GetAllDetailAsync(object whereObject = null, int? commandTimeout = null)
        {
            using var db = DbConnection;
            return await db.Conn.QueryAsync<ApiConfigurationModel>("sp_GetApiConfigurationDetails", whereObject, commandTimeout: 600,
                commandType: CommandType.StoredProcedure);
        }
    }
}