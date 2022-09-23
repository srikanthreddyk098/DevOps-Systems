using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.Storage.Fluent;

namespace CAD.DataCollector.DatabaseBackupFile
{
    public class AzureService
    {
        private readonly string _tenantId;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly CancellationToken _cancellationToken;

        public AzureService(string tenantId, string clientId, string clientSecret, CancellationToken cancellationToken)
        {
            _tenantId = tenantId;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _cancellationToken = cancellationToken;
        }

        private AzureCredentials GetAzureCredentials()
        {
            return SdkContext.AzureCredentialsFactory.FromServicePrincipal(_clientId, _clientSecret, _tenantId, AzureEnvironment.AzureGlobalCloud);
        }

        public Task<IStorageAccount> GetStorageAccountByResourceGroupNameAsync(string subscriptionId, string resourceGroupName, string storageAccountName)
        {
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(GetAzureCredentials()).WithSubscription(subscriptionId);
            return azure.StorageAccounts.GetByResourceGroupAsync(resourceGroupName, storageAccountName, _cancellationToken);
        }
    }
}