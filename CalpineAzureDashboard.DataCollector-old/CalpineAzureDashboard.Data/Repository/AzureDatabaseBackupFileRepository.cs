using System.Collections.Generic;
using System.Threading.Tasks;
using CalpineAzureDashboard.Models;

namespace CalpineAzureDashboard.Data.Repository
{
    public class AzureDatabaseBackupFileRepository<T> : BaseRepository<T>
    {
        public AzureDatabaseBackupFileRepository(string conn) : base(conn)
        {
        }

        public async Task<IEnumerable<DatabaseBackupStorageAccount>> GetDatabaseBackupStorageAccounts()
        {
            using (var db = DbConnection) {
                const string query = "SELECT [SubscriptionId], [Subscription], [ResourceGroup], [Name] FROM [dbo].[AzureDatabaseBackupStorageAccount]";
                return await db.GetCollectionAsync<DatabaseBackupStorageAccount>(query, null, null);
            }
        }
    }
}