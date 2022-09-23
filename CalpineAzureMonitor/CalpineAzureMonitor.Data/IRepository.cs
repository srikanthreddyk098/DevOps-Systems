using System.Collections.Generic;
using System.Threading.Tasks;

namespace CalpineAzureMonitor.Data
{
    public interface IRepository<T>
    {
        Task<int?> InsertAsync(T entity);
        Task<int> UpdateAsync(T entity);
        Task<int> DeleteAsync(T entity);
        Task<IEnumerable<T>> GetCollectionAsync(object whereObject = null);
        Task<IEnumerable<T>> GetCollectionAsync(string whereCondition);
        Task<T> Get(string query, object whereObject = null);
    }
}