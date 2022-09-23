using System;
using System.Collections.Generic;
using System.Linq;
using CalpineAzureDashboard.Azure;
using CalpineAzureDashboard.Data.Repository;
using CalpineAzureDashboard.DataCollector.Base;
using CalpineAzureDashboard.Models;
using Microsoft.Azure.Management.Monitor.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;

namespace CalpineAzureDashboard.DataCollector.Inventory
{
    public abstract class AzureInventory<T> : BaseInventory<T> where T : AzureModel
    {
        protected readonly AzureService AzureService;
        protected readonly IEnumerable<ISubscription> Subscriptions;
        private readonly string _createEventOperationName;

        protected AzureInventory(AzureService azureService, IRepository<T> repository, string inventoryType, string createEventOperationName) : base(repository, inventoryType)
        {
            AzureService = azureService;
            _createEventOperationName = createEventOperationName;

            Subscriptions = AzureService.GetAllSubscriptionsAsync().Result
                .Where(x => x.State.Equals("Enabled") && !x.DisplayName.StartsWith("Visual Studio") && !x.DisplayName.StartsWith("Access to") &&
                            !x.DisplayName.StartsWith("Free Trial") && !x.DisplayName.StartsWith("BillsMSDN") && !x.DisplayName.StartsWith("Nicholie") &&
                            !x.DisplayName.StartsWith("VS Team") && !x.DisplayName.StartsWith("Calpine DMZ"));
        }

        public IEventData GetCreatedEvent(string subscriptionId, string resourceGroup, string azureId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(_createEventOperationName)) {
                throw new Exception("_createEventOperationName value cannot be null or empty. The value must be set in the constructor to use this method.");
            }

            var activityLogs = AzureService.GetActivityLogsByResourceId(subscriptionId, resourceGroup, azureId, startDate, endDate);

            return activityLogs
                   .Where(x => !string.IsNullOrEmpty(x.OperationName?.LocalizedValue) &&
                               x.OperationName.LocalizedValue.StartsWith(_createEventOperationName, StringComparison.OrdinalIgnoreCase)).OrderBy(x => x.EventTimestamp)
                   .FirstOrDefault();
        }
    }
}