using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whetstone.StoryEngine.AlexaProcessor;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.Amazon;
using Whetstone.StoryEngine.Data.DependencyInjection;
using Whetstone.StoryEngine.Data.EntityFramework;
using Whetstone.StoryEngine.Data.EntityFramework.EntityManager;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Google.Management;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Reporting.Models;
using Whetstone.StoryEngine.Reporting.ReportRepository;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.Repository.Amazon;
using Whetstone.StoryEngine.Repository.Messaging;
using Whetstone.StoryEngine.Repository.Twitter;
using Whetstone.StoryEngine.Security;
using Whetstone.StoryEngine.Security.Claims;

namespace Whetstone.StoryEngine.WebLibrary
{
    public static class CoreApiExtensions
    {

        public static void UseStoryEngineServices(this IServiceCollection services, IConfiguration config, BootstrapConfig bootConfig)
        {
            services.AddTransient<UserDataRepository>();

            services.AddSingleton<Func<UserRepositoryType, IStoryUserRepository>>(serviceProvider => handlerKey =>
            {
                IStoryUserRepository retUserRep = null;
                switch (handlerKey)
                {
                    case UserRepositoryType.Database:
                        retUserRep = serviceProvider.GetService<UserDataRepository>();
                        break;
                    case UserRepositoryType.DynamoDB:
                        retUserRep = serviceProvider.GetService<DynamoDBUserRepository>();
                        break;
                    default:
                        throw new KeyNotFoundException(); // or maybe return null, up to you
                }

                return retUserRep;
            });


            services.Configure<ReportGeneratorConfig>(x =>
            {
                x.ReportStepFunctionArn = bootConfig.ReportStepFunction;
                x.ReportBucket = bootConfig.ReportBucket;
            });


            Bootstrapping.ConfigureServices(services, config, bootConfig);
            DataBootstrapping.ConfigureDatabaseService(services, bootConfig.DatabaseSettings);

            services.AddTransient<IReportRequestReceiver, ReportRequestReceiver>();

            services.AddTransient<IMessageReporter, DataMessageReporter>();

            services.AddTransient<ITitleRepository, DataTitleRepository>();

            services.AddTransient<IStoryVersionRepository, DataTitleVersionRepository>();

            services.AddTransient<IAlexaIntentExporter, AlexaIntentExporter>();

            services.AddTransient<IDialogFlowManager, DialogFlowManager>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddTransient<IMediaStreamer, S3MediaStreamer>();

            services.AddTransient<ITitleValidator, TitleValidator>();

            services.AddTransient<IFileReader, S3FileReader>();

            services.AddTransient<IProjectRepository, ProjectRepository>();

            services.AddTransient<IFileRepository, S3FileStore>();

            services.AddSingleton<IAuthorizationService, EngineAuthorization>();

            services.AddSingleton<IOrganizationRepository, OrganizationRepository>();

            services.AddSingleton<IWebHookManager, WebHookManager>();

            services.AddAuthorizationCore(options =>
            {
                options.AddPolicy(FunctionalEntitlements.PermissionViewProject, policy =>
                {
                    policy.RequireClaim(SoniBridgeClaimTypes.Permission,
                            FunctionalEntitlements.PermissionViewProject);
                    policy.RequireClaim(ClaimTypes.Sid);
                });
                
                options.AddPolicy(FunctionalEntitlements.PermissionViewVersion, 
                    policy =>
                    {
                        policy.RequireClaim(SoniBridgeClaimTypes.Permission,
                            FunctionalEntitlements.PermissionViewVersion);

                        policy.RequireClaim(ClaimTypes.Sid);
                    });

                options.AddPolicy(FunctionalEntitlements.IsRegisteredUser, policy => policy.RequireClaim(ClaimTypes.Sid));
            });

            if (bootConfig.Security.AuthenticatorType.GetValueOrDefault(AuthenticatorType.Cognito) ==
                AuthenticatorType.Cognito)
            {
                CognitoConfig cogConfig = bootConfig.Security.Cognito;

                services.AddCognitoAuthentication(cogConfig);
            }
        }

    }
}
