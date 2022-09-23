using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CalpineAzureMonitor.Data.Models;
using CalpineAzureMonitor.Data.Repositories;
using log4net;

namespace CalpineAzureMonitor
{
    class IisAppPoolMonitor
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PingMonitor));

        private readonly string _wmiReader;
        private readonly SecureString _wmiReaderPassword;
        private readonly IisAppPoolRepository _iisAppPoolRepository;
        private readonly IisAppPoolAlertRepository _iisAppPoolAlertRepository;
        private readonly EmailNotifier _emailNotifier;
        private readonly string _teamsAlertUri;

        private readonly CancellationToken _cancellationToken;
        private readonly List<int> _frequenciesAlreadyRunning;

        private HttpClient _httpClient;

        internal IisAppPoolMonitor(string wmiReader, string wmiReaderPassword, IisAppPoolRepository iisAppPoolRepository,
            IisAppPoolAlertRepository iisAppPoolAlertRepository, EmailNotifier emailNotifier, string teamsAlertUri, CancellationToken token)
        {
            _wmiReader = wmiReader;
            _wmiReaderPassword = ConvertToSecureString(wmiReaderPassword);
            _iisAppPoolRepository = iisAppPoolRepository;
            _iisAppPoolAlertRepository = iisAppPoolAlertRepository;
            _emailNotifier = emailNotifier;
            _teamsAlertUri = teamsAlertUri;

            _cancellationToken = token;
            _frequenciesAlreadyRunning = new List<int>();
        }

        public async Task StartMonitoringAsync()
        {
            using (_httpClient = new HttpClient()) {
                while (true) {
                    if (_cancellationToken.IsCancellationRequested) {
                        Log.Debug($"Cancellation request was found in main loop. Returning...");
                        return;
                    }

                    var frequencies = new List<int>();

                    try {
                        Log.Debug("Getting list of frequencies...");
                        frequencies = (await _iisAppPoolRepository.GetFrequenciesAsync()).ToList();
                        Log.Debug($"Found {frequencies.Count} frequencies...");
                    }
                    catch (Exception ex) {
                        Log.Error("An exception occurred getting the list of frequencies.", ex);
                    }

                    foreach (var frequency in frequencies) {
                        if (_frequenciesAlreadyRunning.Any(x => x.Equals(frequency))) {
                            Log.Debug($"A task to handle {frequency} min frequency is already running. Skipping...");
                            continue;
                        }

                        var task = Task.Factory.StartNew(async () =>
                        {
                            while (true) {
                                if (_cancellationToken.IsCancellationRequested) {
                                    Log.Debug($"Cancellation request was found in {frequency} min loop. Returning...");
                                    return;
                                }

                                var iisAppPoolsToMonitor = new List<IisAppPoolModel>();

                                Log.Debug($"Getting IIS app pools that needs to be checked every {frequency} minutes...");
                                try {
                                    iisAppPoolsToMonitor = (await _iisAppPoolRepository.GetServicesForFrequencyAsync(frequency)).ToList();
                                }
                                catch (Exception ex) {
                                    Log.Error($"An exception occurred getting the list of IIS app pools to check for frequency: {frequency}.", ex);
                                }

                                if (iisAppPoolsToMonitor.Count < 1) {
                                    return;
                                }

                                foreach (var serviceMonitor in iisAppPoolsToMonitor) {
                                    var serviceTask = Task.Factory.StartNew(() =>
                                    {
                                        if (_cancellationToken.IsCancellationRequested) {
                                            return;
                                        }

                                        CheckIisAppPoolStatus(serviceMonitor);
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

                        _frequenciesAlreadyRunning.Add(frequency);
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

        private void CheckIisAppPoolStatus(IisAppPoolModel iisAppPoolToMonitor)
        {
            Log.Debug($"Checking status of '{iisAppPoolToMonitor.AppPool}' for '{iisAppPoolToMonitor.Vm}'...");

            try {
                WSManConnectionInfo connectionInfo = new WSManConnectionInfo
                    {Credential = new PSCredential(_wmiReader, _wmiReaderPassword), ComputerName = iisAppPoolToMonitor.Vm};
                using (var runspace = RunspaceFactory.CreateRunspace(connectionInfo)) {
                    runspace.Open();
                    using (PowerShell ps = PowerShell.Create()) {
                        ps.Runspace = runspace;
                        ps.AddScript($"Import-Module WebAdministration; Get-WebAppPoolState \"{iisAppPoolToMonitor.AppPool}\"");
                        var results = ps.Invoke();
                        foreach (var result in results) {
                            var status = result.Members["Value"].Value.ToString();
                            Log.Debug($"VM: '{iisAppPoolToMonitor.Vm}', App Pool: '{iisAppPoolToMonitor.AppPool}', Status: '{status}'");

                            if (string.IsNullOrEmpty(status) || !status.Equals("Started")) {
                                SendNotificationsAsync(iisAppPoolToMonitor, status).GetAwaiter().GetResult();
                            }
                        }
                    }

                    runspace.Close();
                }
            }
            catch (Exception ex) {
                Log.Error($"An exception occurred while getting status of app pool '{iisAppPoolToMonitor.AppPool} for '{iisAppPoolToMonitor.Vm}':", ex);
                SendNotificationsAsync(iisAppPoolToMonitor, null, ex).GetAwaiter().GetResult();
            }
        }

        private async Task SendNotificationsAsync(IisAppPoolModel iisAppPool, string status, Exception ex = null, string message = null)
        {
            var subject = string.IsNullOrEmpty(status)
                ? $"Calpine Azure Monitor Alert - Unable to check status for '{iisAppPool.AppPool}' app pool on {iisAppPool.Vm}"
                : $"Calpine Azure Monitor Alert - Status of app pool {iisAppPool.AppPool} on {iisAppPool.Vm} returned status: {status}";
            var body = string.IsNullOrEmpty(status)
                ? $"Unable to check status for '{iisAppPool.AppPool}' app pool on {iisAppPool.Vm}. {message}<br><br>{ex}"
                : $"Status of app pool {iisAppPool.AppPool} on {iisAppPool.Vm} returned status: {status}. <br><br>{message}";

            var task1 = Task.Run(() => { SendNotificationEmail(iisAppPool, subject, body); }, _cancellationToken);
            var task2 = Task.Run(async () =>
            {
                var result = WriteNotificationToDatabaseAsync(iisAppPool, status, subject, body).GetAwaiter().GetResult();
            }, _cancellationToken);
            var task3 = Task.Run(async () =>
            {
                var result = SendTeamsMessageAsync(iisAppPool, subject, body).GetAwaiter().GetResult();
                if (result == null || !result.IsSuccessStatusCode) {
                    Log.Error($"Teams message http request finished with status code: {result?.StatusCode}");
                }
            }, _cancellationToken);

            await Task.WhenAll(task1, task2, task3);
        }

        private void SendNotificationEmail(IisAppPoolModel iisAppPool, string subject, string body)
        {
            Log.Debug($"Sending email to '{iisAppPool.Email}' with subject '{subject}' and body '{body}'.");
            _emailNotifier.SendEmail(iisAppPool.Email, subject, body);
        }

        private async Task<int?> WriteNotificationToDatabaseAsync(IisAppPoolModel iisAppPool, string status, string subject, string body)
        {
            var iisAppPoolAlert = new IisAppPoolAlertModel {
                Vm = iisAppPool.Vm,
                AppPool = iisAppPool.AppPool,
                Status = status,
                Email = iisAppPool.Email,
                Subject = subject,
                Body = body.Replace("<br>", "")
            };


            try {
                return await _iisAppPoolAlertRepository.InsertAsync(iisAppPoolAlert);
            }
            catch (Exception ex) {
                Log.Error("An exception occurred writing notification to database:", ex);
                return null;
            }
        }

        private async Task<HttpResponseMessage> SendTeamsMessageAsync(IisAppPoolModel iisAppPool, string title, string message)
        {
            Log.Debug($"Sending Teams message to {_teamsAlertUri}");

            try {
                string body = System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Teams-IisAppPoolAlertPayload.json"))
                    .Replace("%Title%", title)
                    .Replace("%Computer%", iisAppPool.Vm)
                    .Replace("%Message%", message.Replace(@"\", @"\\").Replace(Environment.NewLine, @"\r\n").Replace("<br>", @"\r\n"));

                return await _httpClient.PostAsync(_teamsAlertUri, new StringContent(body, Encoding.UTF8, "application/json"), _cancellationToken);
            }
            catch (Exception ex) {
                Log.Error($"An exception occurred sending teams message to {_teamsAlertUri}:", ex);
                return null;
            }
        }

        private SecureString ConvertToSecureString(string stringToSecure)
        {
            var secureString = new SecureString();
            foreach (var c in stringToSecure) {
                secureString.AppendChar(c);
            }

            return secureString;
        }
    }
}
