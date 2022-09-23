using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace CAD.Database
{
    public class DbConnectionWithRetry : DbConnection
    {
        private readonly RetryPolicy _retryPolicy =
            new RetryPolicy<CustomSqlTransientErrorDetectionStrategy>(new ExponentialBackoff(5, new TimeSpan(0, 0, 0),
                new TimeSpan(0, 1, 0), new TimeSpan(0, 0, 2)));

        public DbConnectionWithRetry(IDbConnection dbConnection, string tableName = null, int defaultCommandTimeout = 60) : base(dbConnection, tableName, defaultCommandTimeout) { }

        public override void BeginTransaction()
        {
            _retryPolicy.ExecuteAction(() => base.BeginTransaction());
        }

        public override void CommitTransaction()
        {
            _retryPolicy.ExecuteAction(() => base.CommitTransaction());
        }

        public override Task<T> GetAsync<T>(string query, object parameters = null, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.GetAsync<T>(query, parameters, commandTimeout));
        }

        public override Task<T> GetAsync<T>(object whereObject, string query = null, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.GetAsync<T>(whereObject, query, commandTimeout));
        }

        public override Task<T> GetAsync<T>(object id, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.GetAsync<T>(id, commandTimeout));
        }

        public override Task<IEnumerable<T>> GetAllAsync<T>(object whereObject = null, int? commandTimeout = null, CommandType commandType = CommandType.Text)
        {
            return _retryPolicy.ExecuteAction(() => base.GetAllAsync<T>(whereObject, commandTimeout, commandType));
        }

        public override Task<IEnumerable<T>> GetAllAsync<T>(string query, object parameters = null, int? commandTimeout = null, CommandType commandType = CommandType.Text)
        {
            return _retryPolicy.ExecuteAction(() => base.GetAllAsync<T>(query, parameters, commandTimeout, commandType));
        }

        public override Task<IEnumerable<T>> GetAllPagedAsync<T>(int pageNumber, int rowsPerPage,
            string conditions, string orderBy, object parameters = null, int? commandTimeout = null,
            CommandType commandType = CommandType.Text)
        {
            return _retryPolicy.ExecuteAction(() => base.GetAllPagedAsync<T>(pageNumber, rowsPerPage, conditions,
                orderBy, parameters, commandTimeout, commandType));
        }

        public override Task<int?> InsertAsync<TEntity>(TEntity entityToInsert, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.InsertAsync(entityToInsert, commandTimeout));
        }

        public override Task<TKey> InsertAsync<TKey, TEntity>(TEntity entityToInsert, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.InsertAsync<TKey, TEntity>(entityToInsert, commandTimeout));
        }

        public override Task<bool> BulkInsert<T>(IEnumerable<T> entitiesToInsert)
        {
            return _retryPolicy.ExecuteAction(() => base.BulkInsert(entitiesToInsert));
        }

        public override Task<int> UpdateAsync<TEntity>(TEntity entityToUpdate, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.UpdateAsync(entityToUpdate, commandTimeout));
        }

        public override Task<int> DeleteAsync<T>(T entityToDelete, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.DeleteAsync(entityToDelete, commandTimeout));
        }

        public override Task<int> DeleteAsync<T>(object id, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.DeleteAsync<T>(id, commandTimeout));
        }

        public override Task<int> DeleteCollectionAsync<T>(object whereConditions, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.DeleteCollectionAsync<T>(whereConditions, commandTimeout));
        }

        public override Task<int> DeleteCollectionAsync<T>(string conditions, object parameters = null, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.DeleteCollectionAsync<T>(conditions, parameters, commandTimeout));
        }

        public override Task<int> ExecuteQueryAsync(string query, object parameters = null, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.ExecuteQueryAsync(query, parameters, commandTimeout));
        }

        public override Task<int> GetRecordCountAsync<T>(string conditions = "", object parameters = null, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.GetRecordCountAsync<T>(conditions, parameters, commandTimeout));
        }

        public override Task<int> GetRecordCountAsync<T>(object whereConditions, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.GetRecordCountAsync<T>(whereConditions, commandTimeout));
        }
    }
}