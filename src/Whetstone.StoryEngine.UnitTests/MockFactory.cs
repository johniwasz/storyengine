using Amazon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.StoryEngine;
using Whetstone.StoryEngine.AlexaProcessor;
using Whetstone.StoryEngine.AlexaProcessor.Configuration;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.EntityFramework;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Conditions;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Story.Text;
using Whetstone.StoryEngine.Models.Tracking;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.Repository.Actions;
using Whetstone.StoryEngine.Repository.Amazon;

namespace Whetstone.UnitTests
{
    internal delegate void ProcessBatchRequest(OutboundBatchRecord message);

    internal delegate void ProcessNotificationRequest(INotificationRequest notificationRequest);

    internal delegate void ProcessSessionLog(RequestRecordMessage requestMsg);

    internal class MockFactory
    {

        internal MockFactory()
        {

            System.Environment.SetEnvironmentVariable("AWS_XRAY_CONTEXT_MISSING", "LOG_ERROR");


        }
        internal ProcessBatchRequest ProcessSmsBatchFunc { get; set; }


        internal ProcessSessionLog ProcessSessionLogFunc { get; set; }

        internal ProcessNotificationRequest ProcessNotificationFunc { get; set; }

        internal ProcessSessionLog ProcessWhetstoneQueueFunc { get; set; }

        internal ISessionLogger GetMockSessionLogger()
        {

            var sessionLoggerMock = new Mock<ISessionLogger>();

            sessionLoggerMock.Setup(x => x.LogRequestAsync(It.IsAny<StoryRequest>(), It.IsAny<StoryResponse>()))
                .Callback((StoryRequest req, StoryResponse resp) =>
                {
                    RequestRecordMessage recMsg = new RequestRecordMessage();

                    List<string> responseTextList = resp.LocalizedResponse.GeneratedTextResponse.CleanTextList();
                    string returnText = string.Empty;
                    if (responseTextList != null)
                    {
                        foreach (string responseText in responseTextList)
                            returnText = string.Concat(returnText, responseText);

                    }

                    recMsg.ResponseBodyText = returnText;
                    recMsg.EngineErrorText = resp.EngineErrorText;
                    recMsg.DeploymentId = req.SessionContext.TitleVersion.DeploymentId.GetValueOrDefault();
                    recMsg.EngineRequestId = req.EngineRequestId;
                    recMsg.IntentName = req.Intent;
                    recMsg.RequestType = req.RequestType;
                    recMsg.UserId = req.UserId;

                    ProcessSessionLogFunc?.Invoke(recMsg);

                }).Returns(Task.FromResult(0));


            return sessionLoggerMock.Object;

        }

