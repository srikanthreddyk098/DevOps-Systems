using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;

namespace CAD.DataCollector.Azure
{
    public class AzureApiService
    {
        private readonly string _tenantId;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _resourceUrl;
        private readonly HttpClient _httpClient;
        private readonly CancellationToken _cancellationToken;

        public AzureApiService(string tenantId, string clientId, string clientSecret, string resourceUrl, HttpClient httpClient, CancellationToken? cancellationToken = null)
        {
            _tenantId = tenantId;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _resourceUrl = resourceUrl;
            _httpClient = httpClient;

            _cancellationToken = cancellationToken ?? CancellationToken.None;
        }

        public async Task<IEnumerable<string>> GetDataAsync(string httpMethod, string url, string jsonResultArrayName, string postBody)
        {
            return await GetDataAsync(httpMethod, url, jsonResultArrayName, postBody, null);
        }

        private async Task<IEnumerable<string>> GetDataAsync(string httpMethod, string url, string jsonResultArrayName, string postBody, string authenticationToken, int attemptNumber = 1)
        {
            if (string.IsNullOrEmpty(authenticationToken)) {
                authenticationToken = await GetAuthTokenForApplicationAsync();
            }

            var results = new List<string>();

            using var request = new HttpRequestMessage(GetHttpMethod(httpMethod), url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authenticationToken);
            if (postBody != null) request.Content = new StringContent(postBody, System.Text.Encoding.UTF8, "application/json");

            try {
                HttpResponseMessage response = await _httpClient.SendAsync(request, _cancellationToken);

                if (!response.IsSuccessStatusCode) {
                    string errorMessage = $"An error occurred! The call to {url} returned response code {response.StatusCode}";
                    errorMessage += $"Content: {await response.Content.ReadAsStringAsync()}";
                    if (errorMessage.Contains(
                        "The current subscription type is not permitted to perform operations on any provider namespace. Please use a different subscription.")
                    ) {
                        return results;
                    }

                    if (attemptNumber <= 3) {
                        return await GetDataAsync(httpMethod, url, jsonResultArrayName, postBody, authenticationToken, ++attemptNumber);
                    }

                    throw new HttpRequestException(errorMessage);
                }

                string responseContent = await response.Content.ReadAsStringAsync();
                var jObject = JObject.Parse(responseContent);

                var values = jObject[jsonResultArrayName]?.Children().ToList();
                if (values != null && values.Any()) {
                    results.AddRange(values.Select(x => x.ToString()));

                    var nextLink = jObject["nextLink"]?.ToString() ?? jObject["@odata.nextLink"]?.ToString();
                    if (!string.IsNullOrEmpty(nextLink)) {
                        results.AddRange(await GetDataAsync(httpMethod, nextLink, jsonResultArrayName, postBody, authenticationToken));
                    }
                    //Resource Graph Explorer
                    else {
                        var skipToken = jObject["$skipToken"]?.ToString();
                        if (!string.IsNullOrEmpty(skipToken)) {
                            JObject postBodyJson = JObject.Parse(postBody);
                            postBodyJson.Remove("options");
                            postBodyJson.Add("options", new JObject { { "$skipToken", skipToken } });
                            results.AddRange(await GetDataAsync(httpMethod, url, jsonResultArrayName, postBodyJson.ToString(), authenticationToken));
                        }                        
                    }
                }
            }
            catch (Exception ex) {
                if (attemptNumber <= 3) {
                    return await GetDataAsync(httpMethod, url, jsonResultArrayName, postBody, authenticationToken, ++attemptNumber);
                }

                throw;
            }

            return results;
        }

        private HttpMethod GetHttpMethod(string httpMethod)
        {
            switch (httpMethod.ToLower()) {
                case "get":
                {
                    return HttpMethod.Get;
                }

                case "post":
                {
                    return HttpMethod.Post;
                }

                case "put":
                {
                    return HttpMethod.Put;
                }

                case "head":
                {
                    return HttpMethod.Head;
                }

                case "patch":
                {
                    return HttpMethod.Patch;
                }

                case "options":
                {
                    return HttpMethod.Options;
                }

                case "delete":
                {
                    return HttpMethod.Delete;
                }

                default:
                {
                    throw new Exception($"Unsupported http method found: {httpMethod}");
                }
            }
        }

        private async Task<string> GetAuthTokenForApplicationAsync()
        {
            var authenticationContext = new AuthenticationContext($"https://login.microsoftonline.com/{_tenantId}");
            var clientCred = new ClientCredential(_clientId, _clientSecret);
            return (await authenticationContext.AcquireTokenAsync(_resourceUrl, clientCred)).AccessToken;
        }
    }
}