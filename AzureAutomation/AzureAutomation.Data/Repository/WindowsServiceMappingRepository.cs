using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using AzureAutomation.Data.Models;
using AzureAutomation.Models;

namespace AzureAutomation.Data.Repository
{
    public class WindowsServiceMappingRepository
    {
        private readonly string _conn;

        public WindowsServiceMappingRepository(string conn)
        {
            _conn = conn;
        }
        
        public async Task<IEnumerable<WindowsServiceMappingModel>> GetAllAsync()
        {
            using IDbConnection db = new DbConnectionWithRetry(new SqlConnection(_conn));
            return await db.GetListAsync<WindowsServiceMappingModel>();
        }

        public async Task<WindowsServiceMappingModel> GetAsync(int id)
        {
            using IDbConnection db = new DbConnectionWithRetry(new SqlConnection(_conn));
            return await db.GetAsync<WindowsServiceMappingModel>(id);
        }

        public async Task<int> LogChangesAsync(string action, List<Tuple<string, string, string, string, string>> changedValues, string user)
        {
            if (changedValues.Count < 1) throw new Exception("No changes to log");

            using IDbConnection db = new DbConnectionWithRetry(new SqlConnection(_conn));
            var currentDateTime = DateTime.Now;
            var changeList = new List<WindowsServiceMappingAudit>();
            foreach (var changedValue in changedValues) {
                var changes = new WindowsServiceMappingAudit
                {
                    Vm = changedValue.Item1,
                    Service = changedValue.Item2,
                    Action = action,
                    Type = changedValue.Item3,
                    OldValue = changedValue.Item4,
                    NewValue = changedValue.Item5,
                    ChangedBy = user,
                    ChangedDt = currentDateTime
                };
                changeList.Add(changes);
            }

            return await db.InsertAsync<WindowsServiceMappingAudit>(changeList);
        }
    }
}
