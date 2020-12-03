using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VismaBugBountySelfServicePortal
{
    public class Program
    {
        private static string KeyVaultEndpointFormat => "https://{0}.vault.azure.net/";

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureAppConfiguration((context, config) =>
                {
                    if (!context.HostingEnvironment.IsProduction())
                        return;

                    var azureServiceTokenProvider = new AzureServiceTokenProvider();
                    var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                    using ConfigurationRoot builder = (ConfigurationRoot)config.Build();
                    var keyVaultName = builder["KeyVaultName"];
                    var keyVaultUrl = string.Format(KeyVaultEndpointFormat, keyVaultName);
                    config.AddAzureKeyVault(keyVaultUrl, keyVaultClient, new DefaultKeyVaultSecretManager());

                })
                .UseStartup<Startup>();


    }
}
