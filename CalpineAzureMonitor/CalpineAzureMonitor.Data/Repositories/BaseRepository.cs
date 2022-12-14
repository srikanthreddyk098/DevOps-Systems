using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CalpineAzureMonitor.Data.Repositories
{
    public class BaseRepository<T> : IRepository<T>
    {
        private readonly string _conn;

        protected IDbConnection DbConnection => new DbConnection(new SqlConnection(_conn));

        protected BaseRepository(string conn)
        {
            _conn = conn;
        }

        public virtual async Task<int?> InsertAsync(T entity)
        {
            using (var db = DbConnection)
            {
                return await db.InsertAsync(entity) ?? -1;
            }
        }

        public virtual async Task<int> UpdateAsync(T entity)
        {
            using (var db = DbConnection)
            {
                return await db.UpdateAsync(entity);
            }
        }

        public virtual async Task<int> DeleteAsync(T entity)
        {
            using (var db = DbConnection)
            {
                return await db.DeleteAsync(entity);
            }
        }

        public virtual async Task<IEnumerable<T>> GetCollectionAsync(object whereObject = null)
        {
            using (var db = new DbConnection(new SqlConnection(_conn)))
            {
                return await db.GetCollectionAsync<T>(whereObject);
            }
        }

        public virtual async Task<IEnumerable<T>> GetCollectionAsync(string whereCondition)
        {
            using (var db = DbConnection)
            {
                return await db.GetCollectionAsync<T>(whereCondition);
            }
        }

        public virtual async Task<T> Get(string query, object whereObject = null)
        {
            using (var db = new DbConnection(new SqlConnection(_conn)))
            {
                return await db.GetAsync<T>(query, whereObject);
            }
        }
    }
}