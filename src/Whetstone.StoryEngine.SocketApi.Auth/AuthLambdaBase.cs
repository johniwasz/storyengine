using Amazon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Diagnostics;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.SocketApi
{
    /// <summary>
    /// This is for use by the client-facing lambda components to
    /// speed up boot time.
    /// </summary>
    /// <remarks>Rather than reading from the bootstrap config parameter,
    /// it uses environment variables.</remarks>
    public abstract class AuthLambdaBase
    {
        public const string AUTHTYPECONFIG = "AUTHTYPE";
        public const string AUTHPOOLID = "AUTHPOOLID";
        public const string AUTHCLIENTID = "AUTHCLIENTID";
        public const string LOGLEVELCONFIG = "LOGLEVEL";


        protected Stopwatch _coldStartTimer = new Stopwatch();
        public IServiceProvider Services { get; set; }

        public IConfiguration Configuration { get; set; }

        public static RegionEndpoint CurrentRegion { get; private set; }

        protected AuthLambdaBase()
        {

            _coldStartTimer.Start();
            Console.WriteLine("Entering AuthLambda constructor");
            Stopwatch configWatch = new Stopwatch();
            configWatch.Start();
            Configuration = Bootstrapping.BuildConfiguration(false);

            configWatch.Stop();
            Console.WriteLine($"Config load time is {configWatch.ElapsedMilliseconds}ms");

            Stopwatch bootstrapTime = new Stopwatch();
            bootstrapTime.Start();
            IServiceCollection services = new ServiceCollection();

            BootstrapConfig bootConfig = new BootstrapConfig()
            {
                LogLevel = GetLogLevel(Configuration),
                Security = GetSecurityConfig(Configuration)
            };

            this.ConfigureServices(services, Configuration, bootConfig);


            this.Services = services.BuildServiceProvider();
            bootstrapTime.Stop();

            Console.WriteLine($"Services configuration time is {bootstrapTime.ElapsedMilliseconds}ms");

            Console.WriteLine("Exiting AuthLambdaBase constructor");

        }

        protected virtual void ConfigureServices(IServiceCollection services, IConfiguration config,
            BootstrapConfig bootConfig)
        {
            Stopwatch configServiceTimer = new Stopwatch();
            configServiceTimer.Start();

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
                .AddFilter<SerilogLoggerProvider>(level => level >= bootConfig.LogLevel.GetValueOrDefault(LogLevel.Error)));

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

        private SecurityConfig GetSecurityConfig(IConfiguration config)
        {
            Console.WriteLine($"Getting cognito environment parameters");
            string authTypeName = config[AUTHTYPECONFIG];
            SecurityConfig secConfig = null;

            if (!String.IsNullOrEmpty(authTypeName))
            {
                if (Enum.IsDefined(typeof(AuthenticatorType), authTypeName))
                {
                    AuthenticatorType authType = (AuthenticatorType)Enum.Parse(typeof(AuthenticatorType), authTypeName);

                    if (authType == AuthenticatorType.Cognito)
                    {
                        secConfig = new SecurityConfig()
                        {
                            AuthenticatorType = authType,
                            Cognito = new CognitoConfig()
                            {
                                UserPoolId = config[AUTHPOOLID],
                                UserPoolClientId = config[AUTHCLIENTID]
                            }
                        };
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unsupported AuthenticatorType: {authType}");
                    }

                }
                else
                {
                    throw new InvalidOperationException($"Unknown AuthenticatorType Name: {authTypeName}");
                }

            }
            else
            {
                throw new InvalidOperationException("AUTHTYPE not set!");
            }

            if (String.IsNullOrEmpty(secConfig.Cognito.UserPoolId))
                throw new InvalidOperationException("AUTHPOOLID must have a value");

            if (String.IsNullOrEmpty(secConfig.Cognito.UserPoolClientId))
                throw new InvalidOperationException("AUTHCLIENTID must have a value");

            return secConfig;
        }


    }
}
