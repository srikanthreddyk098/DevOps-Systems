using System.Collections.Generic;
using System.Threading.Tasks;

namespace CalpineAzureDashboard.Data.Repository
{
    public interface IRepository<T>
    {
        Task<int?> InsertAsync(T entity);
        Task<int> UpdateAsync(T entity);
        Task<int> DeleteAsync(T entity);
        Task<IEnumerable<T>> GetCollectionAsync(object whereObject = null);
        Task<IEnumerable<T>> GetCollectionAsync(string whereClause);
        Task<T> Get(string query, object whereObject = null);
    }
}