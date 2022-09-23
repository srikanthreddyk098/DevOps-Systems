using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.Models;
using Microsoft.Azure.Management.Compute.Fluent;
using Newtonsoft.Json;

namespace CalpineAzureDashboard.DataCollector.Inventory
{
    public class VirtualMachineInventory : AzureInventory<VirtualMachineModel>
    {
        private readonly object _lockObject = new object();

        public VirtualMachineInventory(AzureService azureService, IRepository<VirtualMachineModel> repository) : base(azureService, repository, "virtual machine",
                                                                                                                      "Create or Update Virtual Machine") { }

        public override async Task<IEnumerable<VirtualMachineModel>> GetInventoryAsync(IEnumerable<VirtualMachineModel> inventoryParam = null)
        {
            if (inventoryParam != null) {
                return inventoryParam;
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s started at: {DateTime.Now}");
            var inventory = new List<VirtualMachineModel>();

            IList<IVirtualMachineSize> vmSizes = null;
            
            foreach (var subscription in Subscriptions) {
                try {
                    vmSizes = (await AzureService.GetAllVirtualMachineSizesAsync(subscription.SubscriptionId)).ToList();
                    if (vmSizes.Any()) {
                        break;
                    }
                }
                catch (Exception ex) {
                    Log.Warn($"An exception occurred getting VM sizes using subscription '{subscription.DisplayName}'.", ex);
                }
            }
            
            foreach (var subscription in Subscriptions) {
                try {
                    var vms = await AzureService.GetVirtualMachinesInSubscriptionAsync(subscription.SubscriptionId);

                    var tasks = vms.Select(async vm =>
                    {
                        try {
                            var vmSize = vmSizes?.FirstOrDefault(x => x.Name.Equals(vm.Size.Value, StringComparison.OrdinalIgnoreCase));

                            var newVmInventory = new VirtualMachineModel();
                            newVmInventory.SubscriptionId = subscription.SubscriptionId;
                            newVmInventory.Subscription = subscription.DisplayName;
                            newVmInventory.ResourceGroup = vm.ResourceGroupName;
                            newVmInventory.Region = vm.Region?.Name;
                            newVmInventory.AzureId = vm.Id;
                            newVmInventory.Name = vm.Name;
                            newVmInventory.Status = vm.PowerState?.Value?.Split('/').LastOrDefault();
                            newVmInventory.Os = vm.StorageProfile?.OsDisk?.OsType?.ToString();
                            newVmInventory.OsSku = vm.StorageProfile?.ImageReference?.Sku;
                            newVmInventory.OsPublisher = vm.StorageProfile?.ImageReference?.Publisher;
                            newVmInventory.OsType = vm.StorageProfile?.ImageReference?.Offer;
                            newVmInventory.OsVersion = vm.StorageProfile?.ImageReference?.Version;
                            newVmInventory.LicenseType = vm.LicenseType;
                            newVmInventory.Size = vmSize?.Name;
                            newVmInventory.Cores = vmSize?.NumberOfCores;
                            newVmInventory.Memory = vmSize?.MemoryInMB / 1024;
                            newVmInventory.AvailabilitySetId = vm.AvailabilitySetId;
                            newVmInventory.NumberOfNics = vm.NetworkInterfaceIds.Count;
                            newVmInventory.PrimaryNicId = vm.PrimaryNetworkInterfaceId;
                            newVmInventory.OsDisk = vm.StorageProfile?.OsDisk?.Name ?? vm.StorageProfile?.OsDisk?.Vhd?.Uri;
                            newVmInventory.OsDiskSku = vm.OSDiskStorageAccountType?.Value;
                            newVmInventory.OsDiskSize = vm.StorageProfile?.OsDisk?.DiskSizeGB;
                            newVmInventory.IsOsDiskEncrypted = vm.StorageProfile?.OsDisk?.EncryptionSettings?.Enabled;
                            newVmInventory.IsManagedDiskEnabled = vm.IsManagedDiskEnabled;
                            newVmInventory.NumberOfDataDisks = vm.DataDisks.Count;
                            newVmInventory.AzureAgentProvisioningState = vm.InstanceView.VmAgent?.Statuses?.FirstOrDefault()?.DisplayStatus;
                            newVmInventory.AzureAgentVersion = vm.InstanceView.VmAgent?.VmAgentVersion;

                            if (vm.Tags != null && vm.Tags.Any()) {
                                newVmInventory.Tags = JsonConvert.SerializeObject(vm.Tags);

                                try {
                                    //serialize the Dictionary<string, string> and replace the escaped quotes
                                    var serializedString = JsonConvert.SerializeObject(vm.Tags).Replace("\\", "").Replace("\"{", "{").Replace("}\"", "}");
                                    var tags = JsonConvert.DeserializeObject<TagModel>(serializedString);
                                    newVmInventory.TagApplicationName = string.IsNullOrEmpty(tags.Application_Name) ? null : tags.Application_Name;
                                    newVmInventory.TagBackupFrequency = string.IsNullOrEmpty(tags.Backup?.Frequency) ? null : tags.Backup.Frequency;
                                    newVmInventory.TagBackupPolicy = string.IsNullOrEmpty(tags.Backup?.Policy) ? null : tags.Backup.Policy;
                                    newVmInventory.TagProjectCode = string.IsNullOrEmpty(tags.Project_Info?.Code) ? null : tags.Project_Info.Code;
                                    newVmInventory.TagProjectDurationStart = string.IsNullOrEmpty(tags.Project_Info?.Duration?.Start?.ToShortDateString())
                                        ? null
                                        : tags.Project_Info?.Duration?.Start?.ToShortDateString();
                                    newVmInventory.TagProjectDurationEnd = string.IsNullOrEmpty(tags.Project_Info?.Duration?.End?.ToShortDateString())
                                        ? null
                                        : tags.Project_Info?.Duration?.End?.ToShortDateString();
                                    newVmInventory.TagReservedInstance = string.IsNullOrEmpty(tags.Reserved_Instance) ? null : tags.Reserved_Instance;
                                    newVmInventory.TagServerType = string.IsNullOrEmpty(tags.Server_Type) ? null : tags.Server_Type;
                                }
                                catch (Exception ex) {
                                    Log.Error($"An exception occurred getting tags for {InventoryType} with Azure id: {vm.Id}", ex);
                                }
                            }

                            newVmInventory.Extensions = await GetExtensionsForVm(vm);

                            lock (_lockObject) {
                                inventory.Add(newVmInventory);
                            }
                        }
                        catch (Exception ex) {
                            Log.Error($"An exception occurred getting details for {InventoryType} with Azure id: {vm.Id}", ex);
                        }
                    });

                    await Task.WhenAll(tasks);
                }
                catch (Exception ex) {
                    throw new Exception($"An exception occurred getting {InventoryType} inventory for subscription: {subscription.DisplayName}.", ex);
                }
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s finished at: {DateTime.Now}");
            return inventory;
        }

        public override async Task<IEnumerable<VirtualMachineModel>> ProcessInventoryAsync(IEnumerable<VirtualMachineModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var itemsToInsert = new List<VirtualMachineModel>();
            var itemsToUpdate = new List<VirtualMachineModel>();

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
                            var createdEvent = GetCreatedEvent(item.SubscriptionId, item.ResourceGroup, item.AzureId);
                            item.CreatedBy = createdEvent?.Caller;
                            item.CreatedDtUtc = createdEvent?.EventTimestamp;
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
                    Log.Error($"An exception occurred checking whether {InventoryType} already exists in the database with Azure id {item.AzureId}.", ex);
                }
            }

            var itemsToDelete = existingInventory.Where(x => !inventory.Any(y => y.AzureId.Equals(x.AzureId, StringComparison.OrdinalIgnoreCase)));

            var recordsDeleted = await DeleteInventoryAsync(itemsToDelete);
            var recordsUpdated = await UpdateInventoryAsync(itemsToUpdate);
            var recordsInserted = await InsertInventoryAsync(itemsToInsert);

            foreach (var vm in inventory) {
                foreach (var extension in vm.Extensions) {
                    extension.VirtualMachineId = vm.Id;
                }
            }

            Log.Debug($"  Number of {InventoryType} records inserted: {recordsInserted}");
            Log.Debug($"  Number of {InventoryType} records updated: {recordsUpdated}");
            Log.Debug($"  Number of {InventoryType} records deleted: {recordsDeleted}");

            Log.Debug($"Processing Azure inventory for {inventory.Count} {InventoryType}s finished at: {DateTime.Now}");
            return inventory;
        }

