using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using AzureAutomation.Data.Models;
using AzureAutomation.Models;

namespace AzureAutomation.Data.Repository
{
    public class PingMappingRepository
    {
        private readonly string _conn;

        public PingMappingRepository(string conn)
        {
            _conn = conn;
        }
        
        public async Task<IEnumerable<PingMappingModel>> GetAllAsync()
        {
            using IDbConnection db = new DbConnectionWithRetry(new SqlConnection(_conn));
            const string query = "EXEC [dbo].[sp_GetPingMapping]";
            return await db.GetListAsync<PingMappingModel>(query);
        }

        public async Task<PingMappingModel> GetAsync(int id)
        {
            using IDbConnection db = new DbConnectionWithRetry(new SqlConnection(_conn));
            const string query = "EXEC [dbo].[sp_GetPingMapping] @id";
            return await db.GetAsync<PingMappingModel>(query, new { id });
        }

        public async Task<int> LogChangesAsync(List<Tuple<string, string, string, string>> changedValues, string user)
        {
            if (changedValues.Count < 1) throw new Exception("No changes to log");

            using IDbConnection db = new DbConnectionWithRetry(new SqlConnection(_conn));
            var currentDateTime = DateTime.Now;
            var changeList = new List<PingMappingAudit>();
            foreach (var changedValue in changedValues) {
                var changes = new PingMappingAudit {
                    ResourceId = changedValue.Item1,
                    Type = changedValue.Item2,
                    OldValue = changedValue.Item3,
                    NewValue = changedValue.Item4,
                    ChangedBy = user,
                    ChangedDt = currentDateTime
                };
                changeList.Add(changes);
            }

            return await db.InsertAsync<PingMappingAudit>(changeList);
        }
    }
}
