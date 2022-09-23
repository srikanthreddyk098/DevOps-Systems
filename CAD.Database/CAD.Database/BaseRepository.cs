using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CAD.Database
{
    public abstract class BaseRepository<T> : IRepository<T> where T : class
    {
        private readonly string _conn;
        private readonly string _tableName;
        private readonly int _defaultCommandTimeout;

        protected DbConnection DbConnection => new DbConnectionWithRetry(new SqlConnection(_conn), _tableName, _defaultCommandTimeout);

        protected BaseRepository(string conn, string tableName = null, int defaultCommandTimeout = 60)
        {
            _conn = conn;
            _tableName = tableName;
            _defaultCommandTimeout = defaultCommandTimeout;123
        }

        public virtual async Task<int> ExecuteAsync(string query, object whereObject = null, int? commandTimeout = null)
        {
            using var db = DbConnection;
            return await db.ExecuteQueryAsync(query, whereObject, commandTimeout);
        }

        public virtual async Task<T> GetAsync(string query, object whereObject = null, int? commandTimeout = null)
        {
            using var db = DbConnection;
            return await db.GetAsync<T>(query, whereObject, commandTimeout);
        }

        public virtual async Task<T> GetAsync(object whereObject = null, string query = null, int? commandTimeout = null)
        {
            using var db = DbConnection;
            return await db.GetAsync<T>(whereObject, null, commandTimeout);
        }

        public virtual async Task<T> GetAsync(object id = null, int? commandTimeout = null)
        {
            using var db = DbConnection;
            return await db.GetAsync<T>(id, commandTimeout);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(object whereObject = null, int? commandTimeout = null)
        {
            using var db = DbConnection;
            return await db.GetAllAsync<T>(whereObject, commandTimeout);
        }

        public virtual async Task<IEnumerable<T>> GetCollectionAsync(string query, object whereObject = null, int? commandTimeout = null)
        {
            using var db = DbConnection;
            return await db.GetCollectionAsync<T>(query, whereObject, commandTimeout);
        }

        public virtual async Task<int?> InsertAsync(T entity, int? commandTimeout = null)
        {
            using var db = DbConnection;
            return await db.InsertAsync(entity, commandTimeout) ?? -1;
        }

        //public virtual async Task<int> InsertAsync(IEnumerable<T> entities, int? commandTimeout = null)
        //{
        //    using var db = DbConnection;
        //    db.BeginTransaction();

        //    var count = 0;
        //    foreach (var entity in entities)
        //    {
        //        var result = await db.InsertAsync(entity, commandTimeout);
        //        if (result == null)
        //        {
        //            return 0;
        //        }

        //        count++;
        //    }

        //    db.CommitTransaction();

        //    return count;
        //}

        public virtual async Task<bool> InsertAsync(IEnumerable<T> entities, int? commandTimeout = null)
        {
            using var db = DbConnection;
            await db.BulkInsert(entities);
            return true;
        }

        public virtual async Task<int> UpdateAsync(T entity, int? commandTimeout = null)
        {
            using var db = DbConnection;
            return await db.UpdateAsync(entity, commandTimeout);
        }

        public virtual async Task<int> UpdateAsync(IEnumerable<T> entities, int? commandTimeout = null)
        {
            using var db = DbConnection;
            db.BeginTransaction();

            var count = 0;
            foreach (var entity in entities) {
                var result = await db.UpdateAsync(entity, commandTimeout);
                if (result < 1) {
                    return 0;
                }

                count++;
            }

            db.CommitTransaction();

            return count;
        }

        public virtual async Task<int> DeleteAsync(T entity, int? commandTimeout = null)
        {
            using var db = DbConnection;
            return await db.DeleteAsync(entity, commandTimeout);
        }

        public virtual async Task<int> DeleteAsync(IEnumerable<T> entities, int? commandTimeout = null)
        {
            using var db = DbConnection;
            db.BeginTransaction();

            var count = 0;
            foreach (var entity in entities) {
                var result = await db.DeleteAsync(entity, commandTimeout);
                if (result < 1) {
                    return 0;
                }

                count++;
            }

            db.CommitTransaction();

            return count;
        }
    }
}