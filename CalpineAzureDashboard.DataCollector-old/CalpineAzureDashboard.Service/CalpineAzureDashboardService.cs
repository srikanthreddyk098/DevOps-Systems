using System;
using System.Configuration;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using CalpineAzureDashboard.DataCollector.LogAnalytics;
using log4net;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;

namespace CalpineAzureDashboard.Service
{
    public partial class CalpineAzureDashboardService : ServiceBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CalpineAzureDashboardService));
        private CancellationTokenSource _cancellationTokenSource;

        public CalpineAzureDashboardService()
        {
            InitializeComponent();
        }

        internal void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            Log.Debug("OnStart event called for CalpineAzureDashboardService");
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(StartCalpineAzureDashboardAsync);
        }

        protected override void OnStop()
        {
            Log.Debug("OnStop event called for CalpineAzureDashboardService. Cancelling token...");
            _cancellationTokenSource.Cancel();
            Log.Debug("Task finished. CalpineAzureDashboardService was stopped successfully.");
        }

        private async Task StartCalpineAzureDashboardAsync()
        {
            try {
                var keyVaultUri = ConfigurationManager.AppSettings["keyVaultUri"];
                var connectionStringTask = GetKeyVaultSecretAsync(keyVaultUri, "connectionString");
                var clientIdTask = GetKeyVaultSecretAsync(keyVaultUri, "clientId");
                var clientSecretTask = GetKeyVaultSecretAsync(keyVaultUri, "clientSecret");
                var tenantIdTask = GetKeyVaultSecretAsync(keyVaultUri, "tenantId");

                var logAnalyticsDataCollector = new LogAnalyticsDataCollector(_cancellationTokenSource.Token, await clientIdTask, await clientSecretTask,
                    await connectionStringTask);
                await logAnalyticsDataCollector.StartLogAnalyticsDataCollector();
            }
            catch (Exception ex) {
                Log.Error("An unexpected error has occurred: ", ex);
            }
        }

        public static async Task<string> GetKeyVaultSecretAsync(string keyVaultUrl, string secretName)
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            var secret = await keyVaultClient.GetSecretAsync($"{keyVaultUrl}/secrets/{secretName}").ConfigureAwait(false);
            return secret.Value;
        }
    }
}