        private async Task<IEnumerable<VirtualMachineModel.VirtualMachineExtensionModel>> GetExtensionsForVm(IVirtualMachine vm)
        {
            var extensions = new List<VirtualMachineModel.VirtualMachineExtensionModel>();

            try {
                var azureExtensions = await vm.ListExtensionsAsync();
                foreach (var extension in azureExtensions) {
                    try {
                        var newExtension = new VirtualMachineModel.VirtualMachineExtensionModel();
                        newExtension.AzureId = extension.Id;
                        newExtension.Name = extension.Name;
                        newExtension.Publisher = extension.PublisherName;
                        newExtension.ImageName = extension.TypeName;
                        newExtension.ProvisioningState = extension.ProvisioningState;
                        newExtension.AutoUpgradeMinorVersion = extension.AutoUpgradeMinorVersionEnabled;
                        newExtension.Version = extension.VersionName;
                        newExtension.PublicSettings = extension.PublicSettingsAsJsonString;

                        extensions.Add(newExtension);
                    }
                    catch (Exception ex) {
                        Log.Error($"An exception occurred getting information about extension: {extension.Id}", ex);
                    }
                }

                return extensions;
            }
            catch (Exception ex) {
                Log.Error($"An exception occurred getting extensions for vm: {vm.Id}", ex);
                return new List<VirtualMachineModel.VirtualMachineExtensionModel>();
            }
        }
    }
}