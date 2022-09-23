using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.Models;

namespace CalpineAzureDashboard.DataCollector.Inventory
{
    public class DataDiskInventory : AzureInventory<DataDiskModel>
    {
        public DataDiskInventory(AzureService azureService, IRepository<DataDiskModel> repository) : 
            base(azureService, repository, "data disk", "Create or Update Disk") { }

        public override async Task<IEnumerable<DataDiskModel>> GetInventoryAsync(IEnumerable<DataDiskModel> inventoryParam = null)
        {
            if (inventoryParam != null) {
                return inventoryParam;
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s started at: {DateTime.Now}");
            var inventory = new List<DataDiskModel>();

            foreach (var subscription in Subscriptions) {
                try {
                    var dataDisks = AzureService.GetDataDisks(subscription.SubscriptionId);

                    foreach (var dataDisk in dataDisks) {
                        try {
                            var newDataDisk = new DataDiskModel();
                            newDataDisk.AzureId = dataDisk.Id;
                            newDataDisk.SubscriptionId = subscription.SubscriptionId;
                            newDataDisk.Subscription = subscription.DisplayName;
                            newDataDisk.ResourceGroup = dataDisk.ResourceGroupName;
                            newDataDisk.Region = dataDisk.RegionName;
                            newDataDisk.Name = dataDisk.Name;
                            newDataDisk.DiskState = dataDisk.Inner.DiskState?.Value;
                            newDataDisk.SizeInGb = dataDisk.Inner.DiskSizeGB;
                            newDataDisk.DiskIOPSReadWrite = dataDisk.Inner.DiskIOPSReadWrite;
                            newDataDisk.DiskMBpsReadWrite = dataDisk.Inner.DiskMBpsReadWrite;
                            newDataDisk.Os = dataDisk.OSType?.ToString();
                            newDataDisk.CreationMethod = dataDisk.Inner.CreationData?.CreateOption?.Value;
                            newDataDisk.ImageId = dataDisk.Inner.CreationData?.ImageReference?.Id;
                            newDataDisk.Sku = dataDisk.Inner.Sku?.Name?.Value;
                            newDataDisk.IsAttachedToVm = dataDisk.IsAttachedToVirtualMachine;
                            newDataDisk.VirtualMachineAzureId = dataDisk.VirtualMachineId;
                            newDataDisk.IsEncryptionEnabled = dataDisk.Inner.EncryptionSettingsCollection?.Enabled;
                            newDataDisk.CreatedDtUtc = dataDisk.Inner.TimeCreated?.ToUniversalTime();

                            inventory.Add(newDataDisk);
                        }
                        catch (Exception ex) {
                            Log.Error($"An exception occurred getting details for {InventoryType}: {dataDisk.Id}", ex);
                        }
                    }
                }
                catch (Exception ex) {
                    throw new Exception($"An exception occurred getting {InventoryType} inventory for subscription: {subscription.DisplayName}.", ex);
                }
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s finished at: {DateTime.Now}");
            return inventory;
        }

        public override async Task<IEnumerable<DataDiskModel>> ProcessInventoryAsync(IEnumerable<DataDiskModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var itemsToInsert = new List<DataDiskModel>();
            var itemsToUpdate = new List<DataDiskModel>();

            var existingInventory = (await Repository.GetCollectionAsync()).ToList();

            Log.Debug($"Processing Azure inventory for {inventory.Count} {InventoryType}s started at: {DateTime.Now}");
            foreach (var item in inventory) {
                try {
                    if (existingInventory.Count.Equals(0)) {
                        itemsToInsert.Add(item);
                        continue;
                    }

                    var existingItem = existingInventory.FirstOrDefault(x => x.AzureId.Equals(item.AzureId, StringComparison.OrdinalIgnoreCase));

                    if (existingItem == null) {
                        //get the first "Create or Update" event to determine who created the resource and when
                        try {
                            var createdEvent = GetCreatedEvent(item.SubscriptionId, item.ResourceGroup, item.AzureId, item.CreatedDtUtc,
                                item.CreatedDtUtc?.Add(new TimeSpan(0, 0, 1)));
                            item.CreatedBy = createdEvent?.Caller;
                        }
                        catch (Exception ex) {
                            Log.Error($"Something went wrong getting created event from the activity logs for {InventoryType} with Azure id: {item.AzureId}.", ex);
                        }

                        itemsToInsert.Add(item);
                    }
                    else {
                        item.Id = existingItem.Id;
                        item.CreatedBy = existingItem.CreatedBy;
                        item.CreatedDtUtc = existingItem.CreatedDtUtc;

                        if (!item.IsEqual(existingItem)) {
                            itemsToUpdate.Add(item);
                        }
                    }
                }
                catch (Exception ex) {
                    Log.Error($"An exception occurred checking whether {InventoryType} already exists in the database with id {item.AzureId}.", ex);
                }
            }

            var itemsToDelete = existingInventory.Where(x => !inventory.Any(y => y.AzureId.Equals(x.AzureId, StringComparison.OrdinalIgnoreCase)));

            var recordsDeleted = await DeleteInventoryAsync(itemsToDelete);
            var recordsUpdated = await UpdateInventoryAsync(itemsToUpdate);
            var recordsInserted = await InsertInventoryAsync(itemsToInsert);

            Log.Debug($"  Number of {InventoryType} records inserted: {recordsInserted}");
            Log.Debug($"  Number of {InventoryType} records updated: {recordsUpdated}");
            Log.Debug($"  Number of {InventoryType} records deleted: {recordsDeleted}");

            Log.Debug($"Processing Azure inventory for {inventory.Count} {InventoryType}s finished at: {DateTime.Now}");

            return inventory;
        }
    }
}