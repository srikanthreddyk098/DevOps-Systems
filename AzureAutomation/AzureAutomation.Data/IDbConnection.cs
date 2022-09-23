using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AzureAutomation.Data
{
    public interface IDbConnection : IDisposable
    {
        void BeginTransaction();
        void CommitTransaction();
        Task<T> GetAsync<T>(object id, int? commandTimeout = null);
        Task<T> GetAsync<T>(string query, object parameters = null, int? commandTimeout = null);
        Task<IEnumerable<T>> GetListAsync<T>(int? commandTimeout = null);
        Task<IEnumerable<T>> GetListAsync<T>(object whereConditions, int? commandTimeout = null);
        Task<IEnumerable<T>> GetListAsync<T>(string query, object parameters = null, int? commandTimeout = null);

        Task<IEnumerable<T>> GetPagedListAsync<T>(string query, int pageNumber, int rowsPerPage, string whereConditions = null, object parameters = null,
            int? commandTimeout = null);

        Task<IEnumerable<T>> GetPagedListAsync<T>(int pageNumber, int rowsPerPage, string whereConditions, string orderBy, object parameters = null,
            int? commandTimeout = null);

        Task<int> ExecuteQueryAsync(string query, object parameters = null, int? commandTimeout = null);
        Task<int> InsertAsync<T>(T entity, int? commandTimeout = null) where T : class;
        Task<int> InsertAsync<T>(IEnumerable<T> entities, int? commandTimeout = null) where T : class;
        Task<bool> UpdateAsync<T>(T entity, int? commandTimeout = null) where T : class;
        Task<bool> UpdateAsync<T>(IEnumerable<T> entities, int? commandTimeout = null) where T : class;
        Task<bool> DeleteAsync<T>(T entity, int? commandTimeout = null) where T : class;
        Task<bool> DeleteAsync<T>(IEnumerable<T> entities, int? commandTimeout = null) where T : class;
        Task<int> GetRecordCountAsync<T>(string whereConditions = null, object parameters = null, int? commandTimeout = null);
    }
}
