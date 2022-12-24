using Amazon;
using Amazon.DynamoDBv2;
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
using Whetstone.StoryEngine.Cache.DynamoDB;
using Whetstone.StoryEngine.ConfigurationExtensions;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.Amazon;
using Whetstone.StoryEngine.Data.Caching;
using Whetstone.StoryEngine.Data.Yaml;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.Repository.Actions;
using Whetstone.StoryEngine.Repository.Amazon;


namespace Whetstone.StoryEngine.DependencyInjection
{
    public class Bootstrapping
    {
        public static readonly string DBUSERTYPE = "DBUSERTYPE";

        public static readonly int DEFAULT_CACHE_ENDPOINT_TIMEOUT = 2000;
        public static readonly int DEFAULT_CACHE_ENGINE_TIMEOUT = 2000;
        public static readonly int DEFAULT_CACHE_ENDPOINT_RETRIES = 3;
        public static readonly int DEFAULT_CACHE_ENGINE_RETRIES = 2;




        public static IContainerSettingsReader ContainerReader { get; } = new ContainerSettingsReader();

        internal static ILoggerFactory LogFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
#if DEBUG
            builder.AddDebug();
#endif
        });



        public static IConfiguration BuildConfiguration()
        {
            return BuildConfiguration(true);


        }

        public static RegionEndpoint CurrentRegion { get; private set; }

        public static IConfiguration BuildConfiguration(bool useParameterStore)
        {
            IConfiguration retConfiguration;

            ILogger<Bootstrapping> logger = LogFactory.CreateLogger<Bootstrapping>();

            try
            {
                if (ContainerReader == null)
                    throw new ArgumentNullException(nameof(ContainerReader));

                CurrentRegion = ContainerReader.GetAwsEndpoint();

                logger.LogDebug($"Current region is {CurrentRegion.SystemName}");

                IConfigurationBuilder builder;

                if (useParameterStore)
                {
                    string bootParam = ContainerReader.BootstrapParameter;


                    if (string.IsNullOrWhiteSpace(bootParam))
                        throw new Exception($"{bootParam} parameter is empty or null. Cannot proceed.");

                    logger.LogDebug($"Loading configuration data from bootstrap parameter: {bootParam}");


                    builder = new ConfigurationBuilder()
                            .AddParameterYamlStore(CurrentRegion, bootParam, LogFactory)
                            .AddEnvironmentVariables();
                }
                else
                {
                    builder = new ConfigurationBuilder()
                        .AddEnvironmentVariables();

                }


                retConfiguration = builder.Build();

            }
            catch (Exception ex)
            {
                string errMessage = "Error loading configuration";
                logger.LogError(ErrorEvents.ContainerConfigLoadError, ex, errMessage);
                throw new Exception(errMessage, ex);
            }

            return retConfiguration;
        }


        public static void ConfigureServices(IServiceCollection services, IConfiguration config, BootstrapConfig bootstrapConfig)
        {
            Stopwatch configServiceTimer = new Stopwatch();
            configServiceTimer.Start();

            // Logger to use only while configuring services.
            ILogger<Bootstrapping> logger = LogFactory.CreateLogger<Bootstrapping>();

            services.AddOptions();
            services.AddMemoryCache(x =>
            {
                x.SizeLimit = bootstrapConfig.CacheConfig.InMemoryCacheSizeLimit;
            });

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
                .AddFilter<SerilogLoggerProvider>(level => level >= bootstrapConfig.LogLevel.GetValueOrDefault(LogLevel.Error)));

            var awsOptions = config.GetAWSOptions();

            // The options don't appear to always initialize properly which can cause issues down the line, so use
            // the AWS_DEFAULT_REGION environment variable as a fallback if we are in debug mode
            if (awsOptions.Region == null)
            {
                awsOptions.Region = CurrentRegion;
            }
            else
            {
                CurrentRegion = awsOptions.Region;
            }

            services.AddDefaultAWSOptions(awsOptions);


            services.Configure<AmazonDynamoDBConfig>(x =>
            {
                x.RegionEndpoint = awsOptions.Region;
                x.ReadWriteTimeout = new TimeSpan(0, 0, 0, 0, bootstrapConfig.DynamoDBTables.Timeout.GetValueOrDefault(DEFAULT_CACHE_ENDPOINT_TIMEOUT));
                x.Timeout = new TimeSpan(0, 0, 0, 0, bootstrapConfig.DynamoDBTables.Timeout.GetValueOrDefault(DEFAULT_CACHE_ENDPOINT_TIMEOUT));
                x.MaxErrorRetry = bootstrapConfig.DynamoDBTables.ErrorRetries.GetValueOrDefault(DEFAULT_CACHE_ENDPOINT_RETRIES);
            });

            // Apply the same Dynamo throttling settings to S3 storage retrieval.
            services.Configure<AmazonS3Config>(x =>
            {
                x.RegionEndpoint = awsOptions.Region;
                x.ReadWriteTimeout = new TimeSpan(0, 0, 0, 0, bootstrapConfig.DynamoDBTables.Timeout.GetValueOrDefault(DEFAULT_CACHE_ENDPOINT_TIMEOUT));
                x.Timeout = new TimeSpan(0, 0, 0, 0, bootstrapConfig.DynamoDBTables.Timeout.GetValueOrDefault(DEFAULT_CACHE_ENDPOINT_TIMEOUT));
                x.MaxErrorRetry = bootstrapConfig.DynamoDBTables.ErrorRetries.GetValueOrDefault(DEFAULT_CACHE_ENDPOINT_RETRIES);
            });

            services.AddSingleton<IAmazonDynamoDB, WhetstoneDynamoDbClient>();

            services.AddSingleton<IAmazonS3, WhetstoneS3Client>();


            DbUserType? userType = GetDbUserType(config);


            string dynamoDbTableName = bootstrapConfig.CacheConfig.DynamoDBTableName;

            int maxCacheEngineRetries = bootstrapConfig.CacheConfig.MaxEngineRetries.GetValueOrDefault(DEFAULT_CACHE_ENGINE_RETRIES);

            int engineTimeout = bootstrapConfig.CacheConfig.EngineTimeout.GetValueOrDefault(DEFAULT_CACHE_ENGINE_TIMEOUT);

            //var distDbSettings = new DistributedCacheDynamoDbSettings(dynamoDbTableName, regionEnd, bootstrapConfig.CacheConfig.Timeout);

            services.RegisterDynamoDbCacheService(dynamoDbTableName, maxCacheEngineRetries, engineTimeout);

            DistributedCacheEntryOptions distCacheOpts = new DistributedCacheEntryOptions
            {
                SlidingExpiration = new TimeSpan(0, 0, bootstrapConfig.CacheConfig.DefaultSlidingExpirationSeconds)

            };


            Whetstone.StoryEngine.Cache.DistributedCacheExtensions.SetDefaultCacheOptions(distCacheOpts,
                 (bootstrapConfig.CacheConfig?.IsEnabled).GetValueOrDefault(true));

            services.Configure<Models.Configuration.EnvironmentConfig>(
              options =>
              {
                  options.BucketName = bootstrapConfig.Bucket;
                  options.Region = CurrentRegion;
                  options.DbUserType = userType;
              });




            services.AddSingleton<IStepFunctionSender, StepFunctionSender>();

            services.Configure<Models.Configuration.StepFunctionNotificationConfig>(
            options =>
            {
                options.ResourceName = bootstrapConfig.SmsConfig.NotificationStepFunctionArn;
            });


            services.Configure<Models.Configuration.SessionAuditConfig>(
            options => { options.SessionAuditQueue = bootstrapConfig.SessionAuditQueue; });


            services.Configure<PhoneConfig>(
            options => { options.SourceSmsNumber = bootstrapConfig.SmsConfig?.SourceNumber; });


            services.Configure<DynamoDBTablesConfig>(options =>
                {
                    options.UserTable = bootstrapConfig.DynamoDBTables.UserTable;
                });

            services.Configure<MessagingConfig>(
            options =>
            {
                options.ThrottleRetryLimit = (bootstrapConfig.SmsConfig?.MessageSendRetryLimit).GetValueOrDefault(0);
                options.MessageSendDelayInterval =
                    (bootstrapConfig.SmsConfig?.MessageDelaySeconds).GetValueOrDefault(0);
            });

            if (bootstrapConfig.SmsConfig?.TwilioConfig != null)
            {
                services.Configure<TwilioConfig>(options =>
                {
                    options.StatusCallbackUrl = bootstrapConfig.SmsConfig.TwilioConfig.StatusCallbackUrl;
                    options.LiveCredentials = bootstrapConfig.SmsConfig.TwilioConfig.LiveCredentials;
                    options.TestCredentials = bootstrapConfig.SmsConfig.TwilioConfig.TestCredentials;
                });
            }


            if (!string.IsNullOrWhiteSpace(bootstrapConfig.Debug?.LocalFileConfig?.RootPath))
            {
                services.Configure<LocalFileConfig>(options =>
                    {
                        options.RootPath = bootstrapConfig.Debug.LocalFileConfig.RootPath;
                    });
            }

            SmsHandlerType handlerType =
                (bootstrapConfig.SmsConfig?.SmsHandlerType).GetValueOrDefault(SmsHandlerType.StepFunctionSender);

            SessionLoggerType envLoggerType =
                bootstrapConfig.SessionLoggerType.GetValueOrDefault(SessionLoggerType.Queue);

            services.Configure<AuditClientMessagesConfig>(
                options => { options.AuditClientMessages = bootstrapConfig.RecordMessages; });


            services.AddTransient<ISecretStoreReader, SecretStoreReader>();

            services.AddScoped<IStoryRequestProcessor, StoryRequestProcessor>();

            services.AddSingleton<ISessionStoreManager, SessionStoreManager>();

            services.AddTransient<ITitleCacheRepository, TitleCacheRepository>();

            SmsSenderType envSmsSenderType =
                (bootstrapConfig.SmsConfig?.SmsSenderType).GetValueOrDefault(SmsSenderType.Twilio);


            //    services.AddTransient<UserDataRepository>();

            services.RegisterDynamoDbUserRepository(bootstrapConfig.DynamoDBTables.UserTable);



            services.AddTransient<IAppMappingReader, CacheAppMappingReader>();

            services.AddSingleton<ISkillCache, SkillCache>();

            // This is intended to be the same across a single request for the service.
            services.AddTransient<ITitleReader, YamlTitleReader>();

            services.AddActionProcessors();


            services.AddTransient<IMediaLinker, S3MediaLinker>();


            configServiceTimer.Stop();

            logger.LogInformation($"Bootstrapping ConfigureServices load time: {configServiceTimer.ElapsedMilliseconds} milliseconds");

        }

        public static void ConfigureLambdaServices(IServiceCollection services, BootstrapConfig bootConfig)
        {

            //services.AddSingleton<IOutboundMessageLogger, OutboundMessageDatabaseLogger>();

            services.AddSingleton<Func<UserRepositoryType, IStoryUserRepository>>(serviceProvider => handlerKey =>
            {
                IStoryUserRepository retUserRep = null;
                switch (handlerKey)
                {
                    case UserRepositoryType.Database:
                        //  retUserRep = serviceProvider.GetService<UserDataRepository>();
                        throw new KeyNotFoundException();
                    case UserRepositoryType.DynamoDB:
                        retUserRep = serviceProvider.GetService<DynamoDBUserRepository>();
                        break;
                    default:
                        throw new KeyNotFoundException(); // or maybe return null, up to you
                }

                return retUserRep;
            });


            if (string.IsNullOrWhiteSpace(bootConfig.Debug?.LocalFileConfig?.RootPath))
            {
                services.AddTransient<IFileReader, S3FileReader>();
            }
            else
            {
                services.AddTransient<IFileReader, LocalFileReader>();
            }
        }
        public static DbUserType? GetDbUserType(IConfiguration config)
        {
            string dbUserType = config[DBUSERTYPE];
            DbUserType? retType = null;

            if (!string.IsNullOrWhiteSpace(dbUserType))
            {
                if (Enum.TryParse<DbUserType>(dbUserType, true, out var userType))
                    retType = userType;
            }

            return retType;
        }





    }
}
