using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;

namespace AzureAutomation.Data
{
    public partial class DbConnection : IDbConnection
    {
        private readonly System.Data.IDbConnection _conn;
        private IDbTransaction _transaction;
        private readonly int _commandTimeout;


        public DbConnection(System.Data.IDbConnection conn, int? commandTimeout = null)
        {
            _conn = conn;
            _commandTimeout = commandTimeout ?? 60;
        }

        public virtual void BeginTransaction()
        {
            if (_conn.State == ConnectionState.Open) {
                if (_transaction == null) {
                    _transaction = _conn.BeginTransaction();
                }
            }
            else {
                _conn.Open();
                _transaction = _conn.BeginTransaction();
            }
        }

        public virtual void CommitTransaction()
        {
            try {
                _transaction?.Commit();
            }
            catch {
                _transaction?.Rollback();
                throw;
            }
            finally {
                _transaction?.Dispose();
                _conn.Dispose();
                _transaction = null;
            }
        }

        public virtual Task<T> GetAsync<T>(object id, int? commandTimeout = null)
        {
            return _conn.GetAsync<T>(id, _transaction, commandTimeout);
        }

        public virtual Task<T> GetAsync<T>(string query, object parameters = null, int? commandTimeout = null)
        {
            return _conn.QueryFirstOrDefaultAsync<T>(query, parameters, _transaction, commandTimeout ?? _commandTimeout);
        }

        public virtual Task<IEnumerable<T>> GetListAsync<T>(int? commandTimeout = null)
        {
            return _conn.GetListAsync<T>(null, _transaction, commandTimeout ?? _commandTimeout);
        }

        public virtual Task<IEnumerable<T>> GetListAsync<T>(object whereConditions, int? commandTimeout = null)
        {
            return _conn.GetListAsync<T>(whereConditions, _transaction, commandTimeout ?? _commandTimeout);
        }

        public virtual Task<IEnumerable<T>> GetListAsync<T>(string query, object parameters = null, int? commandTimeout = null)
        {
            return _conn.QueryAsync<T>(query, parameters, _transaction, commandTimeout ?? _commandTimeout);
        }

        public virtual Task<int> ExecuteQueryAsync(string query, object parameters = null, int? commandTimeout = null)
        {
            return _conn.ExecuteAsync(query, parameters, _transaction, commandTimeout ?? _commandTimeout);
        }

        public virtual Task<IEnumerable<T>> GetPagedListAsync<T>(int pageNumber, int rowsPerPage, string whereConditions, string orderBy,
            object parameters = null, int? commandTimeout = null)
        {
            return _conn.GetListPagedAsync<T>(pageNumber, rowsPerPage, whereConditions, orderBy, parameters, _transaction, commandTimeout ?? _commandTimeout);
        }

        public virtual Task<IEnumerable<T>> GetPagedListAsync<T>(string query, int pageNumber, int rowsPerPage, string whereConditions = null,
            object parameters = null, int? commandTimeout = null)
        {
            query += " OFFSET @Offset ROWS FETCH NEXT @RowsPerPage ROWS ONLY";
            return _conn.QueryAsync<T>(query, new {Offset = pageNumber * rowsPerPage, RowsPerPage = rowsPerPage});
        }

        public virtual Task<int> InsertAsync<T>(T entity, int? commandTimeout = null) where T : class
        {
            return SqlMapperExtensions.InsertAsync(_conn, entity, _transaction, commandTimeout ?? _commandTimeout);
        }

        public virtual Task<int> InsertAsync<T>(IEnumerable<T> entities, int? commandTimeout = null) where T : class
        {
            return SqlMapperExtensions.InsertAsync(_conn, entities, _transaction, commandTimeout ?? _commandTimeout);
        }

        public virtual Task<bool> UpdateAsync<T>(T entity, int? commandTimeout = null) where T : class
        {
            return SqlMapperExtensions.UpdateAsync(_conn, entity, _transaction, commandTimeout ?? _commandTimeout);
        }

        public virtual Task<bool> UpdateAsync<T>(IEnumerable<T> entities, int? commandTimeout = null) where T : class
        {
            return SqlMapperExtensions.UpdateAsync(_conn, entities, _transaction, commandTimeout ?? _commandTimeout);
        }

        public virtual Task<bool> DeleteAsync<T>(T entity, int? commandTimeout = null) where T : class
        {
            return SqlMapperExtensions.DeleteAsync(_conn, entity, _transaction, commandTimeout ?? _commandTimeout);
        }

        public virtual Task<bool> DeleteAsync<T>(IEnumerable<T> entities, int? commandTimeout = null) where T : class
        {
            return SqlMapperExtensions.DeleteAsync(_conn, entities, _transaction, commandTimeout ?? _commandTimeout);
        }

        public virtual Task<int> GetRecordCountAsync<T>(string whereConditions = null, object parameters = null, int? commandTimeout = null)
        {
            return _conn.RecordCountAsync<T>(whereConditions, parameters, _transaction, commandTimeout ?? _commandTimeout);
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _conn.Dispose();
            _transaction = null;
        }
    }
}
