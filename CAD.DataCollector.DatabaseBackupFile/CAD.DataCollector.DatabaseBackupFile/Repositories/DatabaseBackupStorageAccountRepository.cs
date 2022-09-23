using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CAD.Database;
using CAD.Database.Bulk;
using CAD.DataCollector.DatabaseBackupFile.Models;

namespace CAD.DataCollector.DatabaseBackupFile.Repositories
{
    public class DatabaseBackupStorageAccountRepository : BaseRepository<DatabaseBackupStorageAccountModel>
    {
        public DatabaseBackupStorageAccountRepository(string conn) : base(conn)
        {
        }

        public async Task<IEnumerable<DatabaseBackupStorageAccountModel>> GetDatabaseBackupStorageAccounts()
        {
            //const string query = "SELECT [TenantId], [ClientId], [ClientSecret], [SubscriptionId], [Subscription], [ResourceGroup], [Name] " +
            //                     "FROM [vw_AzureDatabaseBackupStorageAccount]";
            using var db = DbConnection;
            return await db.GetAllAsync<DatabaseBackupStorageAccountModel>();
        }
    }
}