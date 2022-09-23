using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.DataCollector.Interface;
using CalpineAzureDashboard.Models;
using log4net;

namespace CalpineAzureDashboard.DataCollector.Base
{
    public abstract class BaseInventory<T> : IInventory<T> where T : AzureModel
    {
        protected readonly IRepository<T> Repository;
        protected readonly ILog Log = LogManager.GetLogger(typeof(T));
        protected readonly string InventoryType;

        protected BaseInventory(IRepository<T> repository, string inventoryType)
        {
            Repository = repository;
            InventoryType = inventoryType;
        }

        public abstract Task<IEnumerable<T>> GetInventoryAsync(IEnumerable<T> inventory = null);
        public abstract Task<IEnumerable<T>> ProcessInventoryAsync(IEnumerable<T> inventoryParam);

        public virtual async Task<int> InsertInventoryAsync(IEnumerable<T> itemsToInsert)
        {
            var recordsInserted = 0;
            foreach (var item in itemsToInsert) {
                try {
                    item.Id = await Repository.InsertAsync(item);
                    if (item.Id == null) {
                        Log.Error($"Something went wrong inserting {InventoryType} with Azure id: {item.AzureId}. The new id is null.");
                    }
                    else {
                        recordsInserted++;
                    }
                }
                catch (Exception ex) {
                    Log.Error($"An exception occurred inserting {InventoryType} with Azure id: {item.AzureId}.", ex);
                }
            }

            return recordsInserted;
        }

        public virtual async Task<int> UpdateInventoryAsync(IEnumerable<T> itemsToUpdate)
        {
            var recordsUpdated = 0;
            foreach (var item in itemsToUpdate) {
                try {
                    var queryResult = await Repository.UpdateAsync(item);
                    if (queryResult != 1) {
                        Log.Error($"Something went wrong updating {InventoryType} with Azure id: {item.AzureId}. {queryResult} records inserted. Expected 1.");
                    }
                    else {
                        recordsUpdated++;
                    }
                }
                catch (Exception ex) {
                    Log.Error($"An exception occurred updating {InventoryType} with Azure id: {item.AzureId}.", ex);
                }
            }

            return recordsUpdated;
        }

        public virtual async Task<int> DeleteInventoryAsync(IEnumerable<T> itemsToDelete)
        {
            var recordsDeleted = 0;
            foreach (var item in itemsToDelete) {
                try {
                    var queryResult = await Repository.DeleteAsync(item);
                    if (queryResult != 1) {
                        Log.Error($"Something went wrong deleting {InventoryType} with Azure id: {item.AzureId}. {queryResult} records inserted. Expected 1.");
                    }
                    else {
                        recordsDeleted++;
                    }
                }
                catch (Exception ex) {
                    Log.Error($"An exception occurred deleting {InventoryType} with Azure id: {item.AzureId}.", ex);
                }
            }

            return recordsDeleted;
        }
    }
}