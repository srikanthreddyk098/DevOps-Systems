using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CAD.Database.Bulk
{
    internal interface ITransaction
    {
        void CommitTransaction(string connectionName = null, SqlCredential credentials = null, SqlConnection connection = null);
        Task CommitTransactionAsync(string connectionName = null, SqlCredential credentials = null, SqlConnection connection = null);
    }
}
