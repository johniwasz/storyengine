using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.UnitTests
{
    public class ClientLambdaProxy : ClientLambdaBase
    {

        internal bool IsLoggerAvailable()
        {
            ILogger<ClientLambdaProxy> logger = Services.GetService<ILogger<ClientLambdaProxy>>();

            return logger != null;

        }


        protected override void ConfigureServices(IServiceCollection services, IConfiguration config, BootstrapConfig bootConfig)
        {
            // do nothing
        }
    }
}
