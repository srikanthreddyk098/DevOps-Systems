using System.Collections.Generic;
using System.Threading.Tasks;
using CalpineAzureDashboard.Models;

namespace CalpineAzureDashboard.DataCollector.Interface
{
    public interface IInventory<T> where T : AzureModel
    {
        Task<IEnumerable<T>> GetInventoryAsync(IEnumerable<T> inventory = null);
        Task<IEnumerable<T>> ProcessInventoryAsync(IEnumerable<T> inventory);
    }
}