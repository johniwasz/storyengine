using Amazon;
using Amazon.S3;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Whetstone.StoryEngine.Cache;
using Whetstone.StoryEngine.Cache.DynamoDB;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.Amazon;
using Whetstone.StoryEngine.Data.DependencyInjection;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Reporting.Models;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.Repository.Amazon;

namespace Whetstone.StoryEngine.Reporting.ReportRepository
{
    public abstract class ReportTaskBase
    {
        //private Stopwatch _stopWatch = new Stopwatch();
        //protected Stopwatch _coldStartTimer = Stopwatch.StartNew();

        public IServiceProvider Services { get; set; }

        public IConfiguration Configuration { get; set; }


        public ReportTaskBase()
        {

            Console.WriteLine("Entering ReportTaskBase constructor");
            Stopwatch configWatch = new Stopwatch();
            configWatch.Start();
            Configuration = Bootstrapping.BuildConfiguration();
            configWatch.Stop();
            Console.WriteLine($"Config load time is {configWatch.ElapsedMilliseconds}ms");

            Stopwatch bootstrapTime = new Stopwatch();
            bootstrapTime.Start();
            IServiceCollection services = new ServiceCollection();

            // configure services on any child classes
            ConfigureServices(services, Configuration);


            this.Services = services.BuildServiceProvider();
            bootstrapTime.Stop();

            Console.WriteLine($"Services configuration time is {bootstrapTime.ElapsedMilliseconds}ms");

            Console.WriteLine("Exiting ReportTaskBase constructor");


        }

        private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            Stopwatch configServiceTimer = new Stopwatch();
            configServiceTimer.Start();

            BootstrapConfig bootstrapConfig = Configuration.Get<BootstrapConfig>();
            services.Configure<BootstrapConfig>(Configuration);

            services.AddOptions();

            var providers = new LoggerProviderCollection();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
#if DEBUG
                .WriteTo.Debug()
#endif
                .WriteTo.Providers(providers)
                .CreateLogger();



            services.AddLogging(builder => builder
               .AddSerilog(Log.Logger)
                .AddFilter<SerilogLoggerProvider>("Microsoft", LogLevel.Error)
                .AddFilter<SerilogLoggerProvider>(level => level >= bootstrapConfig.LogLevel.GetValueOrDefault(LogLevel.Error))

            );

            services.AddMemoryCache(x =>
            {
                x.SizeLimit = bootstrapConfig.CacheConfig.InMemoryCacheSizeLimit;
            });


            RegionEndpoint regionEnd = StoryEngine.ContainerSettingsReader.GetAwsDefaultEndpoint();
            DbUserType? userType = Bootstrapping.GetDbUserType(Configuration);

            // Apply distributed caching settings
            string dynamoDbTableName = bootstrapConfig.CacheConfig.DynamoDBTableName;

            int maxEngineRetries = bootstrapConfig.CacheConfig.MaxEngineRetries.GetValueOrDefault(Bootstrapping.DEFAULT_CACHE_ENGINE_RETRIES);

            int engineTimeout = bootstrapConfig.CacheConfig.EngineTimeout.GetValueOrDefault(Bootstrapping.DEFAULT_CACHE_ENGINE_TIMEOUT);


            services.RegisterDynamoDbCacheService(dynamoDbTableName, maxEngineRetries, engineTimeout);


            DistributedCacheEntryOptions distCacheOpts = new DistributedCacheEntryOptions
            {
                SlidingExpiration = new TimeSpan(0, 0, bootstrapConfig.CacheConfig.DefaultSlidingExpirationSeconds)
            };
            Whetstone.StoryEngine.Cache.DistributedCacheExtensions.SetDefaultCacheOptions(distCacheOpts,
                (bootstrapConfig.CacheConfig?.IsEnabled).GetValueOrDefault(true));


            services.Configure<EnvironmentConfig>(
                options =>
                {
                    options.BucketName = bootstrapConfig.ReportBucket;
                    options.Region = regionEnd;
                    options.DbUserType = userType;
                });

