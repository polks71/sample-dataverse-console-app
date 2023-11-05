using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;

namespace Sample.Dataverse.Console.App
{
    internal abstract class ProcessorBase<T>
    {
        
        protected readonly ILogger<T> _logger;
        protected readonly ServiceClient _serviceClient;
        public ProcessorBase(IOrganizationService organizationService, ILogger<T> logger)
        {
            
            _logger = logger;
            _serviceClient = (ServiceClient)organizationService;
        }
    }
}
