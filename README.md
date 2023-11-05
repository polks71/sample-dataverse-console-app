# Demonstrate DefaultAzureCredential and Dataverse
This console app is very simple to demonstrate a couple of key ideas:
1. Connecting to Dataverse using DefaultAzureCredential
2. Adding setings from KeyVault
Implementing these two ideas can easily eliminate the possiblity of passwords or app secrets making their way into source control.

## Why KeyVault?
I use KeyVault to store any secrets, key, credentials, or anything else I do not want in source control. A big advantage of using KeyVault even during development is those values that should be kept secret never exist on my local machine, other than in memory temporarily.

## Managed Identity
 This also opens the potential to use [Managed Identities](https://learn.microsoft.com/en-us/azure/app-service/overview-managed-identity?tabs=portal%2Chttp) in Azure Functions, WebApps, etc... to connect to Dataverse. 
 
## DefaultAzureCredential
The key to this functionality is the [DefaultAzureCredential](https://learn.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet) class. This class will look for a token using the following place in order:
* EnvironmentCredential
* WorkloadIdentityCredential
* ManagedIdentityCredential
* SharedTokenCacheCredential
* VisualStudioCredential
* VisualStudioCodeCredential
* AzureCliCredential
* AzurePowerShellCredential
* AzureDeveloperCliCredential
* InteractiveBrowserCredential

### Developing with DefaultAzureCredential
I have found the easiest method to using the DefaultAzureCredential within Visual Studio debugging sessions takes three steps: 
1. [Install the Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-windows?tabs=azure-cli)

Once the Azure CLI is installed it is two easy steps:
2. In Visual Studio open Developer PowerShell terminal.
3. Enter `az login` and press Enter. This will open a browser window where you login and the close the window/tab.

That is it. From then on in Visual Studio the DefaultAzureCredential will resolve to the credentials you entered. It is possible for it to pick up your Visual Studio credential but I ran into many times where the Visual Studio credential cache had timed out and didn't work. Using `az login` seems to work the best.