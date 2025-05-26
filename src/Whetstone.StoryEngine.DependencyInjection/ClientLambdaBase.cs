using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Messaging.Sms;

namespace Whetstone.StoryEngine.DependencyInjection
{

    /// <summary>
    /// This is for use by the client-facing lambda components to
    /// speed up boot time.
    /// </summary>
    /// <remarks>Rather than reading from the bootstrap config parameter,
    /// it uses environment variables.</remarks>
    public abstract class ClientLambdaBase
    {
        public static readonly string STORYBUCKETCONFIG = "BUCKET";
        public static readonly string ISCACHEENABLEDCONFIG = "ISCACHEENABLED";
        public static readonly string CACHESLIDINGCONFIG = "CACHESLIDINGSECONDS";
        public static readonly string CACHETABLECONFIG = "CACHETABLE";
        public static readonly string USERTABLECONFIG = "USERTABLE";
        public static readonly string LOGLEVELCONFIG = "LOGLEVEL";
        public static readonly string MESSAGESTEPFUNCTIONCONFIG = "MESSAGESTEPFUNCTION";
        public static readonly string SESSIONAUDITURLCONFIG = "SESSIONQUEUEURL";
        public static readonly string DEFAULTSMSSENDERCONFIG = "DEFAULTSMSSENDER";
        public static readonly string DEFAULTSMSSOURCENUMBERCONFIG = "DEFAULTSOUCEPHONE";
        public static readonly string TWILIOCALLBACKURLCONFIG = "TWILIOCALLBACKURL";
        public static readonly string TWILIOLIVESECRETKEYCONFIG = "TWILIOLIVESECRETKEY";
        public static readonly string TWILIOTESTSECRETKEYCONFIG = "TWILIOTESTSECRETKEY";

        /// <summary>
        /// Use the local file path for debugging when testing a local store of titles.
        /// </summary>
        public static readonly string LOCALFILEPATH = "LOCAL_FILE_PATH";

        public static readonly string MAXCACHEENGINERETRIESCONFIG = "MAXCACHEENGINERETRIES";
        public static readonly string MAXCACHEENGINETIMEOUTCONFIG = "CACHEENGINETIMEOUT";

        public static readonly string DYNAMODBTIMEOUTCONFIG = "DYNAMODBTIMEOUT";
        public static readonly string MAXDYNAMODBERRORRETRIESCONFIG = "DYNAMODBMAXERRORRETRIES";

        public static readonly string INMEMORYCACHESIZELIMITCONFIG = "INMEMORYCACHESIZELIMIT";

        protected Stopwatch _coldStartTimer = new Stopwatch();
        public IServiceProvider Services { get; set; }

        public IConfiguration Configuration { get; set; }


        protected ClientLambdaBase()
        {

            _coldStartTimer.Start();
            Console.WriteLine("Entering ClientLambdaBase constructor");
            Stopwatch configWatch = new Stopwatch();
            configWatch.Start();
            Configuration = Bootstrapping.BuildConfiguration(false);

            configWatch.Stop();
            Console.WriteLine($"Config load time is {configWatch.ElapsedMilliseconds}ms");

            Stopwatch bootstrapTime = new Stopwatch();
            bootstrapTime.Start();
            IServiceCollection services = new ServiceCollection();

            BootstrapConfig bootConfig = new BootstrapConfig
            {
                Bucket = Configuration[STORYBUCKETCONFIG],
                CacheConfig = GetCacheConfig(Configuration),
                SessionLoggerType = SessionLoggerType.Queue,
                LogLevel = GetLogLevel(Configuration),
                DynamoDBTables = GetDynamoDBTablesConfig(Configuration),
                SmsConfig = new SmsConfig
                {
                    SmsHandlerType = SmsHandlerType.StepFunctionSender,
                    NotificationStepFunctionArn = Configuration[MESSAGESTEPFUNCTIONCONFIG],
                    SmsSenderType = GetSmsSenderType(Configuration),
                    SourceNumber = Configuration[DEFAULTSMSSOURCENUMBERCONFIG]
                },
                SessionAuditQueue = Configuration[SESSIONAUDITURLCONFIG],
                EnforceAlexaPolicy = true,
                RecordMessages = false
            };

            string localPath = Configuration[LOCALFILEPATH];

            if (!string.IsNullOrWhiteSpace(localPath))
            {

                if (bootConfig.Debug == null)
                {
                    bootConfig.Debug = new DebugConfig();
                }

                if (bootConfig.Debug.LocalFileConfig == null)
                {
                    bootConfig.Debug.LocalFileConfig = new LocalFileConfig();
                }

                bootConfig.Debug.LocalFileConfig.RootPath = localPath;
            }


            Bootstrapping.ConfigureServices(services, Configuration, bootConfig);

            Bootstrapping.ConfigureLambdaServices(services, bootConfig);

            ConfigureServices(services, Configuration, bootConfig);


            this.Services = services.BuildServiceProvider();
            bootstrapTime.Stop();

            Console.WriteLine($"Services configuration time is {bootstrapTime.ElapsedMilliseconds}ms");

            Console.WriteLine("Exiting ClientLambdaBase constructor");

        }

