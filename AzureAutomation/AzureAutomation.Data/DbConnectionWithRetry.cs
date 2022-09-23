using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using AzureAutomation.Data.Helper;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace AzureAutomation.Data
{
    public class DbConnectionWithRetry : DbConnection
    {
        private readonly RetryPolicy _retryPolicy =
            new RetryPolicy<CustomSqlTransientErrorDetectionStrategy>(new ExponentialBackoff(5, new TimeSpan(0, 0, 0),
                new TimeSpan(0, 1, 0), new TimeSpan(0, 0, 2)));

        public DbConnectionWithRetry(System.Data.IDbConnection dbConnection, int? commandTimeout = null) : base(dbConnection, commandTimeout) { }

        public override void BeginTransaction()
        {
            _retryPolicy.ExecuteAction(() => base.BeginTransaction());
        }

        public override void CommitTransaction()
        {
            _retryPolicy.ExecuteAction(() => base.CommitTransaction());
        }

        public override Task<T> GetAsync<T>(object id, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.GetAsync<T>(id, commandTimeout));
        }

        public override Task<T> GetAsync<T>(string query, object parameters = null, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.GetAsync<T>(query, parameters, commandTimeout));
        }

        public override Task<IEnumerable<T>> GetListAsync<T>(int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.GetListAsync<T>(commandTimeout));
        }

        public override Task<IEnumerable<T>> GetListAsync<T>(string query, object parameters = null, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.GetListAsync<T>(query, parameters, commandTimeout));
        }

        public override Task<int> ExecuteQueryAsync(string query, object parameters = null, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.ExecuteQueryAsync(query, parameters, commandTimeout));
        }

        public override Task<int> InsertAsync<T>(T entityToInsert, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.InsertAsync(entityToInsert, commandTimeout));
        }

        public override Task<int> InsertAsync<T>(IEnumerable<T> entityToInsert, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.InsertAsync(entityToInsert, commandTimeout));
        }

        public override Task<bool> UpdateAsync<T>(T entityToInsert, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.UpdateAsync(entityToInsert, commandTimeout));
        }

        public override Task<bool> UpdateAsync<T>(IEnumerable<T> entityToInsert, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.UpdateAsync(entityToInsert, commandTimeout));
        }

        public override Task<bool> DeleteAsync<T>(T entityToInsert, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.DeleteAsync(entityToInsert, commandTimeout));
        }

        public override Task<bool> DeleteAsync<T>(IEnumerable<T> entityToInsert, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.DeleteAsync(entityToInsert, commandTimeout));
        }

        public override Task<int> GetRecordCountAsync<T>(string conditions = null, object parameters = null, int? commandTimeout = null)
        {
            return _retryPolicy.ExecuteAction(() => base.GetRecordCountAsync<T>(conditions, parameters, commandTimeout));
        }
    }
}