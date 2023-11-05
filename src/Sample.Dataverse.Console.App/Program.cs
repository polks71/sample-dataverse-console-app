// See https://aka.ms/new-console-template for more information
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Sample.Dataverse.Console.App;
using System.Reflection;

string dataverseurl = string.Empty;
IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.AddSimpleConsole();
        logging.AddDebug();
    })
    .ConfigureAppConfiguration((context, config) =>
    {
        var settings = config.Build();
        dataverseurl = settings.GetValue<string>("dataverseUrl");
        if (!context.HostingEnvironment.IsDevelopment())
        {
            //loads configuration settings from an Azure KeyVault instance
            config.AddAzureKeyVault(new Uri(settings.GetValue<string>("keyvaulturl")), new DefaultAzureCredential());
        }
        else
        {
            config.AddUserSecrets(Assembly.GetExecutingAssembly());
        }

    })
    .ConfigureServices((services) =>
    {

        services.AddTransient<SampleWorker>();
        services.AddMemoryCache();
        services.AddSingleton(new DefaultAzureCredential());

        services.AddSingleton<IOrganizationService, ServiceClient>(provider =>
        {
            var managedIdentity = provider.GetRequiredService<DefaultAzureCredential>();
            var environment = dataverseurl;
            var cache = provider.GetService<IMemoryCache>();
            return new ServiceClient(
                    tokenProviderFunction: f => GetDataverseToken(environment, managedIdentity, cache),
                    instanceUrl: new Uri(environment),
                    useUniqueInstance: true);
        });




    }).Build();
var sampleworker = host.Services.GetRequiredService<SampleWorker>();
await sampleworker.ExecuteAsync();


async Task<string> GetDataverseToken(string environment, DefaultAzureCredential credential, IMemoryCache cache)
{
    var accessToken = await cache.GetOrCreateAsync(environment, async (cacheEntry) => {
        cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20);
        var token = (await credential.GetTokenAsync(new TokenRequestContext(new[] { $"{environment}/.default" })));
        return token;
    });
    return accessToken.Token;
}