        private DynamoDBTablesConfig GetDynamoDBTablesConfig(IConfiguration configuration)
        {
            var retConfig = new DynamoDBTablesConfig { UserTable = Configuration[USERTABLECONFIG] };

            string maxDynamoDBErrorRetriesText = configuration[MAXDYNAMODBERRORRETRIESCONFIG];


            if (int.TryParse(maxDynamoDBErrorRetriesText, out int maxErrorRetries))
            {
                retConfig.ErrorRetries = maxErrorRetries;
            }

            string timeoutText = configuration[DYNAMODBTIMEOUTCONFIG];
            if (int.TryParse(timeoutText, out int dynamoDbTimeout))
            {
                retConfig.Timeout = dynamoDbTimeout;
            }


            return retConfig;
        }

        private CacheConfig GetCacheConfig(IConfiguration configuration)
        {
            CacheConfig config = new CacheConfig();

            string isEnabledText = configuration[ISCACHEENABLEDCONFIG];
            // Assume it is enabled unless explicitly set to false
            bool isEnabled = true;


            if (bool.TryParse(isEnabledText, out bool isEnabledResult))
            {
                isEnabled = isEnabledResult;
            }


            config.IsEnabled = isEnabled;

            int slidingSeconds = GetIntConfig(configuration, CACHESLIDINGCONFIG);

            config.DefaultSlidingExpirationSeconds = slidingSeconds == 0 ? 900 : slidingSeconds;

            config.DynamoDBTableName = configuration[CACHETABLECONFIG];



            string maxCacheEngineRetriesText = configuration[MAXCACHEENGINERETRIESCONFIG];
            if (int.TryParse(maxCacheEngineRetriesText, out int maxCacheEngineRetries))
            {
                config.MaxEngineRetries = maxCacheEngineRetries;
            }



            string cacheEngineTimeoutText = configuration[MAXCACHEENGINETIMEOUTCONFIG];
            if (int.TryParse(cacheEngineTimeoutText, out int engineTimeout))
            {
                config.EngineTimeout = engineTimeout;
            }

            string inMemoryCacheSizeLimitText = configuration[INMEMORYCACHESIZELIMITCONFIG];

            if (int.TryParse(inMemoryCacheSizeLimitText, out int inMemoryCacheSizeLimit))
            {
                config.InMemoryCacheSizeLimit = inMemoryCacheSizeLimit;
            }

            return config;

        }

        private int GetIntConfig(IConfiguration config, string configSetting)
        {
            int retVal = 0;

            string configText = config[configSetting];

            if (!string.IsNullOrWhiteSpace(configText))
            {
                if (int.TryParse(configText, out int intVal))
                {
                    retVal = intVal;
                }

            }

            return retVal;
        }

        private LogLevel GetLogLevel(IConfiguration config)
        {
            LogLevel retVal = LogLevel.Debug;

            string configText = config[LOGLEVELCONFIG];

            if (!string.IsNullOrWhiteSpace(configText))
            {
                if (Enum.TryParse(configText, out LogLevel logLevelVal))
                {
                    retVal = logLevelVal;
                }

            }

            return retVal;
        }

        private SmsSenderType GetSmsSenderType(IConfiguration config)
        {
            SmsSenderType retVal = SmsSenderType.Twilio;

            string configText = config[DEFAULTSMSSENDERCONFIG];

            if (!string.IsNullOrWhiteSpace(configText))
            {
                if (Enum.TryParse(configText, out SmsSenderType senderTypeVal))
                {
                    retVal = senderTypeVal;
                }

            }

            return retVal;
        }


        protected abstract void ConfigureServices(IServiceCollection services, IConfiguration config,
            BootstrapConfig bootConfig);

    }
}
