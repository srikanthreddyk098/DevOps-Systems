using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CAD.Database;
using CAD.Database.Bulk;
using CAD.DataCollector.Azure.Models;

namespace CAD.DataCollector.Azure.Repositories
{
    public class AzureResourceSkuRepository : BaseRepository<AzureResourceSkuModel>
    {
        public AzureResourceSkuRepository(string conn) : base(conn)
        {
        }

        public override async Task<bool> InsertAsync(IEnumerable<AzureResourceSkuModel> entities, int? commandTimeout = null)
        {
            var bulk = new BulkOperations();
            bulk.Setup<AzureResourceSkuModel>(x => x.ForCollection(entities))
                .WithTable("AzureResourceSku")
                .WithBulkCopyBatchSize(5000)
                .WithBulkCopyCommandTimeout(commandTimeout ?? 60)
                .WithSqlCommandTimeout(commandTimeout ?? 600)
                .WithBulkCopyEnableStreaming(true)
                .AddAllColumns()
                .TmpDisableAllNonClusteredIndexes()
                .BulkMerge()
                .SetIdentityColumn(x => x.Id)
                .MatchTargetOn(x => x.AzureId)
                .DeleteWhenNotMatched(true);
            await bulk.CommitTransactionAsync((SqlConnection)DbConnection.Conn);
            return true;
        }
    }
}
