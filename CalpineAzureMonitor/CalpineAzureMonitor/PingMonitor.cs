using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CalpineAzureMonitor.Data.Models;
using CalpineAzureMonitor.Data.Repositories;
using log4net;

namespace CalpineAzureMonitor
{
    class PingMonitor
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PingMonitor));

        private readonly PingRepository _pingRepository;
        private readonly PingAlertRepository _pingAlertRepository;
        private readonly EmailNotifier _emailNotifier;
        private readonly string _teamsAlertUri;

        private readonly CancellationToken _cancellationToken;
        private readonly List<int> _frequenciesAlreadyRunning;
        private readonly object _lock;

        private HttpClient _httpClient;

        internal PingMonitor(PingRepository pingRepository, PingAlertRepository pingAlertRepository, EmailNotifier emailNotifier, string teamsAlertUri,
            CancellationToken cancellationToken)
        {
            _pingRepository = pingRepository;
            _pingAlertRepository = pingAlertRepository;
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
                        await _pingRepository.UpdatePingMapping();
                    }
                    catch (Exception ex) {
                        Log.Error("An exception occurred running the stored procedure sp_UpdatePingMapping.", ex);
                    }

                    var frequencies = new List<int>();

                    try {
                        Log.Debug("Getting list of frequencies...");
                        frequencies = (await _pingRepository.GetFrequenciesAsync()).ToList();
                        Log.Debug($"Found {frequencies.Count} frequencies...");
                    }
                    catch (Exception ex) {
                        Log.Error("An exception occurred getting the list of frequencies.", ex);
                    }

                    foreach (var frequency in frequencies) {
                        lock (_lock) {
                            if (_frequenciesAlreadyRunning.Any(x => x.Equals(frequency))) {
                                Log.Debug($"A task to handle {frequency} min frequency is already running. Skipping...");
                                continue;
                            }
                        }

                        var task = Task.Factory.StartNew(async () =>
                        {
                            while (true) {
                                if (_cancellationToken.IsCancellationRequested) {
                                    Log.Debug($"Cancellation request was found in {frequency} min loop. Returning...");
                                    return;
                                }

                                var machinesToPing = new List<PingModel>();

                                Log.Debug($"Getting machines that needs to be pinged every {frequency} minutes...");
                                try {
                                    machinesToPing = (await _pingRepository.GetMachinesForFrequencyAsync(frequency)).ToList();
                                }
                                catch (Exception ex) {
                                    Log.Error($"An exception occurred getting the list machines to ping for frequency: {frequency}.", ex);
                                }

                                if (machinesToPing.Count < 1) {
                                    lock (_lock) {
                                        _frequenciesAlreadyRunning.Remove(frequency);
                                    }

                                    return;
                                }

                                foreach (var pingMonitor in machinesToPing) {
                                    var serviceTask = Task.Factory.StartNew(() =>
                                    {
                                        if (_cancellationToken.IsCancellationRequested) {
                                            return;
                                        }

                                        CheckPing(pingMonitor);
                                    }, _cancellationToken);
                                }

                                try {
                                    await Task.Delay(frequency * 60000, _cancellationToken);
                                }
                                catch (TaskCanceledException) {
                                    Log.Debug("Task.Delay threw a task cancelled exception.");
                                }
                            }
                        }, _cancellationToken);
                        lock (_lock) {
                            _frequenciesAlreadyRunning.Add(frequency);
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

        private void CheckPing(PingModel pingMonitor)
        {
            try {
                var pingStatus = GetPingStatus(pingMonitor.Vm, pingMonitor.Subscription);
                if (pingStatus != IPStatus.Success) {
                    string subject = $"Calpine Azure Monitor Alert - {pingMonitor.Subscription} - Ping check for {pingMonitor.Vm} returned status: {pingStatus.ToString()}";
                    string body = $"Ping check for {pingMonitor.Vm} returned status: {pingStatus}.";
                    SendNotificationsAsync(pingMonitor, pingStatus.ToString(), subject, body).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex) {
                Log.Error($"An exception occurred trying to ping {pingMonitor.Vm} in {pingMonitor.Subscription}:", ex);

                if (ex.InnerException?.Message == "No such host is known") {
                    if (!string.IsNullOrEmpty(pingMonitor.PrivateIp)) {
                        try {
                            var pingStatus = GetPingStatus(pingMonitor.PrivateIp, pingMonitor.Subscription);

                            if (pingStatus == IPStatus.Success) {
                                string subject = $"Calpine Azure Monitor Alert - {pingMonitor.Subscription} - DNS resolution failed for {pingMonitor.Vm}";
                                string body = $"Unable to resolve hostname: {pingMonitor.Vm}. ";
                                SendNotificationsAsync(pingMonitor, "DNS failure", subject, body).GetAwaiter().GetResult();
                            }
                            else {
                                string subject = $"Calpine Azure Monitor Alert - {pingMonitor.Subscription} - Ping check for {pingMonitor.Vm} returned status: {pingStatus.ToString()}";
                                string body = $"Unable to resolve hostname: {pingMonitor.Vm}. ";
                                body += $"<br><br>Ping check for {pingMonitor.PrivateIp} after DNS resolution failure returned: {pingStatus.ToString()}.";
                                Log.Debug(body);
                                SendNotificationsAsync(pingMonitor, pingStatus.ToString(), subject, body).GetAwaiter().GetResult();
                            }
                        }
                        catch (Exception ex2) {
                            Log.Error($"An exception occurred trying to ping {pingMonitor.PrivateIp} after DNS resolution failed:", ex2);
                            string subject = $"Calpine Azure Monitor Alert - {pingMonitor.Subscription} - Ping check for {pingMonitor.Vm} threw an exception";
                            string body = $"An exception occurred trying to ping {pingMonitor.PrivateIp} after DNS resolution failed: {ex2.Message}.";
                            body += $"<br><br>{ex2}";
                            SendNotificationsAsync(pingMonitor, "Exception", subject, body).GetAwaiter().GetResult();
                        }
                    }
                }
                else {
                    string subject = $"Calpine Azure Monitor Alert - {pingMonitor.Subscription} - Ping check for {pingMonitor.Vm} threw an exception";
                    string body = $"An exception occurred trying to ping {pingMonitor.Vm}: {ex.Message}.";
                    body += $"<br><br>{ex}";
                    SendNotificationsAsync(pingMonitor, "Exception", subject, body).GetAwaiter().GetResult();
                }
            }
        }

        private IPStatus GetPingStatus(string hostname, string subscription)
        {
            var ping = new Ping();
            var reply = ping.Send(hostname);
            Log.Debug($"Ping check for {hostname} in {subscription} returned: {reply?.Status}");

            if (reply?.Status == IPStatus.TimedOut) {
                Task.Delay(5000, _cancellationToken).GetAwaiter().GetResult();
                reply = ping.Send(hostname);
                Log.Debug($"Second ping check for {hostname} in {subscription} returned: {reply?.Status}");
            }

            if (reply?.Status == IPStatus.TimedOut) {
                Task.Delay(30000, _cancellationToken).GetAwaiter().GetResult();
                reply = ping.Send(hostname);
                Log.Debug($"Third and final ping check for {hostname} in {subscription} returned: {reply?.Status}");
            }

            if (reply == null) { throw new Exception($"Ping check for {hostname} returned null.");}
            return reply.Status;
        }

        private async Task SendNotificationsAsync(PingModel pingMonitor, string status, string subject, string body)
        {
            var task1 = Task.Run(() => { SendNotificationEmail(pingMonitor, subject, body); }, _cancellationToken);
            var task2 = Task.Run(async () =>
            {
                var result = WriteNotificationToDatabaseAsync(pingMonitor, status, subject, body).GetAwaiter().GetResult();
            }, _cancellationToken);
            var task3 = Task.Run(async () =>
            {
                var result = SendTeamsMessageAsync(pingMonitor, subject, body).GetAwaiter().GetResult();
                if (result == null || !result.IsSuccessStatusCode) {
                    Log.Error($"Teams message http request finished with status code: {result?.StatusCode}");
                }
            }, _cancellationToken);

            await Task.WhenAll(task1, task2, task3);
        }

        private void SendNotificationEmail(PingModel pingMonitor, string subject, string body)
        {
            Log.Debug($"Sending email to '{pingMonitor.Email}' with subject '{subject}' and body '{body}'.");
            _emailNotifier.SendEmail(pingMonitor.Email, subject, body);
        }

        private async Task<int?> WriteNotificationToDatabaseAsync(PingModel pingMonitor, string status, string subject, string body)
        {
            var pingAlert = new PingAlertModel {
                Subscription = pingMonitor.Subscription,
                Vm = pingMonitor.Vm,
                PrivateIp = pingMonitor.PrivateIp,
                Status = status,
                Email = pingMonitor.Email,
                Subject = subject,
                Body = body.Replace("<br>", "")
            };

            try {
                return await _pingAlertRepository.InsertAsync(pingAlert);
            }
            catch (Exception ex) {
                Log.Error("An exception occurred writing notification to database:", ex);
                return null;
            }
        }

        private async Task<HttpResponseMessage> SendTeamsMessageAsync(PingModel pingMonitor, string title, string message)
        {
            Log.Debug($"Sending Teams message to {_teamsAlertUri}");

            try {
                string body = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Teams-PingAlertPayload.json"))
                    .Replace("%Title%", title)
                    .Replace("%Computer%", pingMonitor.Vm)
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
