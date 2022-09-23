using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CAD.DataCollector.Azure.Models;
using CAD.DataCollector.Azure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace CAD.DataCollector.Azure
{
    public class AzureResourceDataCollector : BackgroundService
    {
        private readonly ILogger<AzureResourceDataCollector> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly HttpClient _httpClient;
        private readonly AzureResourceSkuRepository _azureResourceSkuRepository;
        private Dictionary<int, DateTime> _lastResourceRunTime;
        private readonly object _lastResourceRunTimeLock;

        public AzureResourceDataCollector(ILogger<AzureResourceDataCollector> logger, IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = clientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromHours(1);

            _connectionString = configuration["connectionString"];
            _azureResourceSkuRepository = new AzureResourceSkuRepository(_connectionString);
            _lastResourceRunTime = new Dictionary<int, DateTime>();
            _lastResourceRunTimeLock = new object();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try {
                while (!cancellationToken.IsCancellationRequested) {
                    var startTime = DateTime.Now;
                    _logger.LogInformation($"Starting AzureDataCollector run at: {startTime}");

                    var apiConfigurationRepository = new ApiConfigurationRepository(_connectionString);
                    var apiConfigurations = await apiConfigurationRepository.GetAllAsync();

                    var tenantGroups = apiConfigurations.GroupBy(x => x.TenantId);
                    foreach (var tenantGroup in tenantGroups) {
                        await ProcessTenantGroup(tenantGroup, cancellationToken);
                    }

                    var endTime = DateTime.Now;
                    double elapsedTimeInMinutes = Math.Round((endTime - startTime).TotalMinutes, 2);

                    int minutesUntilNextHour = 60 - endTime.Minute;
                    _logger.LogInformation(
                        $"AzureDataCollector run completed in {elapsedTimeInMinutes} minutes. Sleeping for {minutesUntilNextHour} minutes.");
                    await Task.Delay(minutesUntilNextHour * 60000, cancellationToken);
                }
            }
            catch (Exception ex) {
                _logger.LogCritical(ex, "An unexpected exception occurred. Exiting main thread.");
            }
        }

        private async Task ProcessTenantGroup(IGrouping<string, ApiConfigurationModel> tenantGroup, CancellationToken cancellationToken)
        {
            var configurationsByTenant = tenantGroup.GroupBy(x => new {x.ResourceUrl, x.TenantId, x.ClientId, x.ClientSecret});
            var tasks = new List<Task>();

            foreach (var configuration in configurationsByTenant) {
                tasks.Add(Task.Run(async () =>
                {
                    //can put each group in it's own thread
                    string tenantId = configuration.Key.TenantId;
                    string resourceUrl = configuration.Key.ResourceUrl;
                    string clientId = configuration.Key.ClientId;
                    string clientSecret = configuration.Key.ClientSecret;
                    await ProcessTenantAuthenticationGroup(tenantId, resourceUrl, _configuration[clientId], _configuration[clientSecret],
                        configuration,
                        cancellationToken);
                }, cancellationToken));
            }

            await Task.WhenAll(tasks);
        }

        private async Task ProcessTenantAuthenticationGroup(string tenantId, string resourceUrl, string clientId, string clientSecret,
            IEnumerable<ApiConfigurationModel> configurationsByTenant, CancellationToken cancellationToken)
        {
            var azureService = new AzureApiService(tenantId, clientId, clientSecret, resourceUrl, _httpClient, cancellationToken);
            var configurationsByProcessingOrder = configurationsByTenant.GroupBy(x => x.ProcessingOrder).OrderBy(x => x.Key).ToList();

            foreach (var configurationGroups in configurationsByProcessingOrder) {
                var tasks = new List<Task<Tuple<int, DateTime>>>();
                var configurationsByResourceTypeAndInsertOnly = configurationGroups.GroupBy(x => new {x.ResourceType, x.InsertOnly}).ToList();
                foreach (var configurationByResourceTypeAndInsertOnly in configurationsByResourceTypeAndInsertOnly) {
                    try {
                        var resourceType = configurationByResourceTypeAndInsertOnly.Key.ResourceType;
                        var insertOnly = configurationByResourceTypeAndInsertOnly.Key.InsertOnly;

                        var apiConfigurationRepository = new ApiConfigurationRepository(_connectionString);
                        var apiConfigurations =
                            (await apiConfigurationRepository.GetAllAsync(new
                                {TenantId = tenantId, ResourceType = resourceType, InsertOnly = insertOnly})).ToList();
                        var apiConfigurationId = configurationByResourceTypeAndInsertOnly.First().Id;
                        var frequency = configurationByResourceTypeAndInsertOnly.First().Frequency;

                        if (!ShouldRun(apiConfigurationId, frequency)) {
                            _logger.LogDebug($"Skipping {resourceType} because it already ran this period.");
                            continue;
                        }

                        if (string.Equals(resourceType, "ResourceSku", StringComparison.OrdinalIgnoreCase)) {

                            //tasks.Add(Task.Run(async () =>
                            await Task.Run(async () =>
                            {
                                await ProcessResourceSkuGroup(azureService, apiConfigurations, cancellationToken);
                                return new Tuple<int, DateTime>(apiConfigurations.First().Id, DateTime.Now);
                            }, cancellationToken);
                        }
                        else {
                            //tasks.Add(Task.Run(async () =>
                            await Task.Run(async () =>
                            {
                                await ProcessResourceTypeGroup(azureService, apiConfigurations, cancellationToken);
                                return new Tuple<int, DateTime>(apiConfigurations.First().Id, DateTime.Now);
                            }, cancellationToken);
                        }
                    }
                    catch (Exception ex) {
                        _logger.LogError(ex, $"An exception occurred processing resource type {configurationsByResourceTypeAndInsertOnly.First()?.Key?.ResourceType}:");
                    }
                }

                try {
                    var taskResults = await Task.WhenAll(tasks);

                    foreach (var result in taskResults) {
                        lock (_lastResourceRunTimeLock) {
                            if (_lastResourceRunTime.ContainsKey(result.Item1)) {
                                _lastResourceRunTime[result.Item1] = result.Item2;
                            }
                            else {
                                _lastResourceRunTime.Add(result.Item1, result.Item2);
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    _logger.LogError(ex, $"An exception occurred processing configuration groups:");
                }
            }
        }

        private bool ShouldRun(int apiConfigurationId, string frequency)
        {
            lock (_lastResourceRunTimeLock) {
                if (!_lastResourceRunTime.ContainsKey(apiConfigurationId)) return true;
                switch (frequency) {
                    case "Hourly":
                    {
                        return _lastResourceRunTime[apiConfigurationId].Date < DateTime.Now.Date ||
                               _lastResourceRunTime[apiConfigurationId].Hour < DateTime.Now.Hour;
                    }

                    case "Daily":
                    {
                        return _lastResourceRunTime[apiConfigurationId].Date < DateTime.Now.Date;
                    }

                    default:
                    {
                        _logger.LogError($"Unsupported frequency, {frequency}, for ApiConfiguration id {apiConfigurationId}.");
                        return false;
                    }
                }
            }
        }

        private async Task ProcessResourceTypeGroup(AzureApiService azureService, IEnumerable<ApiConfigurationModel> configurations,
            CancellationToken cancellationToken)
        {
            var configurationList = configurations.ToList();
            var tenantId = configurationList.First().TenantId;
            var resourceType = configurationList.First().ResourceType;
            var insertOnly = configurationList.First().InsertOnly;

            var startTime = DateTime.Now;
            _logger.LogInformation($"Processing for {resourceType} started at: {startTime}");

            var resourcesToAdd = new List<AzureResourceModel>();
            var resourcesToAddLock = new object();
            var resourcesToUpdate = new List<AzureResourceModel>();
            var resourcesToUpdateLock = new object();
            var resourcesThatStillExist = new List<AzureResourceModel>();
            var resourcesThatStillExistLock = new object();

            var azureResourceRepository = new AzureResourceRepository(_connectionString);
            var existingResources =
                insertOnly ? null : (await azureResourceRepository.GetAllAsync(new {TenantId = tenantId, ResourceType = resourceType})).ToList();

            var tasks = new List<Task>();

            foreach (var configuration in configurationList) {
                tasks.Add(Task.Run(async () =>
                {
                    try {
                        if (string.IsNullOrEmpty(configuration.Url)) {
                            throw new Exception($"Url for {configuration.ResourceType} is empty or null!");
                        }

                        var results = await azureService.GetDataAsync(configuration.HttpMethod, configuration.Url, configuration.JsonResultArrayName, configuration.PostBody);

                        foreach (string result in results) {
                            if (cancellationToken.IsCancellationRequested) {
                                return;
                            }

                            var json = JObject.Parse(result);
                            var subscriptionId = Regex.Match(configuration.Url, @"(?<=subscriptions\/).*?(?=\/)").Value;

                            string azureId = configuration.AzureIdCustomPrefix ?? "";
                            azureId += string.IsNullOrEmpty(configuration.AzureIdProperty)
                                ? $"/subscriptions/{subscriptionId}/resourceType/{json["resourceType"]}/name/{json[configuration.NameProperty]}"
                                : json[configuration.AzureIdProperty].ToString();

                            var resourceGroup = Regex.Match(azureId, @"(?<=resourceGroups\/).*?(?=\/|$)").Value;

                            var resource = new AzureResourceModel {
                                ResourceType = configuration.ResourceType,
                                TenantId = configuration.TenantId,
                                SubscriptionId = string.IsNullOrEmpty(subscriptionId) ? null : subscriptionId,
                                ResourceGroup = string.IsNullOrEmpty(resourceGroup) ? null : resourceGroup,
                                AzureId = azureId,
                                Name = string.IsNullOrEmpty(configuration.NameProperty)
                                    ? null
                                    : GetResourceNameValue(json, configuration.NameProperty.Split(".")),
                                ParentAzureId = string.IsNullOrEmpty(configuration.ParentAzureId) ? null : configuration.ParentAzureId,
                                Data = result
                            };

                            if (configuration.InsertOnly) {
                                lock (resourcesToAddLock) {
                                    resourcesToAdd.Add(resource);
                                }

                                continue;
                            }

                            var existingResource = existingResources?.FirstOrDefault(x => x.AzureId.Equals(resource.AzureId));

                            if (existingResource == null) {
                                lock (resourcesToAddLock) {
                                    resourcesToAdd.Add(resource);
                                }

                                continue;
                            }

                            lock (resourcesThatStillExistLock) {
                                resourcesThatStillExist.Add(existingResource);
                            }

                            if (!resource.IsEquals(existingResource)) {
                                lock (resourcesToUpdateLock) {
                                    resource.Id = existingResource.Id;
                                    resourcesToUpdate.Add(resource);
                                }
                            }
                        }
                    }
                    catch (Exception ex) {
                        throw new Exception($"An exception occurred calling API with url: {configuration.Url}", ex);
                    }
                }, cancellationToken));
            }

            await Task.WhenAll(tasks);

            var resourcesToDelete = existingResources?.Except(resourcesThatStillExist).ToList() ?? new List<AzureResourceModel>();

            var repository = new AzureResourceRepository(_connectionString, configurationList.FirstOrDefault()?.CustomTableName);

            try {
                int recordsDeleted = await repository.DeleteAsync(resourcesToDelete);
                int recordsInserted = await repository.InsertAsync(resourcesToAdd) ? resourcesToAdd.Count : 0;
                int recordsUpdated = await repository.UpdateAsync(resourcesToUpdate);

                _logger.LogInformation($"  Number of {resourceType} records inserted: {recordsInserted}");
                _logger.LogInformation($"  Number of {resourceType} records updated: {recordsUpdated}");
                _logger.LogInformation($"  Number of {resourceType} records deleted: {recordsDeleted}");

                string postProcessingQuery = configurationList.FirstOrDefault()?.PostProcessingQuery;
                if (!string.IsNullOrEmpty(postProcessingQuery)) {
                    await _azureResourceSkuRepository.ExecuteAsync(postProcessingQuery, commandTimeout: 600);
                    _logger.LogInformation($"  Post processing query for {resourceType} completed.");
                }
            }
            catch (Exception ex) {
                _logger.LogError(ex, $"An exception occurred updating database for {resourceType}:");
            }

            var endTime = DateTime.Now;
            var elapsedTime = (endTime - startTime).TotalMinutes;
            _logger.LogInformation($"Processing for {resourceType} finished at: {endTime} and took {Math.Round(elapsedTime, 2)} minutes.");
        }

        private string GetResourceNameValue(JToken json, string[] propertyStrings, int count = 0)
        {
            if (count >= propertyStrings.Length) {
                return json.ToString();
            }

            return GetResourceNameValue(json[propertyStrings[count]], propertyStrings, ++count);
        }

        private async Task ProcessResourceSkuGroup(AzureApiService azureService, IEnumerable<ApiConfigurationModel> configurations,
            CancellationToken cancellationToken)
        {
            var configurationList = configurations.ToList();
            var resourceType = configurationList.First().ResourceType;

            var startTime = DateTime.Now;
            _logger.LogInformation($"Processing for {resourceType} started at: {startTime}");

            var resourceSkusToAdd = new List<AzureResourceSkuModel>();
            //var tasks = new List<Task>();

            foreach (ApiConfigurationModel configuration in configurationList) {
                if (cancellationToken.IsCancellationRequested) {
                    return;
                }

                //tasks.Add(Task.Run(async () =>
                //{
                try {
                    if (string.IsNullOrEmpty(configuration.Url)) throw new Exception("Url is empty!");

                    var results = await azureService.GetDataAsync(configuration.HttpMethod, configuration.Url, configuration.JsonResultArrayName, configuration.PostBody);

                    foreach (string result in results) {
                        if (cancellationToken.IsCancellationRequested) {
                            return;
                        }

                        var json = JObject.Parse(result);
                        var subscriptionId = Regex.Match(configuration.Url, @"(?<=subscriptions\/).*?(?=\/)").Value;

                        JArray locationJArray = (JArray)json["locationInfo"];
                        string location = null;
                        if (locationJArray.HasValues) {
                            location = locationJArray[0]["location"].ToString();
                        }
                        else {
                            locationJArray = (JArray)json["locations"];
                            location = locationJArray[0].ToString();
                        }

                        string azureId = "/subscriptions/" + Regex.Match(configuration.Url, @"(?<=subscriptions\/).*?(?=\/)") +
                                         "/resourceType/" + json["resourceType"] +
                                         "/name/" + json[configuration.NameProperty] +
                                         "/location/" + location;

                        if (json["size"] != null) {
                            azureId += "/size/" + json["size"];
                        }

                        var resourceSku = new AzureResourceSkuModel {
                            TenantId = configuration.TenantId,
                            SubscriptionId = subscriptionId,
                            ResourceType = json["resourceType"].ToString(),
                            Name = json[configuration.NameProperty].ToString(),
                            AzureId = azureId,
                            Location = json["locations"][0].ToString(),
                            Data = result
                        };

                        resourceSkusToAdd.Add(resourceSku);
                    }
                }
                catch (Exception ex) {
                    if (ex.Message.Contains(
                        "The current subscription type is not permitted to perform operations on any provider namespace. Please use a different subscription.")
                    ) {
                        return;
                    }

                    throw new Exception($"An exception occurred calling API with url: {configuration.Url}", ex);
                }

                //}, cancellationToken));
            }

            //await Task.WhenAll(tasks);

            try {
                int recordsInserted = await _azureResourceSkuRepository.InsertAsync(resourceSkusToAdd, 600) ? resourceSkusToAdd.Count : 0;
                _logger.LogInformation($"  Number of {resourceType} records inserted: {recordsInserted}");

                var postProcessingQuery = configurationList.FirstOrDefault()?.PostProcessingQuery;
                if (!string.IsNullOrEmpty(postProcessingQuery)) {
                    await _azureResourceSkuRepository.ExecuteAsync(postProcessingQuery, commandTimeout: 600);
                    _logger.LogInformation($"  Post processing query for {resourceType} completed.");
                }
            }
            catch (Exception ex) {
                _logger.LogError(ex, $"An exception occurred updating database for {resourceType}:");
            }

            var endTime = DateTime.Now;
            var elapsedTime = (endTime - startTime).TotalMinutes;
            _logger.LogInformation($"Processing for {resourceType} finished at: {endTime} and took {Math.Round(elapsedTime, 2)} minutes.");
        }
    }
}
