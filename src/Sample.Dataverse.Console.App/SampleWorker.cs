using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Sample.Dataverse.Console.App
{
    internal class SampleWorker : ProcessorBase<SampleWorker>
    {
        public SampleWorker(IOrganizationService organizationService, ILogger<SampleWorker> logger) :
            base(organizationService, logger)
        {

        }

        public async Task ExecuteAsync()
        {
            var accounts = await GetAcounts();
            //do some more work
            _logger.LogInformation($"{accounts.Count} Accounts found");

        }


        public async Task<List<Entity>> GetAcounts()
        {
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
<fetch>
  <entity name=""account"">
    <attribute name=""name"" />
    <attribute name=""accountnumber"" />
  </entity>
</fetch>";
            var accountsResult = await _serviceClient.RetrieveMultipleAsync(new FetchExpression(fetchXml));
            return accountsResult.Entities.ToList();
        }
    }
}
