using System.Threading.Tasks;
using CAD.Database;
using CAD.DataCollector.StorageAccountSize.Models;

namespace CAD.DataCollector.StorageAccountSize.Repositories
{
    public class StorageAccountSizeRepository : BaseRepository<StorageAccountSizeModel>
    {
        public StorageAccountSizeRepository(string conn, string tableName = null) : base(conn, tableName)
        {
        }

        public override async Task<int?> InsertAsync(StorageAccountSizeModel entity, int? commandTimeout = null)
        {
            const string query = "MERGE INTO [StorageAccountSize] AS TARGET USING (VALUES " +
                                 "(@TenantId, @AzureId, @DateCaptured, @NumberOfBlobContainers, @BlobSizeInGb, @NumberOfBlobs, @FileShareSizeInGb, @NumberOfFileShares)" +
                                 ") AS SOURCE (TenantId, AzureId, DateCaptured, NumberOfBlobContainers, BlobSizeInGb, NumberOfBlobs, FileShareSizeInGb, NumberOfFileShares) " +
                                 "ON SOURCE.TenantId = TARGET.TenantId AND SOURCE.AzureId = TARGET.AzureId AND SOURCE.DateCaptured = TARGET.DateCaptured " +
                                 "WHEN MATCHED THEN " +
                                 "UPDATE SET TARGET.NumberOfBlobContainers = SOURCE.NumberOfBlobContainers, TARGET.BlobSizeInGb = SOURCE.BlobSizeInGb, " +
                                 "TARGET.NumberOfBlobs = SOURCE.NumberOfBlobs, TARGET.FileShareSizeInGb = SOURCE.FileShareSizeInGb, " +
                                 "TARGET.NumberOfFileShares = SOURCE.NumberOfFileShares " +
                                 "WHEN NOT MATCHED THEN " +
                                 "INSERT (TenantId, AzureId, DateCaptured, NumberOfBlobContainers, BlobSizeInGb, NumberOfBlobs, FileShareSizeInGb, NumberOfFileShares) " +
                                 "VALUES (SOURCE.TenantId, SOURCE.AzureId, SOURCE.DateCaptured, SOURCE.NumberOfBlobContainers, SOURCE.BlobSizeInGb, SOURCE.NumberOfBlobs, " +
                                 "SOURCE.FileShareSizeInGb, SOURCE.NumberOfFileShares);";
            var db = DbConnection;
            return await db.ExecuteQueryAsync(query, entity);
        }
    }
}
