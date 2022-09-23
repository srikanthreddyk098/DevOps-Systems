using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CalpineAzureDashboard.Data.Repository
{
    public class VirtualMachineExtensionRepository<T> : BaseRepository<T>
    {
        public VirtualMachineExtensionRepository(string conn) : base(conn)
        {
        }

        //public override async Task<IEnumerable<T>> GetCollectionAsync(object whereObject = null)
        //{
        //    return await base.GetCollectionAsync(new {IsDeleted = 0});
        //}

        //public override async Task<int> DeleteAsync(T entity)
        //{
        //    using (var db = DbConnection) {
        //        var idProperties = db.GetIdProperties(entity.GetType()).ToList();

        //        string query =
        //            $"Update {db.GetTableName(entity.GetType())} " +
        //            $"Set {db.Encapsulate("IsDeleted")} = 1 " +
        //            $"where {db.GetWhereClauseFromIdColumns(idProperties)}";

        //        return await db.ExecuteQueryAsync(query, entity);
        //    }
        //}
    }
}