using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.WebLibrary
{
    /// <summary>
    /// This is for use by the client-facing lambda components to
    /// speed up boot time.
    /// </summary>
    /// <remarks>Rather than reading from the bootstrap config parameter,
    /// it uses environment variables.</remarks>
    public abstract class ApiLambdaBase
    {
        protected Stopwatch _coldStartTimer = new Stopwatch();
        public IServiceProvider Services { get; set; }

        public IConfiguration Configuration { get; set; }

        protected ApiLambdaBase()
        {

            _coldStartTimer.Start();
            Console.WriteLine("Entering ApiLambdaBase constructor");
            Stopwatch configWatch = new Stopwatch();
            configWatch.Start();

            Configuration = Bootstrapping.BuildConfiguration();

            configWatch.Stop();
            Console.WriteLine($"Config load time is {configWatch.ElapsedMilliseconds}ms");

            Stopwatch bootstrapTime = new Stopwatch();
            bootstrapTime.Start();
            IServiceCollection services = new ServiceCollection();

            BootstrapConfig bootstrapConfig = Configuration.Get<BootstrapConfig>();


            this.ConfigureServices(services, Configuration, bootstrapConfig);


            this.Services = services.BuildServiceProvider();
            bootstrapTime.Stop();

            Console.WriteLine($"Services configuration time is {bootstrapTime.ElapsedMilliseconds}ms");

            Console.WriteLine("Exiting ApiLambdaBase constructor");

        }

        protected virtual void ConfigureServices(IServiceCollection services, IConfiguration config, BootstrapConfig bootConfig)
        {
            services.UseStoryEngineServices(config, bootConfig);

        }

    }
}
