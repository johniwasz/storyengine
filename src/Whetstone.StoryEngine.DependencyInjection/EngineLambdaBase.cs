using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using Whetstone.StoryEngine.Models.Configuration;


namespace Whetstone.StoryEngine.DependencyInjection
{

    /// <summary>
    /// This is the base class for all Whetstone Engine lambda functions.
    /// </summary>
    public abstract class EngineLambdaBase
    {
        private readonly Stopwatch _stopWatch = new Stopwatch();
        protected Stopwatch _coldStartTimer = Stopwatch.StartNew();

        public IServiceProvider Services { get; set; }

        public IConfiguration Configuration { get; set; }



        public EngineLambdaBase()
        {

            Console.WriteLine("Entering EngineLambdaBase constructor");
            Stopwatch configWatch = new Stopwatch();
            configWatch.Start();
            Configuration = Bootstrapping.BuildConfiguration();
            configWatch.Stop();
            Console.WriteLine($"Config load time is {configWatch.ElapsedMilliseconds}ms");

            Stopwatch bootstrapTime = new Stopwatch();
            bootstrapTime.Start();
            IServiceCollection services = new ServiceCollection();

            BootstrapConfig bootConfig = Configuration.Get<BootstrapConfig>();

            Bootstrapping.ConfigureServices(services, Configuration, bootConfig);

            // configure services on any child classes
            ConfigureServices(services, Configuration);


            this.Services = services.BuildServiceProvider();
            bootstrapTime.Stop();

            Console.WriteLine($"Services configuration time is {bootstrapTime.ElapsedMilliseconds}ms");

            Console.WriteLine("Exiting EngineLambdaBase constructor");
        }


        /// <summary>
        /// Override this method to add more services during bootstrapping.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        protected virtual void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            // This is designed to be overriden to add more services.

        }


        protected EngineLambdaBase(IServiceProvider servProv)
        {
            this.Services = servProv;
        }



        protected void StartProcessingTime()
        {
            _stopWatch.Reset();
            _stopWatch.Start();
        }

        protected void LogEndtime()
        {
            _stopWatch.Stop();

            Console.WriteLine($"Processing complete. {_stopWatch.ElapsedMilliseconds}ms");

            _stopWatch.Reset();
        }

        protected void LogEndtime(Stopwatch stopWatch)
        {

            Console.WriteLine($"Processing complete. {stopWatch.ElapsedMilliseconds}ms");
        }


    }
}
