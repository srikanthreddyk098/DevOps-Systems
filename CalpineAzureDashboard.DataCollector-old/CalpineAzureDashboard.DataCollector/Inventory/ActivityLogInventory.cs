using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.Models;
using Newtonsoft.Json;

namespace CalpineAzureDashboard.DataCollector.Inventory
{
    class ActivityLogInventory : AzureInventory<ActivityLogModel>
    {
        public ActivityLogInventory(AzureService azureService, IRepository<ActivityLogModel> repository) : base(azureService, repository, "activity log", null) { }

        public override async Task<IEnumerable<ActivityLogModel>> GetInventoryAsync(IEnumerable<ActivityLogModel> inventoryParam = null)
        {
            if (inventoryParam != null) {
                return inventoryParam;
            }

            Log.Debug($"Getting Azure inventory for {InventoryType}s started at: {DateTime.UtcNow}");
            var inventory = new List<ActivityLogModel>();

            foreach (var subscription in Subscriptions) {
                try {
                    DateTime? startDateTimeUtc = null;
                    DateTime? endDateTimeUtc = DateTime.UtcNow;
                    var query = "SELECT TOP 1 * FROM [ActivityLog] WHERE [Subscription] = @Subscription ORDER BY [EventTimestampUtc] DESC";

                    var latestActivityLogInDb = await Repository.Get(query, new {Subscription = subscription.DisplayName});
                    if (latestActivityLogInDb?.EventTimestampUtc != null) {
                        startDateTimeUtc = latestActivityLogInDb.EventTimestampUtc?.Add(new TimeSpan(0, 0, 1));
                    }

                    var resourceProviders = new[] {"Microsoft.Compute", "Microsoft.Storage"};

                    foreach (var resourceProvider in resourceProviders) {
                        var events =
                            await AzureService.GetActivityLogsByResourceProviderAsync(subscription.SubscriptionId, resourceProvider, startDateTimeUtc,
                                endDateTimeUtc);

                        foreach (var eventData in events) {
                            try {
                                var activityLog = new ActivityLogModel();
                                activityLog.AzureId = eventData.Id;
                                activityLog.Subscription = subscription.DisplayName;
                                activityLog.SubscriptionId = subscription.SubscriptionId;
                                activityLog.Action = eventData.Authorization?.Action;
                                activityLog.Caller = eventData.Caller;
                                activityLog.Category = eventData.Category?.LocalizedValue;
                                activityLog.ClientIpAddress = eventData.HttpRequest?.ClientIpAddress;
                                activityLog.ClientRequestId = eventData.HttpRequest?.ClientRequestId;
                                activityLog.CorrelationId = eventData.CorrelationId;
                                activityLog.Description = string.IsNullOrEmpty(eventData.Description) ? null : eventData.Description;
                                activityLog.Event = eventData.EventName?.LocalizedValue;
                                activityLog.EventDataId = eventData.EventDataId;
                                activityLog.EventTimestampUtc = eventData.EventTimestamp?.ToUniversalTime();
                                activityLog.HttpMethod = eventData.HttpRequest?.Method;
                                activityLog.Level = eventData.Level?.ToString();
                                activityLog.Operation = eventData.OperationName?.LocalizedValue;
                                activityLog.OperationId = eventData.OperationId;
                                activityLog.Properties = JsonConvert.SerializeObject(eventData.Inner?.Properties)
                                    ?.Replace("\"[", "[").Replace("]\"", "]")
                                    ?.Replace("\\\"", "\"");
                                if (activityLog.Properties == "null") {
                                    activityLog.Properties = null;
                                }

                                activityLog.ResourceGroup = eventData.ResourceGroupName;
                                activityLog.ResourceId = eventData.ResourceId;
                                activityLog.ResourceProvider = eventData.ResourceProviderName?.LocalizedValue;
                                activityLog.ResourceType = eventData.ResourceType?.LocalizedValue;
                                activityLog.Role = eventData.Authorization?.Role;
                                activityLog.Scope = eventData.Authorization?.Scope;
                                activityLog.Status = eventData.Status?.LocalizedValue;
                                activityLog.SubStatus = string.IsNullOrEmpty(eventData.SubStatus?.LocalizedValue) ? null : eventData.SubStatus?.LocalizedValue;
                                activityLog.SubmissionTimeStampUtc = eventData.SubmissionTimestamp?.ToUniversalTime();
                                activityLog.Uri = eventData.HttpRequest?.Uri;

                                inventory.Add(activityLog);
                            }
                            catch (Exception ex) {
                                Log.Error($"An exception occurred getting details for {InventoryType} with Azure id: {eventData.Id}", ex);
                            }
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

        public override async Task<IEnumerable<ActivityLogModel>> ProcessInventoryAsync(
            IEnumerable<ActivityLogModel> inventoryParam)
        {
            var inventory = inventoryParam.ToList();
            if (inventory.Count == 0) {
                return inventory;
            }

            var itemsToInsert = inventory;
            var recordsInserted = await InsertInventoryAsync(itemsToInsert);

            Log.Debug($"  Number of {InventoryType} records inserted: {recordsInserted}");

            Log.Debug($"Processing Azure inventory for {inventory.Count} {InventoryType}s finished at: {DateTime.Now}");
            return inventory;
        }
    }
}