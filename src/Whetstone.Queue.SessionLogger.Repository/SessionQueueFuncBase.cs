using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.DependencyInjection;
using Whetstone.StoryEngine.Data.EntityFramework;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.Queue.SessionLogger.Repository
{
    public abstract class SessionQueueFuncBase : EngineLambdaBase
    {
        protected override void ConfigureServices(IServiceCollection services, IConfiguration config)
        {


            BootstrapConfig bootConfig = Configuration.Get<BootstrapConfig>();

            DatabaseConfig dbConfig = bootConfig.DatabaseSettings;

            DataBootstrapping.ConfigureDatabaseService(services, dbConfig);

            services.AddTransient<ISessionLogger, SessionDataLogger>();

            services.AddTransient<IQueueSessionProcessor, QueueSessionProcessor>();


            base.ConfigureServices(services, config);
        }
    }
}
