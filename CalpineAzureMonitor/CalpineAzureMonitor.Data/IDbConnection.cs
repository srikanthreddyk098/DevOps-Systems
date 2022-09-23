using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;

namespace CalpineAzureMonitor.Data
{
    public interface IDbConnection : IDisposable
    {
        void BeginTransaction();
        void CommitTransaction();
        Task<T> GetAsync<T>(string query, object parameters = null, int? commandTimeout = null);
        Task<T> GetAsync<T>(object id, int? commandTimeout = null);

        Task<IEnumerable<T>> GetCollectionAsync<T>(object whereObject = null, int? commandTimeout = null,
            CommandType commandType = CommandType.Text);

        Task<IEnumerable<T>> GetCollectionAsync<T>(string query, object parameters = null,
            int? commandTimeout = null, CommandType commandType = CommandType.Text);

        Task<IEnumerable<T>> GetPagedCollectionAsync<T>(int pageNumber, int rowsPerPage, string conditions,
            string orderby, object parameters = null, int? commandTimeout = null,
            CommandType commandType = CommandType.Text);

        Task<int?> InsertAsync<TEntity>(TEntity entityToInsert, int? commandTimeout = null);
        Task<TKey> InsertAsync<TKey, TEntity>(TEntity entityToInsert, int? commandTimeout = null);
        Task<int> UpdateAsync<TEntity>(TEntity entityToUpdate, int? commandTimeout = null);
        Task<int> DeleteAsync<T>(T entityToDelete, int? commandTimeout = null);
        Task<int> DeleteAsync<T>(object id, int? commandTimeout = null);
        Task<int> DeleteCollectionAsync<T>(object whereConditions, int? commandTimeout = null);
        Task<int> DeleteCollectionAsync<T>(string conditions, object parameters = null, int? commandTimeout = null);
        Task<int> ExecuteQueryAsync(string query, object parameters = null, int? commandTimeout = null);
        Task ExecuteStoredProcedureAsync(string storedProcedureName, object parameters = null, int? commandTimeout = null);
        Task<int> GetRecordCountAsync<T>(string conditions = "", object parameters = null, int? commandTimeout = null);
        Task<int> GetRecordCountAsync<T>(object whereConditions, int? commandTimeout = null);

        string GetTableName(Type type);
        string Encapsulate(string databaseword);
        IEnumerable<PropertyInfo> GetIdProperties(Type type);
        string GetWhereClauseFromIdColumns(IEnumerable<PropertyInfo> idProperties);
    }
}
