using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AzureAutomation.Models;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Graph.RBAC.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AzureAutomation.Azure
{
    public class AzureService
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _tenantId;
        private readonly HttpClient _httpClient;

        public AzureService(string clientId, string clientSecret, string tenantId)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _tenantId = tenantId;

            _httpClient = new HttpClient();
        }

        public async Task<IPagedCollection<ISubscription>> GetAllSubscriptionsAsync()
        {
            return await Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).Subscriptions.ListAsync();
        }

        public async Task<IPagedCollection<IVirtualMachine>> GetVirtualMachinesInSubscriptionAsync(string subscriptionId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.VirtualMachines.ListAsync();
        }

        public async Task<IVirtualMachine> GetVirtualMachineByIdAsync(string subscriptionId, string vmId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.VirtualMachines.GetByIdAsync(vmId);
        }

        public IVirtualMachine GetVirtualMachineById(string subscriptionId, string vmId)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return azure.VirtualMachines.GetById(vmId);
        }

        public async Task<IVirtualMachine> GetVirtualMachineByResourceGroupAsync(string subscriptionId, string resourceGroup, string vmName)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return await azure.VirtualMachines.GetByResourceGroupAsync(resourceGroup, vmName);
        }

        public PowerState GetVmStatus(IVirtualMachine azureVm)
        {
            return azureVm.PowerState;
        }

        public async Task StartVm(IVirtualMachine azureVm)
        {
            await azureVm.StartAsync();
        }

        public async Task StopVm(IVirtualMachine azureVm)
        {
            await azureVm.DeallocateAsync();
        }

        public ScheduleTagsModel GetScheduleTags(IReadOnlyDictionary<string, string> tags)
        {
            var scheduleTags = new ScheduleTagsModel();

            if (tags.TryGetValue("Shutdown_Schedule", out var powerSchedule)) {
                var scheduleJson = JsonSerializer.Deserialize<ScheduleJsonModel>(powerSchedule);

                scheduleTags.Days = scheduleJson.Days;

                if (int.TryParse(scheduleJson.On.Split(':')[0], out var scheduledStartTimeInt)) {
                    scheduleTags.StartTime =
                        new DateTime(TimeSpan.FromHours(scheduledStartTimeInt).Ticks).ToString("h tt");
                }

                if (int.TryParse(scheduleJson.Off.Split(':')[0], out var scheduledStopTimeInt)) {
                    scheduleTags.StopTime =
                        new DateTime(TimeSpan.FromHours(scheduledStopTimeInt).Ticks).ToString("h tt");
                }
            }

            return scheduleTags;
        }

        private AzureCredentials GetAzureCredentials()
        {
            return SdkContext.AzureCredentialsFactory.FromServicePrincipal(_clientId, _clientSecret, _tenantId, AzureEnvironment.AzureGlobalCloud);
        }

        public async Task<IPagedCollection<IActiveDirectoryUser>> GetAzureActiveDirectoryUsersAsync()
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials());
            return await azure.ActiveDirectoryUsers.ListAsync();
        }

        public async Task<IPagedCollection<IActiveDirectoryGroup>> GetAzureActiveDirectoryGroupsAsync()
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials());
            return await azure.ActiveDirectoryGroups.ListAsync();
        }

        public async Task<string> UpdateVmTagAsync(string resourceId, IReadOnlyDictionary<string, string> tags, string location)
        {
            string url = $"https://management.azure.com{resourceId}?api-version=2019-07-01";
            string authenticationToken = await GetOAuthTokenForApplicationAsync();

            var jsonObject = new JObject {["location"] = location, ["tags"] = JObject.FromObject(tags)};

            using var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authenticationToken);
            request.Content = new StringContent(jsonObject.ToString(), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode) {
                string errorMsg = $"An error occurred! The call to {url} returned: [{response.StatusCode}]{Environment.NewLine}{await response.Content.ReadAsStringAsync()}{Environment.NewLine}{JsonConvert.SerializeObject(response)}";
                throw new Exception(errorMsg);
            }

            string data = await response.Content.ReadAsStringAsync();
            var deserializedData = JObject.Parse(data);
            return deserializedData["tags"].ToString();
        }

        private async Task<string> GetOAuthTokenForApplicationAsync()
        {
            var authenticationContext = new AuthenticationContext($"https://login.microsoftonline.com/{_tenantId}");
            var clientCred = new ClientCredential(_clientId, _clientSecret);
            return (await authenticationContext.AcquireTokenAsync("https://management.azure.com/", clientCred)).AccessToken;
        }
    }
}
