using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Hosting;

namespace AzureAutomation.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Register Datatables Editor database connection
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(ConfigConfiguration)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

        static void ConfigConfiguration(HostBuilderContext webHostBuilderContext, IConfigurationBuilder configurationBuilder)
        {
            var config = configurationBuilder.Build();
            var keyVaultEndpoint = config["AzureKeyVault"];
            if (string.IsNullOrEmpty(keyVaultEndpoint)) {
                configurationBuilder
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddEnvironmentVariables()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{webHostBuilderContext.HostingEnvironment.EnvironmentName.ToLower()}.json", true,
                        true);
            }
            else {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

                configurationBuilder
                    .AddAzureKeyVault(keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager())
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddEnvironmentVariables()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{webHostBuilderContext.HostingEnvironment.EnvironmentName.ToLower()}.json", optional: true,
                        reloadOnChange: true);
            }
        }
    }
}
