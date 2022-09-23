using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CalpineAzureMonitor.Data.Models;
using CalpineAzureMonitor.Data.Repositories;
using log4net;

namespace CalpineAzureMonitor
{
    class UrlMonitor
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UrlMonitor));

        private readonly UrlRepository _urlRepository;
        private readonly UrlAlertRepository _urlAlertRepository;
        private readonly EmailNotifier _emailNotifier;
        private readonly string _teamsAlertUri;

        private readonly CancellationToken _cancellationToken;
        private readonly List<int> _frequenciesAlreadyRunning;
        private readonly object _lock;

        private HttpClient _httpClient;

        internal UrlMonitor(UrlRepository urlRepository, UrlAlertRepository urlAlertRepository, EmailNotifier emailNotifier, string teamsAlertUri,
            CancellationToken cancellationToken)
        {
            _urlRepository = urlRepository;
            _urlAlertRepository = urlAlertRepository;
            _emailNotifier = emailNotifier;
            _teamsAlertUri = teamsAlertUri;

            _cancellationToken = cancellationToken;
            _frequenciesAlreadyRunning = new List<int>();
            _lock = new object();
        }

        public async Task StartMonitoringAsync()
        {
            using (_httpClient = new HttpClient()) {
                while (true) {
                    if (_cancellationToken.IsCancellationRequested) {
                        Log.Error($"Cancellation request was found in main loop. Returning...");
                        return;
                    }

                    try {
                        await _urlRepository.UpdateUrlMapping();
                    }
                    catch (Exception ex) {
                        Log.Error("An exception occurred running the stored procedure sp_UpdateUrlMapping.", ex);
                    }

                    var frequenciesInMin = new List<int>();

                    try {
                        Log.Debug("Getting list of frequencies...");
                        frequenciesInMin = (await _urlRepository.GetFrequenciesAsync()).ToList();
                        Log.Debug($"Found {frequenciesInMin.Count} frequencies...");
                    }
                    catch (Exception ex) {
                        Log.Error("An exception occurred getting the list of frequencies.", ex);
                    }

                    foreach (var frequencyInMin in frequenciesInMin) {
                        lock (_lock) {
                            if (_frequenciesAlreadyRunning.Any(x => x.Equals(frequencyInMin))) {
                                Log.Debug($"A task to handle {frequencyInMin} min frequency is already running. Skipping...");
                                continue;
                            }
                        }

                        var task = Task.Factory.StartNew(async () =>
                        {
                            while (true) {
                                if (_cancellationToken.IsCancellationRequested) {
                                    Log.Debug($"Cancellation request was found in {frequencyInMin} min loop. Returning...");
                                    return;
                                }

                                var urlsToTest = new List<UrlModel>();

                                Log.Debug($"Getting urls that needs to be checked every {frequencyInMin} minutes...");
                                try {
                                    urlsToTest = (await _urlRepository.GetUrlsForFrequencyAsync(frequencyInMin)).ToList();
                                }
                                catch (Exception ex) {
                                    Log.Error($"An exception occurred getting the list urls to check for frequency: {frequencyInMin}.", ex);
                                }

                                if (urlsToTest.Count < 1) {
                                    lock (_lock) {
                                        _frequenciesAlreadyRunning.Remove(frequencyInMin);
                                    }

                                    return;
                                }

                                foreach (var urlToTest in urlsToTest) {
                                    var serviceTask = Task.Factory.StartNew(async () =>
                                    {
                                        if (_cancellationToken.IsCancellationRequested) {
                                            return;
                                        }

                                        await CheckUrl(urlToTest);
                                    }, _cancellationToken);
                                }

                                try {
                                    await Task.Delay(frequencyInMin * 60000, _cancellationToken);
                                }
                                catch (TaskCanceledException) {
                                    Log.Debug("Task.Delay threw a task cancelled exception.");
                                }
                            }
                        }, _cancellationToken);
                        lock (_lock) {
                            _frequenciesAlreadyRunning.Add(frequencyInMin);
                        }
                    }

                    try {
                        await Task.Delay(300000, _cancellationToken);
                    }
                    catch (TaskCanceledException) {
                        Log.Debug("Task.Delay threw a task cancelled exception.");
                    }
                }
            }
        }

        private async Task CheckUrl(UrlModel urlToTest)
        {
            try {
                var response = await GetUrlStatus(urlToTest.Url, urlToTest.ExpectedResponseCode);
                var responseCode = (int) response.StatusCode;
                if (responseCode.ToString().Equals(urlToTest.ExpectedResponseCode)) {
                    Log.Debug($"Url check for {urlToTest.Url} returned expected response code {response.StatusCode}.");

                    if (!string.IsNullOrEmpty(urlToTest.ExpectedResponseBody)) {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        if (!responseBody.Equals(urlToTest.ExpectedResponseBody)) {
                            Log.Error(
                                $"Url check for {urlToTest.Url} returned expected response code {response.StatusCode} " +
                                $"but the body of the response did not match:{Environment.NewLine}" +
                                $"Expected:{Environment.NewLine}" +
                                $"{urlToTest.ExpectedResponseBody}{Environment.NewLine}" +
                                $"Received:{Environment.NewLine}" +
                                $"{responseBody}");
                            string subject = $"Calpine Azure Monitor Alert - URL check failed for {urlToTest.Application}";
                            string body =
                                $"Last url check for {urlToTest.Url} returned expected response code {response.StatusCode} " +
                                $"but the body of the response did not match:<br><br>" +
                                $"Expected:<br>" +
                                $"{urlToTest.ExpectedResponseBody}<br><br>" +
                                $"Received:<br>" +
                                $"{responseBody}";
                            await SendNotificationsAsync(urlToTest, response.StatusCode.ToString(), subject, body);
                        }
                    }
                }
                else {
                    Log.Error($"Last url check for {urlToTest.Url} returned response code {response.StatusCode}. Expected {urlToTest.ExpectedResponseCode}");
                    string subject = $"Calpine Azure Monitor Alert - URL check failed for {urlToTest.Application}";
                    string body = $"Url check for {urlToTest.Url} returned response code {response.StatusCode}.<br>Expected {urlToTest.ExpectedResponseCode}";
                    await SendNotificationsAsync(urlToTest, response.StatusCode.ToString(), subject, body);
                }
            }
            catch (Exception ex) {
                Log.Error($"An exception occurred trying to check url: {urlToTest.Url} for application {urlToTest.Application}:", ex);
                string subject = $"Calpine Azure Monitor Alert - URL check for {urlToTest.Application} threw an exception";
                string body = $"Url check for {urlToTest.Url} threw an exception: {ex.Message}.";
                body += $"<br><br>{ex}";
                SendNotificationsAsync(urlToTest, "Exception", subject, body).GetAwaiter().GetResult();
            }
        }

        private async Task<HttpResponseMessage> GetUrlStatus(string url, string expectedResponseCode)
        {
            
            HttpResponseMessage response = await GetHttpResponseMessageAsync(url);
            var statusCode = (int) response.StatusCode;
            if (statusCode.ToString().Equals(expectedResponseCode)) return response;

            Log.Debug($"First url check for {url} returned response code {response.StatusCode}. Expected {expectedResponseCode}. Trying again in 5 seconds...");
            Task.Delay(5000, _cancellationToken).GetAwaiter().GetResult();
            response = await GetHttpResponseMessageAsync(url);
            statusCode = (int)response.StatusCode;
            if (statusCode.ToString().Equals(expectedResponseCode)) return response;

            Log.Debug(
                $"Second url check for {url} returned response code {response.StatusCode}. Expected {expectedResponseCode}. Trying again in 30 seconds...");
            Task.Delay(30000, _cancellationToken).GetAwaiter().GetResult();
            response = await GetHttpResponseMessageAsync(url);

            return response;
        }

        private async Task<HttpResponseMessage> GetHttpResponseMessageAsync(string url, int numberOfTries = 3)
        {
            int attemptNumber = 1;
            try {
                return await _httpClient.GetAsync(url, _cancellationToken);
            }
            catch (Exception ex) {
                if (attemptNumber < numberOfTries) {
                    return await GetHttpResponseMessageAsync(url, --numberOfTries);
                }
                else {
                    throw new Exception("Error: Unable to get a response from the http client. See inner exception for more details.", ex);
                }
            }
        }

        private async Task SendNotificationsAsync(UrlModel urlToTest, string responseCode, string subject, string body)
        {
            var task1 = Task.Run(() => { SendNotificationEmail(urlToTest, subject, body); }, _cancellationToken);
            var task2 = Task.Run(async () =>
            {
                var result = WriteNotificationToDatabaseAsync(urlToTest, responseCode, subject, body).GetAwaiter().GetResult();
            }, _cancellationToken);
            var task3 = Task.Run(async () =>
            {
                var result = SendTeamsMessageAsync(urlToTest, subject, body).GetAwaiter().GetResult();
                if (result == null || !result.IsSuccessStatusCode) {
                    Log.Error($"Teams message http request finished with status code: {result?.StatusCode}");
                }
            }, _cancellationToken);

            await Task.WhenAll(task1, task2, task3);
        }

        private void SendNotificationEmail(UrlModel urlToTest, string subject, string body)
        {
            Log.Debug($"Sending email to '{urlToTest.Email}' with subject '{subject}' and body '{body}'.");
            _emailNotifier.SendEmail(urlToTest.Email, subject, body);
        }

        private async Task<int?> WriteNotificationToDatabaseAsync(UrlModel urlToTest, string responseCode, string subject, string body)
        {
            var urlAlert = new UrlAlertModel {
                Url = urlToTest.Url,
                ResponseCode = responseCode,
                Email = urlToTest.Email,
                Subject = subject,
                Body = body.Replace("<br>", "")
            };

            try {
                return await _urlAlertRepository.InsertAsync(urlAlert);
            }
            catch (Exception ex) {
                Log.Error("An exception occurred writing notification to database:", ex);
                return null;
            }
        }

        private async Task<HttpResponseMessage> SendTeamsMessageAsync(UrlModel urlToTest, string title, string message)
        {
            Log.Debug($"Sending Teams message to {_teamsAlertUri}");

            try {
                string body = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Teams-UrlAlertPayload.json"))
                    .Replace("%Title%", title)
                    .Replace("%Url%", urlToTest.Url)
                    .Replace("%Message%", message.Replace(@"\", @"\\").Replace(Environment.NewLine, @"\r\n").Replace("<br>", @"\r\n"));

                return await _httpClient.PostAsync(_teamsAlertUri, new StringContent(body, Encoding.UTF8, "application/json"), _cancellationToken);
            }
            catch (Exception ex) {
                Log.Error($"An exception occurred sending teams message to {_teamsAlertUri}:", ex);
                return null;
            }
        }
    }
}
