using System.Collections.Generic;
using System.Threading.Tasks;

namespace CAD.Database
{
    public interface IRepository<T>
    {
        Task<int> ExecuteAsync(string query, object whereObject = null, int? commandTimeout = null);
        Task<T> GetAsync (string query, object whereObject = null, int? commandTimeout = null);
        Task<IEnumerable<T>> GetCollectionAsync (string query, object whereObject = null, int? commandTimeout = null);
        Task<T> GetAsync (object whereObject = null, int? commandTimeout = null);
        Task<IEnumerable<T>> GetAllAsync(object whereObject = null, int? commandTimeout = null);
        Task<int?> InsertAsync(T entity, int? commandTimeout = null);
        Task<bool> InsertAsync(IEnumerable<T> entities, int? commandTimeout = null);
        Task<int> UpdateAsync(T entity, int? commandTimeout = null);
        Task<int> UpdateAsync(IEnumerable<T> entities, int? commandTimeout = null);
        Task<int> DeleteAsync(T entity, int? commandTimeout = null);
        Task<int> DeleteAsync(IEnumerable<T> entities, int? commandTimeout = null);
    }
}