        internal ILogger<T> GetLogger<T>()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                       .AddConsole();
            });


            return loggerFactory.CreateLogger<T>();

        }

        internal Microsoft.Extensions.Logging.ILogger GetLogger()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddConsole();
            });


            return loggerFactory.CreateLogger("Generic");

        }

        internal IAppMappingReader GetMockAppMappingReader()
        {
            var appMappingMock = new Mock<IAppMappingReader>();

            appMappingMock.Setup(x => x.GetTitleAsync(Client.GoogleHome, "animalfarmpi", null))
                .ReturnsAsync(new TitleVersion("animalfarmpi", "1.5"));



            return appMappingMock.Object;
        }

        internal static IOptionsSnapshot<AuditClientMessagesConfig> GetAuditOptions()
        {
            var auditMock = new Mock<IOptionsSnapshot<AuditClientMessagesConfig>>();


            auditMock.Setup(x => x.Get(It.IsAny<string>()))
                .Returns(new AuditClientMessagesConfig() { AuditClientMessages = true });


            return auditMock.Object;
        }

        internal IStoryUserRepository GetStoryUserRepository()
        {
            var storyUserRepMock = new Mock<IStoryUserRepository>();

            Dictionary<string, DataTitleClientUser> userStore = new Dictionary<string, DataTitleClientUser>();


            storyUserRepMock.Setup(x => x.BootstrapUser(It.IsAny<StoryRequest>()))
                .Returns((StoryRequest request) =>
                {
                    DateTime curTime = DateTime.UtcNow;
                    DataTitleClientUser user = new DataTitleClientUser
                    {
                        TitleId = request.SessionContext.TitleVersion.TitleId.GetValueOrDefault(),

                        UserId = request.UserId,
                        Client = request.Client,
                        LastAccessedDate = curTime,
                        CreatedTime = curTime,
                        Locale = request.Locale,
                        IsNew = true,
                        TitleState = new List<IStoryCrumb>(),
                        PermanentTitleState = new List<IStoryCrumb>()
                    };

                    return user;
                });


            storyUserRepMock.Setup(x => x.GetUserAsync(It.IsAny<StoryRequest>()))
               .ReturnsAsync(
               (StoryRequest request) =>
               {
                   DataTitleClientUser user = null;
                   if (!string.IsNullOrWhiteSpace(request.UserId) && userStore.ContainsKey(request.UserId))
                   {
                       user = userStore[request.UserId];
                   }
                   else if (request.IsGuest.GetValueOrDefault(false) && userStore.ContainsKey(request.SessionId))
                   {
                       user = userStore[request.SessionId];
                   }
                   else
                   {
                       DateTime curTime = DateTime.UtcNow;
                       user = new DataTitleClientUser
                       {
                           Id = Guid.NewGuid(),
                           TitleId = request.SessionContext.TitleVersion.TitleId.GetValueOrDefault(),
                           UserId = string.IsNullOrWhiteSpace(request.UserId) ? Guid.NewGuid().ToString() : request.UserId,
                           Client = request.Client,
                           LastAccessedDate = curTime,
                           CreatedTime = curTime,
                           Locale = request.Locale,
                           IsNew = true,
                           TitleState = new List<IStoryCrumb>(),
                           PermanentTitleState = new List<IStoryCrumb>()
                       };
                       userStore.Add(user.UserId, user);
                   }
                   return user;
               });


            storyUserRepMock.Setup(x => x.SaveUserAsync(It.IsAny<DataTitleClientUser>()))
                .Callback(
                (DataTitleClientUser user) =>
                 {
                     if (userStore.ContainsKey(user.UserId))
                     {
                         userStore[user.UserId] = user;
                     }
                     else
                     {
                         userStore.Add(user.UserId, user);
                     }
                 }
                ).Returns(Task.FromResult(0));


            return storyUserRepMock.Object;

        }


        internal static ITitleReader GetTitleReader(StoryTitle title)
        {
            var titleMock = new Mock<ITitleReader>();

            titleMock.Setup(x => x.GetPhoneInfoAsync(It.IsAny<TitleVersion>()))
                .ReturnsAsync((TitleVersion titleId) =>
                {
                    return title.PhoneInfo;
                });

            titleMock.Setup(x => x.GetIntentsAsync(It.IsAny<TitleVersion>()))
                .ReturnsAsync((TitleVersion titleId) =>
                {
                    return title.Intents;
                });

            titleMock.Setup(x => x.GetByIdAsync(It.IsAny<TitleVersion>()))
                .ReturnsAsync((TitleVersion titleId) =>
                {
                    return title;
                });

            titleMock.Setup(x => x.GetNodeByNameAsync(It.IsAny<TitleVersion>(), It.IsAny<string>()))
                .ReturnsAsync((TitleVersion titleVer, string nodeName) =>
                {
                    StoryNode retNode = title.Nodes.FirstOrDefault(x => x.Name.Equals(nodeName));

                    return retNode;
                });


            titleMock.Setup(x => x.GetIntentByNameAsync(It.IsAny<TitleVersion>(), It.IsAny<string>()))
                .ReturnsAsync((TitleVersion titleId, string intentName) =>
                {
                    Intent returnIntent = null;

                    if ((title.Intents?.Any()).GetValueOrDefault(false))
                    {
                        returnIntent = title.Intents.FirstOrDefault(x => x.Name.Equals(intentName, StringComparison.OrdinalIgnoreCase));
                    }

                    return returnIntent;

                });


            titleMock.Setup(x => x.GetStoryConditionAsync(It.IsAny<TitleVersion>(), It.IsAny<string>()))
                    .ReturnsAsync((TitleVersion titleId, string condName) =>
                    {
                        StoryConditionBase storyCond = null;

                        if ((title.Intents?.Any()).GetValueOrDefault(false))
                        {
                            storyCond = title.Conditions.FirstOrDefault(x => x.Name.Equals(condName, StringComparison.OrdinalIgnoreCase));
                        }

                        return storyCond;

                    });


            titleMock.Setup(x => x.GetStoryConditionsAsync(It.IsAny<TitleVersion>()))
                    .ReturnsAsync((TitleVersion titleId) =>
                    {

                        return title.Conditions;

                    });

            titleMock.Setup(x => x.IsPrivacyLoggingEnabledAsync(It.IsAny<TitleVersion>()))
                .ReturnsAsync((TitleVersion titleId) =>
                {

                    return title.EnablePrivacyLogging.GetValueOrDefault(false);

                });


            titleMock.Setup(x => x.GetStartNodeNameAsync(It.IsAny<TitleVersion>(), It.IsAny<bool>()))
                .ReturnsAsync((TitleVersion titleId, bool isNewUser) =>
                {
                    string retNodeName;

                    if (isNewUser)
                    {
                        retNodeName = title.NewUserNodeName;
                    }
                    else
                    {
                        retNodeName = title.ReturningUserNodeName;
                    }

                    return retNodeName;
                });

            titleMock.Setup(x => x.GetBadIntentNodeAsync(It.IsAny<TitleVersion>(), It.IsAny<int>()))
                .ReturnsAsync((TitleVersion titleId, int badIntentCount) =>
                    {

                        if (title.BadIntentResponses != null)
                        {
                            if (title.BadIntentResponses.Count > 0)
                                return title.BadIntentResponses[0];
                        }

                        return null;
                    });

            titleMock.Setup(x => x.GetSlotTypes(It.IsAny<TitleVersion>()))
                .ReturnsAsync((TitleVersion titleId) => { return title.Slots; });


            return titleMock.Object;
        }


        internal ISmsHandler GetSmsHandler(bool allSent)
        {
            var smsHandler = new Mock<ISmsHandler>();


            smsHandler.Setup(x => x.SendOutboundSmsMessagesAsync(It.IsAny<OutboundBatchRecord>()))
                .ReturnsAsync((OutboundBatchRecord outboundBatch) =>
                {
                    outboundBatch.AllSent = allSent;
                    ProcessSmsBatchFunc?.Invoke(outboundBatch);
                    return outboundBatch;

                });


            return smsHandler.Object;

        }


        internal static ITitleReader GetNewUserTitle(StoryRequest req, string newUserNodeName)
        {

            var titleReaderMock = new Mock<ITitleReader>();

            string titleId = req.SessionContext.TitleVersion.ShortName;

            _ = titleReaderMock.Setup(x => x.GetByIdAsync(It.IsAny<TitleVersion>()))
            .ReturnsAsync((TitleVersion titleVer) =>
            {
                StoryTitle title = new StoryTitle
                {
                    Title = titleVer.ShortName,
                    NewUserNodeName = newUserNodeName
                };
                return title;

            });

            titleReaderMock.Setup(x => x.GetNodeByNameAsync(It.IsAny<TitleVersion>(), newUserNodeName))
            .ReturnsAsync((TitleVersion titleVer, string nodeName) =>
            {
                StoryNode newUserNode = new StoryNode
                {
                    Name = nodeName,
                    ResponseSet = new List<LocalizedResponseSet>()
                };

                LocalizedResponse resp = new LocalizedResponse
                {
                    TextFragments = new List<TextFragmentBase>() { new SimpleTextFragment("This a new user response") }
                };

                LocalizedResponseSet locRespSet = new LocalizedResponseSet
                {
                    LocalizedResponses = new List<LocalizedResponse>()
                };
                locRespSet.LocalizedResponses.Add(resp);

                newUserNode.ResponseSet = new List<LocalizedResponseSet>() { locRespSet };

                return newUserNode;

            });


            return titleReaderMock.Object;
        }



        internal static StoryTitle LoadStoryTitle(TitleVersion titleVer)
        {
            string titlePath = $"TitleFiles/{titleVer.ShortName}/{titleVer.Version}/{titleVer.ShortName}.yaml";

            string text = File.ReadAllText(titlePath);

            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();
            StoryTitle retTitle = yamlDeserializer.Deserialize<StoryTitle>(text);

            return retTitle;
        }


        internal static StoryTitle CreateStoryTitle(string titleName)
        {
            StoryTitle title = new StoryTitle
            {
                Title = titleName,
                PhoneInfo = new StoryPhoneInfo()
            };
            title.PhoneInfo.SourcePhone = "+12157099492";

            return title;
        }

        internal IServiceCollection InitServiceCollection(TitleVersion titleVer)
        {

            StoryTitle loadedTitle = LoadStoryTitle(titleVer);
            IServiceCollection servCol = BuildServiceCollection(loadedTitle, titleVer);
            return servCol;
        }


        internal IServiceCollection InitServiceCollection(string titleId)
        {
            StoryTitle title = CreateStoryTitle(titleId);

            TitleVersion titleVer = new TitleVersion();
            titleVer.ShortName = title.Title;
            titleVer.VersionId = Guid.NewGuid();
            titleVer.TitleId = Guid.NewGuid();
            titleVer.DeploymentId = Guid.NewGuid();

            IServiceCollection servCol = BuildServiceCollection(title, titleVer);
            return servCol;
        }

        internal IServiceCollection InitServiceCollection()
        {
            IServiceCollection servCol = BuildServiceCollection();
            return servCol;
        }


        private IServiceCollection BuildServiceCollection()
        {
            return BuildServiceCollection(null, null);

        }

        private IServiceCollection BuildServiceCollection(StoryTitle title, TitleVersion titleVer)
        {

            IServiceCollection services = new ServiceCollection();
            if (title != null)
            {
                ITitleReader reader = MockFactory.GetTitleReader(title);
                services.AddTransient<ITitleReader>(x => reader);
            }

            if (titleVer != null)
            {
                var appMappingMock = new Mock<IAppMappingReader>();

                appMappingMock.Setup(x => x.GetTitleAsync(It.IsAny<Client>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync((Client clientType, string appId, string alias) =>
                    {
                        return titleVer;
                    });


                services.AddTransient<IAppMappingReader>(x => appMappingMock.Object);
            }


            // Create Distributed Cache

            services.AddMemoryCache();
            services.AddDistributedMemoryCache();


            var providers = new LoggerProviderCollection();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .WriteTo.Debug()
                .WriteTo.Providers(providers)
                .CreateLogger();



            services.AddLogging(builder => builder
               .AddSerilog(Log.Logger)
                .AddFilter<SerilogLoggerProvider>("Microsoft", LogLevel.Error)
                .AddFilter<SerilogLoggerProvider>(level => level >= LogLevel.Trace)

            ); ;


            services.Configure<EnvironmentConfig>(options =>
            {
                options.Region = RegionEndpoint.USEast1;
                options.BucketName = "unittestbucket";
            });

            //string dbCon = GetEncryptedValue(@"/storyengine/dev/enginedb");


            string dbCon = GetEncryptedValue();





            services.Configure<SessionAuditConfig>(options =>
            {
                options.SessionAuditQueue = "sqsauditqueue";
            });

            services.Configure<AuditClientMessagesConfig>(options =>
                {
                    options.AuditClientMessages = true;
                });

            services.Configure<AlexaConfig>(options =>
            {
                options.AuditClientMessages = false;
                options.EnforceAlexaPolicyCheck = false;


            });

            services.AddTransient<IAlexaRequestProcessor, AlexaRequestProcessor>();

            IStoryUserRepository storyUserRep = GetStoryUserRepository();


            services.AddSingleton<Func<UserRepositoryType, IStoryUserRepository>>(dispatcherFunc => key =>
            {
                return storyUserRep;
            });

            services.AddSingleton<Func<string, DbContextOptions<UserDataContext>>>(key =>
            {

                DbContextOptionsBuilder<UserDataContext> contextBuilder = new DbContextOptionsBuilder<UserDataContext>();

                contextBuilder.EnableSensitiveDataLogging();
                contextBuilder.UseNpgsql(dbCon,
                     b =>
                     {
                         b.EnableRetryOnFailure();
                     });

                return contextBuilder.Options;
            });

            services.Configure<SmsStepFunctionHandlerConfig>(options =>
            {
                options.ResourceName = "arn:aws:states:us-east-1:940085449815:stateMachine:SaveMessageTestFunctions";

            });


            var mockStepSender = new Mock<IStepFunctionSender>();

            services.AddTransient<IStepFunctionSender>(x => mockStepSender.Object);


            services.AddSingleton<ISessionLogger>(x => GetMockSessionLogger());

            services.AddSingleton<Func<string, ISmsHandler>>(serviceProvider => key =>
            {
                ISmsHandler smsHandler = this.GetSmsHandler(true);

                return smsHandler;
            });

            services.AddSingleton<Func<SmsHandlerType, ISmsHandler>>(serviceProvider => key =>
            {
                ISmsHandler smsHandler = null;

                switch (key)
                {
                    case SmsHandlerType.DirectSender:
                        //smsHandler = serviceProvider.GetService<SmsDirectSendHandler>();

                        smsHandler = this.GetSmsHandler(true);
                        break;
                    case SmsHandlerType.StepFunctionSender:


                        smsHandler = this.GetSmsHandler(true);

                        // smsHandler = serviceProvider.GetService<SmsStepFunctionHandler>();
                        break;
                    default:
                        throw new KeyNotFoundException(); // or maybe return null, up to you
                }

                return smsHandler;
            });

            services.AddTransient<IStoryRequestProcessor, StoryRequestProcessor>();

            services.AddTransient<IMediaLinker, S3MediaLinker>();

            var skillCacheMock = new Mock<ISkillCache>();
            ISkillCache skillCacheRep = skillCacheMock.Object;
            services.AddTransient<ISkillCache>(x => skillCacheRep);

            ISessionStoreManager sessionStoreRep = GetSessionStoreManager();
            services.AddTransient<ISessionStoreManager>(x => sessionStoreRep);

            services.Configure<MessagingConfig>(options =>
            {
                options.MessageSendDelayInterval = 0;
                options.ThrottleRetryLimit = 3;
            });


            // string twilioKeyEntry = GetEncryptedValue("/storyengine/dev/twiliokeys");

            string twilioKeyEntry = GetEncryptedValue();


            services.Configure<TwilioConfig>(options =>
            {
                options.StatusCallbackUrl = "https://reeaf6p08d.execute-api.us-east-1.amazonaws.com/Prod/v1/smsmessagestatus";
                options.LiveCredentials = twilioKeyEntry;
                options.TestCredentials = twilioKeyEntry;
            });

            services.AddActionProcessors();

            return services;
        }

        internal static ISessionStoreManager GetSessionStoreManager()
        {
            var sessionStoreMock = new Mock<ISessionStoreManager>();

            Dictionary<string, int> badIntentCountStore = new Dictionary<string, int>();

            Dictionary<string, SessionStartType> startTypeStore = new Dictionary<string, SessionStartType>();

            sessionStoreMock.Setup(x => x.GetBadIntentCounterAsync(It.IsAny<StoryRequest>()))
                .ReturnsAsync((StoryRequest req) =>
                {
                    int badIntentCount = 0;
                    string sessionId = req.SessionId;
                    if (badIntentCountStore.ContainsKey(sessionId))
                    {
                        badIntentCount = badIntentCountStore[sessionId];
                    }

                    return badIntentCount;
                });



            sessionStoreMock.Setup(x => x.IncrementBadIntentCounterAsync(It.IsAny<StoryRequest>()))
                .ReturnsAsync((StoryRequest req) =>
                {
                    int badIntentCount = 1;
                    string sessionId = req.SessionId;
                    if (badIntentCountStore.ContainsKey(sessionId))
                    {
                        badIntentCount = badIntentCountStore[sessionId];
                        badIntentCount++;
                        badIntentCountStore[sessionId] = badIntentCount;
                    }
                    else
                    {
                        badIntentCountStore.Add(sessionId, badIntentCount);
                    }

                    return badIntentCount;
                });


            sessionStoreMock.Setup(x => x.ResetBadIntentCounterAsync(It.IsAny<StoryRequest>()))
               .Callback((StoryRequest req) =>
                {

                    string sessionId = req.SessionId;
                    if (badIntentCountStore.ContainsKey(sessionId))
                    {

                        badIntentCountStore[sessionId] = 0;
                    }
                    else
                    {
                        badIntentCountStore.Add(sessionId, 0);
                    }

                }).Returns(Task.FromResult(0));


            sessionStoreMock.Setup(x => x.SaveSessionStartTypeAsync(It.IsAny<StoryRequest>()))
                   .ReturnsAsync((StoryRequest req) =>
                   {


                       SessionStartType retType = SessionStartType.IntentStart;

                       if (req.IsNewSession.GetValueOrDefault(false))
                       {
                           retType = string.IsNullOrWhiteSpace(req.Intent) ? SessionStartType.LaunchStart : SessionStartType.IntentStart;

                       }
                       else
                       {
                           retType = SessionStartType.LaunchStart;

                       }

                       string sessionId = req.SessionId;

                       if (startTypeStore.ContainsKey(sessionId))
                       {
                           startTypeStore[sessionId] = retType;

                       }
                       else
                       {
                           startTypeStore.Add(sessionId, retType);
                       }

                       return retType;
                   });



            sessionStoreMock.Setup(x => x.GetSessionStartTypeAsync(It.IsAny<StoryRequest>()))
                   .ReturnsAsync((StoryRequest req) =>
                   {
                       SessionStartType retType;

                       string sessionId = req.SessionId;

                       if (startTypeStore.ContainsKey(sessionId))
                       {
                           retType = startTypeStore[sessionId];
                       }
                       else
                       {
                           retType = req.RequestType != StoryRequestType.Launch && req.IsNewSession.GetValueOrDefault(false) ?
                            SessionStartType.IntentStart :
                            SessionStartType.LaunchStart;
                       }

                       return retType;
                   });


            sessionStoreMock.Setup(x => x.ClearSessionCacheAsync(It.IsAny<StoryRequest>()))
            .Callback((StoryRequest req) =>
            {

                string sessionId = req.SessionId;
                if (badIntentCountStore.ContainsKey(sessionId))
                {
                    badIntentCountStore.Remove(sessionId);
                }


            }).Returns(Task.FromResult(0));


            return sessionStoreMock.Object;
        }



        internal string GetEncryptedValue()
        {
            string paramValue = null;

            paramValue = "DBKEVALUE";

            return paramValue;


        }


    }
}
