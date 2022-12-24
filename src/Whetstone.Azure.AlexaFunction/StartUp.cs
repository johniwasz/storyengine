using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Xml.Linq;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Models.Configuration;

[assembly: FunctionsStartup(typeof(Whetstone.Azure.AlexaFunction.Startup))]

namespace Whetstone.Azure.AlexaFunction
{
    public class Startup : FunctionBase
    {
       
        protected override void ConfigureServices(IServiceCollection services, IConfiguration config, BootstrapConfig bootConfig)
        {

        }
    }
}
