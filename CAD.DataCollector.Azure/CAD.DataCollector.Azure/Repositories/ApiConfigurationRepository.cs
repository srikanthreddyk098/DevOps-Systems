using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CAD.Database;
using CAD.DataCollector.Azure.Models;
using Dapper;

namespace CAD.DataCollector.Azure.Repositories
{
    public class ApiConfigurationRepository : BaseRepository<ApiConfigurationModel>
    {
        public ApiConfigurationRepository(string conn) : base(conn)
        {
        }

        public override async Task<IEnumerable<ApiConfigurationModel>> GetAllAsync(object whereObject = null, int? commandTimeout = null) {
            using var db = DbConnection;
            return await db.Conn.QueryAsync<ApiConfigurationModel>("sp_GetApiConfigurationDetails", whereObject, commandTimeout: 600,
                commandType: CommandType.StoredProcedure);
        }
    }
}