            var awsOptions = configuration.GetAWSOptions();
            // The options don't appear to always initialize properly which can cause issues down the line, so use
            // the AWS_DEFAULT_REGION environment variable as a fallback if we are in debug mode
            if (awsOptions.Region == null)
            {
                awsOptions.Region = regionEnd;
            }
            else
            {
                regionEnd = awsOptions.Region;
            }

            services.AddDefaultAWSOptions(awsOptions);

            services.Configure<AmazonS3Config>(x =>
            {
                x.RegionEndpoint = awsOptions.Region;
                x.ReadWriteTimeout = new TimeSpan(0, 0, 0, 0, bootstrapConfig.DynamoDBTables.Timeout.GetValueOrDefault(Bootstrapping.DEFAULT_CACHE_ENDPOINT_TIMEOUT));
                x.Timeout = new TimeSpan(0, 0, 0, 0, bootstrapConfig.DynamoDBTables.Timeout.GetValueOrDefault(Bootstrapping.DEFAULT_CACHE_ENDPOINT_TIMEOUT));
                x.MaxErrorRetry = bootstrapConfig.DynamoDBTables.ErrorRetries.GetValueOrDefault(Bootstrapping.DEFAULT_CACHE_ENDPOINT_RETRIES);
            });

            services.AddSingleton<IAmazonS3, WhetstoneS3Client>();

            services.AddTransient<IFileRepository, S3FileStore>();

            services.AddTransient<IReportDefinitionRetriever, ReportDefinitionRetrieverS3>();

            services.AddTransient<IReportRequestProcessor, ReportRequestProcessor>();

            DatabaseConfig dbConfig = bootstrapConfig.DatabaseSettings;
            DataBootstrapping.ConfigureDatabaseService(services, dbConfig);

            services.AddTransient<ReportDataRetrieverFunction>();

            services.AddTransient<Func<ReportDataSourceType, IReportDataRetriever>>(serviceProvider => retrieverKey =>
            {
                IReportDataRetriever dataRetriever = null;

                switch (retrieverKey)
                {
                    case ReportDataSourceType.DatabaseFunction:
                        dataRetriever = serviceProvider.GetService<ReportDataRetrieverFunction>();
                        break;
                    default:
                        throw new KeyNotFoundException(); // or maybe return null, up to you
                }

                return dataRetriever;
            });



            services.AddTransient<ReportFormatterCsv>();
            //  services.AddTransient<SmsStepFunctionHandler>();

            services.AddTransient<Func<ReportOutputType, IReportFormatter>>(serviceProvider => outputKey =>
            {
                IReportFormatter reportFormatter = null;

                switch (outputKey)
                {
                    case ReportOutputType.Csv:
                        reportFormatter = serviceProvider.GetService<ReportFormatterCsv>();
                        break;
                    //case SmsHandlerType.StepFunctionSender:
                    //    retSmsHandler = x.GetService<SmsStepFunctionHandler>();
                    //    break;
                    default:
                        throw new KeyNotFoundException(); // or maybe return null, up to you
                }

                return reportFormatter;
            });

            //ISftpClient sftpClient, ISecretStoreReader secretReader)

            services.AddTransient<ISftpClient, SftpClient>();

            services.AddTransient<ISecretStoreReader, SecretStoreReader>();

            services.AddTransient<ReportSenderSftp>();
            //  services.AddTransient<SmsStepFunctionHandler>();

            services.AddTransient<Func<ReportDestinationType, IReportSender>>(serviceProvider => outputKey =>
            {
                IReportSender reportSender = null;

                switch (outputKey)
                {
                    case ReportDestinationType.SftpEndpoint:
                        reportSender = serviceProvider.GetService<ReportSenderSftp>();
                        break;
                    //case SmsHandlerType.StepFunctionSender:
                    //    retSmsHandler = x.GetService<SmsStepFunctionHandler>();
                    //    break;
                    default:
                        throw new KeyNotFoundException(); // or maybe return null, up to you
                }

                return reportSender;
            });

        }
    }
}
