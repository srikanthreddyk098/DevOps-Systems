using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using AzureAutomation.Models;
using Dapper;

namespace AzureAutomation.Data.Repository
{
    public class VmRepository
    {
        private readonly string _conn;

        public VmRepository(string conn)
        {
            _conn = conn;
        }

        public async Task<IEnumerable<VmModel>> GetAllVmsAsync()
        {
            using (IDbConnection db = new DbConnectionWithRetry(new SqlConnection(_conn))) {
                const string query = "SELECT [Id], [SubscriptionId], [Subscription], [ResourceGroup], [Name], [Location], [Os], [Size], [Tags] FROM [vw_VirtualMachine]";
                return await db.GetListAsync<VmModel>(query);
            }
        }
    }
}
