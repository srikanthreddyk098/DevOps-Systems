using System.Collections.Generic;
using System.Threading.Tasks;
using CAD.Database;
using CAD.DataCollector.DatabaseBackupFile.Models;

namespace CAD.DataCollector.DatabaseBackupFile.Repositories
{
    public class DatabaseBackupFileRepository : BaseRepository<DatabaseBackupFileModel>
    {
        public DatabaseBackupFileRepository(string conn, string tableName = null) : base(conn, tableName)
        {
        }

        public override async Task<bool> InsertAsync(IEnumerable<DatabaseBackupFileModel> entities, int? commandTimeout = null)
        {
            using var db = DbConnection;
            return await db.BulkInsert(entities);

            //var bulk = new BulkOperations();
            //bulk.Setup<AzureDatabaseBackupFileModel>(x => x.ForCollection(entities))
            //    .WithTable("AzureDatabaseBackupFile")
            //    .WithBulkCopyBatchSize(5000)
            //    .WithBulkCopyCommandTimeout(commandTimeout ?? 60)
            //    .WithSqlCommandTimeout(commandTimeout ?? 600)
            //    .WithBulkCopyEnableStreaming(true)
            //    .AddAllColumns()
            //    .TmpDisableAllNonClusteredIndexes()
            //    .BulkMerge()
            //    .SetIdentityColumn(x => x.Id)
            //    .MatchTargetOn(x => x.StorageAccountAzureId)
            //    .MatchTargetOn(x => x.ServerName)
            //    .MatchTargetOn(x => x.InstanceName)
            //    .MatchTargetOn(x => x.DatabaseName)
            //    .MatchTargetOn(x => x.BackupFileName);
            //await bulk.CommitTransactionAsync((SqlConnection)DbConnection.Conn);
            //return true;
        }

        public async Task<int> TruncateStagingTable()
        {
            const string query = "TRUNCATE TABLE [DatabaseBackupFile_stage];";
            using var db = DbConnection;
            return await db.ExecuteQueryAsync(query);
        }

        public async Task<int> ProcessStagingTable()
        {
            const string query = "TRUNCATE TABLE [DatabaseBackupFile];" +
                                 "INSERT INTO [DatabaseBackupFile] (TenantId, SubscriptionId, Subscription, ResourceGroup, StorageAccountAzureId, StorageAccountName, ServerName, InstanceName, DatabaseName, BackupFileName, LastModifiedDateUtc, SizeInMb) " +
                                 "SELECT TenantId, SubscriptionId, Subscription, ResourceGroup, StorageAccountAzureId, StorageAccountName, ServerName, InstanceName, DatabaseName, BackupFileName, LastModifiedDateUtc, SizeInMb " +
                                 "FROM[DatabaseBackupFile_stage];";
            using var db = DbConnection;
            return await db.ExecuteQueryAsync(query);
        }
    }
}