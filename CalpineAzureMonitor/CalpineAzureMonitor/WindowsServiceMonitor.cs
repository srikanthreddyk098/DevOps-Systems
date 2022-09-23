using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CalpineAzureMonitor.Data.Models;
using CalpineAzureMonitor.Data.Repositories;
using log4net;

namespace CalpineAzureMonitor
{
    class WindowsServiceMonitor
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WindowsServiceMonitor));

        private readonly string _wmiReader;
        private readonly string _wmiReaderPassword;
        private readonly WindowsServiceRepository _windowsServiceRepository;
        private readonly WindowsServiceAlertRepository _windowsServiceAlertRepository;
        private readonly EmailNotifier _emailNotifier;
        private readonly string _teamsAlertUri;

        private readonly CancellationToken _cancellationToken;
        private readonly List<int> _frequenciesAlreadyRunning;
        private readonly object _lock;

        private HttpClient _httpClient;

        internal WindowsServiceMonitor(string wmiReader, string wmiReaderPassword, WindowsServiceRepository windowsServiceRepository,
            WindowsServiceAlertRepository windowsServiceAlertRepository, EmailNotifier emailNotifier, string teamsAlertUri, CancellationToken token)
        {
            _wmiReader = wmiReader;
            _wmiReaderPassword = wmiReaderPassword;
            _windowsServiceRepository = windowsServiceRepository;
            _windowsServiceAlertRepository = windowsServiceAlertRepository;
            _emailNotifier = emailNotifier;
            _teamsAlertUri = teamsAlertUri;

            _cancellationToken = token;
            _frequenciesAlreadyRunning = new List<int>();
            _lock = new object();
        }

        public async Task StartMonitoringAsync()
        {
            using (_httpClient = new HttpClient()) {
                while (true) {
                    if (_cancellationToken.IsCancellationRequested) {
                        Log.Debug($"Cancellation request was found in main loop. Returning...");
                        return;
                    }

                    try {
                        await _windowsServiceRepository.UpdateWindowsServiceMapping();
                    }
                    catch (Exception ex) {
                        Log.Error("An exception occurred running the stored procedure sp_UpdateWindowsServiceMapping.", ex);
                    }

                    var frequencies = new List<int>();

                    try {
                        Log.Debug("Getting list of frequencies...");
                        frequencies = (await _windowsServiceRepository.GetFrequenciesAsync()).ToList();
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

                                var servicesToMonitor = new List<WindowsServiceModel>();

                                Log.Debug($"Getting services that needs to be checked every {frequency} minutes...");
                                try {
                                    servicesToMonitor = (await _windowsServiceRepository.GetServicesForFrequencyAsync(frequency)).ToList();
                                }
                                catch (Exception ex) {
                                    Log.Error($"An exception occurred getting the list of services to check for frequency: {frequency}.", ex);
                                }

                                if (servicesToMonitor.Count < 1) {
                                    lock (_lock) {
                                        _frequenciesAlreadyRunning.Remove(frequency);
                                    }

                                    return;
                                }

                                foreach (var serviceMonitor in servicesToMonitor) {
                                    var serviceTask = Task.Factory.StartNew(() =>
                                    {
                                        if (_cancellationToken.IsCancellationRequested) {
                                            return;
                                        }

                                        CheckServiceStatus(serviceMonitor);
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
                        Log.Error("Task.Delay threw a task cancelled exception.");
                    }
                }
            }
        }

        private void CheckServiceStatus(WindowsServiceModel serviceToMonitor)
        {
            Log.Debug($"Checking status of '{serviceToMonitor.Service}' for '{serviceToMonitor.Vm}'...");
            var options = new ConnectionOptions {Username = _wmiReader, Password = _wmiReaderPassword};
            ManagementScope scope = new ManagementScope($@"\\{serviceToMonitor.Vm}\root\cimv2", options);

            try {
                scope.Connect();
            }
            catch (System.Runtime.InteropServices.COMException ex) {
                if (ex.Message.StartsWith("The RPC server is unavailable.")) {
                    Log.Error($"{serviceToMonitor.Vm} could not be reached: {ex.Message}");
                    SendNotificationsAsync(serviceToMonitor, null, ex).GetAwaiter().GetResult();
                    return;
                }

                throw;
            }
            catch (Exception ex) {
                Log.Error($@"An exception occurred connect to scope \\{serviceToMonitor.Vm}\root\cimv2", ex);
                SendNotificationsAsync(serviceToMonitor, null, ex).GetAwaiter().GetResult();
                return;
            }

            var query = new ObjectQuery($"SELECT * FROM Win32_Service WHERE Name = '{serviceToMonitor.Service}'");
            var searcher = new ManagementObjectSearcher(scope, query);
            try {
                var results = searcher.Get();
                foreach (var result in results) {
                    var status = result["State"]?.ToString();
                    Log.Debug($"VM: '{serviceToMonitor.Vm}', Service: '{serviceToMonitor.Service}', Status: '{status}'");

                    if (string.IsNullOrEmpty(status) || !status.Equals("Running")) {
                        SendNotificationsAsync(serviceToMonitor, status).GetAwaiter().GetResult();
                    }
                }
            }
            catch (Exception ex) {
                Log.Error($"An exception occurred while querying Win32_Services for '{serviceToMonitor.Vm}':", ex);
                SendNotificationsAsync(serviceToMonitor, null, ex).GetAwaiter().GetResult();
            }
        }

        private async Task SendNotificationsAsync(WindowsServiceModel serviceMonitor, string status, Exception ex = null, string message = null)
        {
            var subject = string.IsNullOrEmpty(status)
                ? $"Calpine Azure Monitor Alert - Windows Service - '{serviceMonitor.Service}' on '{serviceMonitor.Vm}' is 'N/A'"
                : $"Calpine Azure Monitor Alert - Windows Service - '{serviceMonitor.Service}' on '{serviceMonitor.Vm}' is '{status}'";
            var body = string.IsNullOrEmpty(status)
                ? $"Unable to determine status of '{serviceMonitor.Service}' running on '{serviceMonitor.Vm}'.<br><br>{ex}"
                : $"The service '{serviceMonitor.Service}' running on '{serviceMonitor.Vm}' has a status of '{status}'.";

            var task1 = Task.Run(() => { SendNotificationEmail(serviceMonitor, subject, body); }, _cancellationToken);
            var task2 = Task.Run(async () =>
            {
                var result = WriteNotificationToDatabaseAsync(serviceMonitor, status, subject, body).GetAwaiter().GetResult();
            }, _cancellationToken);
            var task3 = Task.Run(async () =>
            {
                var result = SendTeamsMessageAsync(serviceMonitor, subject, body).GetAwaiter().GetResult();
                if (result == null || !result.IsSuccessStatusCode) {
                    Log.Error($"Teams message http request finished with status code: {result?.StatusCode}");
                }
            }, _cancellationToken);

            await Task.WhenAll(task1, task2, task3);
        }

        private void SendNotificationEmail(WindowsServiceModel serviceMonitor, string subject, string body)
        {

            Log.Debug($"Sending email to '{serviceMonitor.Email}' with subject '{subject}' and body '{body}'.");
            _emailNotifier.SendEmail(serviceMonitor.Email, subject, body);
        }

        private async Task<int?> WriteNotificationToDatabaseAsync(WindowsServiceModel serviceMonitor, string status, string subject, string body)
        {
            var windowsServiceAlert = new WindowsServiceAlertModel {
                Vm = serviceMonitor.Vm,
                Service = serviceMonitor.Service,
                Status = status,
                Email = serviceMonitor.Email,
                Subject = subject,
                Body = body.Replace("<br>", "")
            };

            try {
                return await _windowsServiceAlertRepository.InsertAsync(windowsServiceAlert);
            }
            catch (Exception ex) {
                Log.Error("An exception occurred writing notification to database:", ex);
                return null;
            }
        }

        private async Task<HttpResponseMessage> SendTeamsMessageAsync(WindowsServiceModel serviceMonitor, string title, string message)
        {
            Log.Debug($"Sending Teams message to {_teamsAlertUri}");

            try {
                string body = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Teams-WindowsServiceAlertPayload.json"))
                    .Replace("%Title%", title)
                    .Replace("%Computer%", serviceMonitor.Vm)
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
