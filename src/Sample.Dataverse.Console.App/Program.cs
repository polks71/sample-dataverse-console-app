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
        //Get a reference to the URL of Dataverse
        dataverseurl = settings.GetValue<string>("dataverseUrl");


        //loads configuration settings from an Azure KeyVault instance
        config.AddAzureKeyVault(new Uri(settings.GetValue<string>("keyvaulturl")), new DefaultAzureCredential());

        //If desired to use a local secrets file comment the KeyVault line and uncomment this line.
        //config.AddUserSecrets(Assembly.GetExecutingAssembly());


    })
    .ConfigureServices((services) =>
    {
        //Add the SampleWork class
        services.AddTransient<SampleWorker>();
        //Add MemoryCache for Dataverse Token
        services.AddMemoryCache();
        //Add a reference to the DefaultAzureCredential
        services.AddSingleton(new DefaultAzureCredential());
        //Get a ServiceClient singleton
        services.AddSingleton<IOrganizationService, ServiceClient>(provider =>
        {

            var managedIdentity = provider.GetRequiredService<DefaultAzureCredential>();
            var environment = dataverseurl;
            var cache = provider.GetService<IMemoryCache>();
            //create a new ServiceClient using the GetDatataverseToken method to manage the token.
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
    var accessToken = await cache.GetOrCreateAsync(environment, async (cacheEntry) =>
    {
        cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20);
        var token = (await credential.GetTokenAsync(new TokenRequestContext(new[] { $"{environment}/.default" })));
        return token;
    });
    return accessToken.Token;
}