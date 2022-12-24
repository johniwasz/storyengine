namespace Whetstone.StoryEngine.DependencyInjection
{
    //public abstract class ExternalLambdaBase
    //{
    //    private static bool _isConfigLoaded = false;
    //    private static readonly object _configLocker = new object();



    //    public abstract ILogger Logger { get;  }


    //    protected void LoadConfiguration(Bootstrapping bootstrapping, ILogger logger)
    //    {

    //        if (!_isConfigLoaded)
    //        {
    //            lock (_configLocker)
    //            {
    //                if (!_isConfigLoaded)
    //                {

    //                    Logger.LogDebug("Loading configuration");

    //                    Stopwatch bootstrapTime = new Stopwatch();
    //                    bootstrapTime.Start();

    //                    try
    //                    {
    //                        var builder = new ConfigurationBuilder()
    //                            .AddEnvironmentVariables();

    //                        this.Configuration = builder.Build();
    //                    }
    //                    catch (Exception ex)
    //                    {
    //                        Logger.LogError(ErrorEvents.ContainerConfigLoadError, ex, "Error loading configuration from environment");
    //                        throw;
    //                    }

    //                    IServiceCollection services = new ServiceCollection();



    //                    IDistributedCache cache = RedisManager.

    //                    // TODO get the bucket name
    //                    services.Configure<Models.Configuration.EnvironmentConfig>(
    //                        options => { options.Region = Bootstrapping.ContainerReader.GetAwsEndpoint(); });


    //                    services.AddSingleton<IDistributedCache>(cache);
    //                    services.AddSingleton<ILogger>(logger);

    //                    LoadServices(services);

    //                    bootstrapTime.Stop();

    //                    this.Services = services.BuildServiceProvider();

    //                    Logger.LogInformation("Config load time is {0} milliseconds", bootstrapTime.ElapsedMilliseconds);
    //                    _isConfigLoaded = true;
    //                }
    //            }
    //        }
    //    }

    //    protected abstract void LoadServices(IServiceCollection services);

    //    public abstract IServiceProvider Services { get; set; }

    //    public abstract IConfiguration Configuration { get; set; }
    //}
}
