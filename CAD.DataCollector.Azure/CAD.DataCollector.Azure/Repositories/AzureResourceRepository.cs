using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CAD.Database;
using CAD.DataCollector.Azure.Models;
using Dapper;

namespace CAD.DataCollector.Azure.Repositories
{
    public class AzureResourceRepository : BaseRepository<AzureResourceModel>
    {
        public AzureResourceRepository(string conn, string tableName = null) : base(conn, tableName)
        {
        }

        public override async Task<IEnumerable<AzureResourceModel>> GetAllAsync(object whereObject = null, int? commandTimeout = null)
        {
            using var db = DbConnection; ;
            return await db.Conn.QueryAsync<AzureResourceModel>("sp_GetAzureResources", whereObject, commandType: CommandType.StoredProcedure);
        }
    }
}
