using Autofac.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data.Caching;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Repository.Amazon;
using Whetstone.StoryEngine.Repository;
using Microsoft.Extensions.DependencyInjection;
using Whetstone.StoryEngine.Data.Yaml;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Repository.Actions;

namespace Whetstone.StoryEngine.DependencyInjection
{
    public static class FunctionExtensions
    {


        public static void ConfigureStoryEngine(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureAppConfiguration((hostingContext, configuration) =>
            {

                configuration.Sources.Clear();
                configuration.AddEnvironmentVariables();
            });

            hostBuilder.ConfigureServices(services =>
            {

                services.AddTransient<ISecretStoreReader, SecretStoreReader>();

                services.AddScoped<IStoryRequestProcessor, StoryRequestProcessor>();

                services.AddSingleton<ISessionStoreManager, SessionStoreManager>();

                services.AddTransient<ITitleCacheRepository, TitleCacheRepository>();
                services.AddTransient<ITitleCacheRepository, TitleCacheRepository>();

                services.AddTransient<IAppMappingReader, CacheAppMappingReader>();

                services.AddSingleton<ISkillCache, SkillCache>();

                // This is intended to be the same across a single request for the service.
                services.AddTransient<ITitleReader, YamlTitleReader>();

                services.AddActionProcessors();

                services.AddTransient<IMediaLinker, S3MediaLinker>();

            });

            //BootstrapConfig bootConfig = new BootstrapConfig
            //{
            //    Bucket = Configuration[STORYBUCKETCONFIG],
            //    CacheConfig = GetCacheConfig(Configuration),
            //    SessionLoggerType = SessionLoggerType.Queue,
            //    LogLevel = GetLogLevel(Configuration),
            //    DynamoDBTables = GetDynamoDBTablesConfig(Configuration),
            //    SmsConfig = new SmsConfig
            //    {
            //        SmsHandlerType = SmsHandlerType.StepFunctionSender,
            //        NotificationStepFunctionArn = Configuration[MESSAGESTEPFUNCTIONCONFIG],
            //        SmsSenderType = GetSmsSenderType(Configuration),
            //        SourceNumber = Configuration[DEFAULTSMSSOURCENUMBERCONFIG]
            //    },
            //    SessionAuditQueue = Configuration[SESSIONAUDITURLCONFIG],
            //    EnforceAlexaPolicy = true,
            //    RecordMessages = false
            //};


        }


    }
}
