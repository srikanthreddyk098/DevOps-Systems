using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using CalpineAzureMonitor.Data.Repositories;
using log4net;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;

namespace CalpineAzureMonitor
{
    partial class CalpineAzureMonitor : ServiceBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CalpineAzureMonitor));

        private readonly IList<Task> _tasks;
        private CancellationTokenSource _cancellationTokenSource;

        public CalpineAzureMonitor()
        {
            InitializeComponent();
            _tasks = new List<Task>();
        }

        internal void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            Log.Debug("OnStart event called for CalpineAzureMonitor");
            _cancellationTokenSource = new CancellationTokenSource();
            _tasks.Add(Task.Run(StartPingMonitorAsync));
            _tasks.Add(Task.Run(StartWindowsServiceMonitorAsync));
            _tasks.Add(Task.Run(StartIisMonitorAsync));
            _tasks.Add(Task.Run(StartUrlMonitorAsync));
        }

        protected override void OnStop()
        {
            Log.Debug("OnStop event called for CalpineAzureMonitor. Cancelling token.");
            _cancellationTokenSource.Cancel();
            Log.Debug("Waiting for task to finish...");
            Task.WaitAll(_tasks.ToArray());
            Log.Debug("Task finished. CalpineAzureMonitor was stopped successfully.");
        }

        private async Task StartPingMonitorAsync()
        {
            try {
                Log.Debug("StartPingMonitorAsync task started.");
                var keyVaultUrl = ConfigurationManager.AppSettings["keyVaultUrl"];
                var connectionStringTask = GetKeyVaultSecretAsync(keyVaultUrl, "connectionString");
                var teamsWebhookUriTask = GetKeyVaultSecretAsync(keyVaultUrl, "teamsWebhookUri");
                var pingRepository = new PingRepository(await connectionStringTask);
                var pingAlertRepository = new PingAlertRepository(await connectionStringTask);

                var smtpServer = ConfigurationManager.AppSettings["smtpServer"];
                var defaultFrom = ConfigurationManager.AppSettings["defaultFromEmail"];
                var emailNotifier = new EmailNotifier(smtpServer, defaultFrom);
                var pingMonitor = new PingMonitor(pingRepository, pingAlertRepository, emailNotifier, await teamsWebhookUriTask,
                    _cancellationTokenSource.Token);
                Log.Debug("Running PingMonitor StartMonitoringAsync...");
                _tasks.Add(Task.Factory.StartNew(() => pingMonitor.StartMonitoringAsync(), _cancellationTokenSource.Token, TaskCreationOptions.LongRunning,
                    TaskScheduler.Default));
            }
            catch (KeyVaultErrorException) {
                Stop();
            }
            catch (Exception ex) {
                Log.Error("An unhandled exception has occurred. Stopping PingMonitor service...", ex);
                Stop();
            }
        }

        private async Task StartWindowsServiceMonitorAsync()
        {
            try {
                Log.Debug("StartWindowsServiceMonitorAsync task started.");
                var keyVaultUrl = ConfigurationManager.AppSettings["keyVaultUrl"];
                var wmiReaderTask = GetKeyVaultSecretAsync(keyVaultUrl, "wmiReader");
                var wmiReaderPasswordTask = GetKeyVaultSecretAsync(keyVaultUrl, "wmiReaderPassword");
                var connectionStringTask = GetKeyVaultSecretAsync(keyVaultUrl, "connectionString");
                var teamsWebhookUriTask = GetKeyVaultSecretAsync(keyVaultUrl, "teamsWebhookUri");
                var windowsServiceRepository = new WindowsServiceRepository(await connectionStringTask);
                var windowsServiceAlertRepository = new WindowsServiceAlertRepository(await connectionStringTask);

                var smtpServer = ConfigurationManager.AppSettings["smtpServer"];
                var defaultFrom = ConfigurationManager.AppSettings["defaultFromEmail"];
                var emailNotifier = new EmailNotifier(smtpServer, defaultFrom);
                var windowsServiceMonitor = new WindowsServiceMonitor(await wmiReaderTask, await wmiReaderPasswordTask, windowsServiceRepository,
                    windowsServiceAlertRepository, emailNotifier, await teamsWebhookUriTask, _cancellationTokenSource.Token);
                Log.Debug("Running WindowsServiceMonitor StartMonitoringAsync...");
                _tasks.Add(Task.Factory.StartNew(() => windowsServiceMonitor.StartMonitoringAsync(), _cancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default));
            }
            catch (KeyVaultErrorException) {
                Stop();
            }
            catch (Exception ex) {
                Log.Error("An unhandled exception has occurred. Stopping WindowsServiceMonitor service...", ex);
                Stop();
            }
        }

        private async Task StartIisMonitorAsync()
        {
            try {
                Log.Debug("StartIisAppPoolMonitorAsync task started.");
                var keyVaultUrl = ConfigurationManager.AppSettings["keyVaultUrl"];
                var wmiReaderTask = GetKeyVaultSecretAsync(keyVaultUrl, "wmiReader");
                var wmiReaderPasswordTask = GetKeyVaultSecretAsync(keyVaultUrl, "wmiReaderPassword");
                var connectionStringTask = GetKeyVaultSecretAsync(keyVaultUrl, "connectionString");
                var teamsWebhookUriTask = GetKeyVaultSecretAsync(keyVaultUrl, "teamsWebhookUri");
                var iisAppPoolRepository = new IisAppPoolRepository(await connectionStringTask);
                var iisAppPoolAlertRepository = new IisAppPoolAlertRepository(await connectionStringTask);

                var smtpServer = ConfigurationManager.AppSettings["smtpServer"];
                var defaultFrom = ConfigurationManager.AppSettings["defaultFromEmail"];
                var emailNotifier = new EmailNotifier(smtpServer, defaultFrom);
                var iisAppPoolMonitor = new IisAppPoolMonitor(await wmiReaderTask, await wmiReaderPasswordTask, iisAppPoolRepository, iisAppPoolAlertRepository,
                    emailNotifier, await teamsWebhookUriTask, _cancellationTokenSource.Token);
                Log.Debug("Running IisAppPoolMonitor StartMonitoringAsync...");
                _tasks.Add(Task.Factory.StartNew(() => iisAppPoolMonitor.StartMonitoringAsync(), _cancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default));
            }
            catch (KeyVaultErrorException) {
                Stop();
            }
            catch (Exception ex) {
                Log.Error("An unhandled exception has occurred. Stopping IisAppPoolMonitor service...", ex);
                Stop();
            }
        }

        private async Task StartUrlMonitorAsync()
        {
            try
            {
                Log.Debug("StartUrlMonitorAsync task started.");
                var keyVaultUrl = ConfigurationManager.AppSettings["keyVaultUrl"];
                var connectionStringTask = GetKeyVaultSecretAsync(keyVaultUrl, "connectionString");
                var teamsWebhookUriTask = GetKeyVaultSecretAsync(keyVaultUrl, "teamsWebhookUri");
                var urlRepository = new UrlRepository(await connectionStringTask);
                var urlAlertRepository = new UrlAlertRepository(await connectionStringTask);

                var smtpServer = ConfigurationManager.AppSettings["smtpServer"];
                var defaultFrom = ConfigurationManager.AppSettings["defaultFromEmail"];
                var emailNotifier = new EmailNotifier(smtpServer, defaultFrom);
                var urlMonitor = new UrlMonitor(urlRepository, urlAlertRepository, emailNotifier, await teamsWebhookUriTask, _cancellationTokenSource.Token);
                Log.Debug("Running UrlMonitor StartMonitoringAsync...");
                _tasks.Add(Task.Factory.StartNew(() => urlMonitor.StartMonitoringAsync(), _cancellationTokenSource.Token,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default));
            }
            catch (KeyVaultErrorException)
            {
                Stop();
            }
            catch (Exception ex)
            {
                Log.Error("An unhandled exception has occurred. Stopping UrlMonitor service...", ex);
                Stop();
            }
        }

        private static async Task<string> GetKeyVaultSecretAsync(string keyVaultUrl, string secretName)
        {
            try {
                Log.Debug($"Getting key vault secret '{secretName}' from {keyVaultUrl}");
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                var secret = await keyVaultClient.GetSecretAsync($"{keyVaultUrl}/secrets/{secretName}");
                return secret.Value;
            }
            catch (KeyVaultErrorException ex) {
                Log.Error($"An exception occurred getting key vault secret '{secretName}' from {keyVaultUrl}", ex);
                throw;
            }
        }
    }
}
