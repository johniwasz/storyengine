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
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Story.Text;
using Whetstone.StoryEngine.Models.Tracking;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.Repository.Actions;
using Whetstone.StoryEngine.Repository.Amazon;
using Whetstone.StoryEngine.Repository.Messaging;
using Whetstone.StoryEngine.Repository.Phone;
using Whetstone.StoryEngine.SocketApi.Repository;

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

        internal IWhetstoneQueue GetMockWhetstoneQueue()
        {

            var whetstoneQueeu = new Mock<IWhetstoneQueue>();

            whetstoneQueeu.Setup(x => x.AddMessageToQueueAsync<RequestRecordMessage>(It.IsAny<string>(), It.IsAny<RequestRecordMessage>()))
                .Callback((string queueName, RequestRecordMessage requestRecord) =>
                {
                    ProcessWhetstoneQueueFunc?.Invoke(requestRecord);

                }).Returns(Task.FromResult(0));


            return whetstoneQueeu.Object;

        }

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


        internal ISmsConsentRepository GetConsentRepository()
        {
            var consentRepo = new Mock<ISmsConsentRepository>();

            Dictionary<string, UserPhoneConsent> consentDict = new Dictionary<string, UserPhoneConsent>();

            Dictionary<string, UserPhoneConsent> crossTitleDict = new Dictionary<string, UserPhoneConsent>();




            consentRepo.Setup(x => x.SaveConsentAsync(It.IsAny<UserPhoneConsent>()))
                .ReturnsAsync((UserPhoneConsent phoneConsent) =>
                {
                    string consentName = phoneConsent.Name;

                    string phoneNumber = phoneConsent.Phone?.PhoneNumber;


                    string consentKey = $"{consentName}|{phoneConsent.PhoneId}";
                    if (consentDict.ContainsKey(consentKey))
                        consentDict[consentKey] = phoneConsent;
                    else
                        consentDict.Add(consentKey, phoneConsent);
                    if (!string.IsNullOrWhiteSpace(phoneNumber))
                    {

                        string crossTitleKey = $"{consentName}|{phoneNumber}";
                        if (crossTitleDict.ContainsKey(crossTitleKey))
                            crossTitleDict[crossTitleKey] = phoneConsent;
                        else
                            crossTitleDict.Add(crossTitleKey, phoneConsent);
                    }

                    return phoneConsent;
                });

            consentRepo.Setup(x => x.GetConsentAsync(It.IsAny<string>(), It.IsAny<Guid>()))
                .ReturnsAsync((string consentName, Guid phoneId) =>
                {
                    UserPhoneConsent retConsent = null;
                    string key = $"{consentName}|{phoneId}";
                    if (consentDict.ContainsKey(key))
                        retConsent = consentDict[key];
                    return retConsent;
                });




            return consentRepo.Object;

        }

        internal INotificationDispatcher GetNotificationDispatcher()
        {

            var notifDispatcher = new Mock<INotificationDispatcher>();

            notifDispatcher.Setup(x => x.DispatchNotificationAsync(It.IsAny<INotificationRequest>()))
                .Callback((INotificationRequest notifReq) =>
                {
                    ProcessNotificationFunc?.Invoke(notifReq);

                }).Returns(Task.FromResult(0));


            return notifDispatcher.Object;

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



        internal static IPhoneInfoRetriever GetPhoneTypeRetriever()
        {
            var phoneTypeRetriever = new Mock<IPhoneInfoRetriever>();

            phoneTypeRetriever.Setup(x => x.GetPhoneInfoAsync(It.IsAny<string>()))
                .ReturnsAsync((string phoneNumber) =>
                {
                    DataPhone retPhone = new DataPhone();

                    if (phoneNumber.Equals("+12675551212"))
                    {
                        retPhone.Id = new Guid("58862389-FA5F-481A-A7C0-F97D807FFFA6");
                        retPhone.PhoneNumber = phoneNumber;
                        retPhone.Type = PhoneTypeEnum.Mobile;
                        retPhone.CanGetSmsMessage = true;
                    }
                    else if (phoneNumber.Equals("+12158852358"))
                    {
                        retPhone.Id = new Guid("3E0CF1AB-842D-417C-9D2F-C3743A2E1064");
                        retPhone.PhoneNumber = phoneNumber;
                        retPhone.Type = PhoneTypeEnum.Landline;
                        retPhone.CanGetSmsMessage = false;
                    }
                    else
                    {
                        retPhone.Id = Guid.NewGuid();
                        retPhone.PhoneNumber = phoneNumber;
                        retPhone.Type = PhoneTypeEnum.Other;
                        retPhone.CanGetSmsMessage = false;
                    }

                    return retPhone;
                });

            return phoneTypeRetriever.Object;
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

            services.AddTransient<INotificationDispatcher>(x => Mock.Of<INotificationDispatcher>());

            services.AddSingleton<Func<NotificationsDispatchTypeEnum, INotificationDispatcher>>(dispatcherFunc => key =>
            {
                INotificationDispatcher dispatcher = Mock.Of<INotificationDispatcher>();
                return dispatcher;
            });


            IStoryUserRepository storyUserRep = GetStoryUserRepository();


            services.AddSingleton<Func<UserRepositoryType, IStoryUserRepository>>(dispatcherFunc => key =>
            {
                return storyUserRep;
            });


            _ = services.AddSingleton<Func<SessionLoggerType, ISessionLogger>>(key =>
              {
                  var auditConfigMock = new Mock<IOptionsSnapshot<SessionAuditConfig>>();

                  _ = auditConfigMock.Setup(x => x.Get(It.IsAny<string>())).Returns(


                      (string env) =>
                      {
                          SessionAuditConfig auditConfig = new SessionAuditConfig
                          {
                              SessionAuditQueue = "someurl"
                          };

                          return auditConfig;
                      });


                  IWhetstoneQueue queue = Mock.Of<IWhetstoneQueue>();

                  ILogger<SessionQueueLogger> queueConsoleLogger = Mock.Of<ILogger<SessionQueueLogger>>();


                  ISessionLogger logger = new SessionQueueLogger(auditConfigMock.Object, queue, queueConsoleLogger);

                  return logger;
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




            // Add this so we know what environment to access the queue in.
            IPhoneInfoRetriever phoneRetriever = MockFactory.GetPhoneTypeRetriever();



            services.Configure<PhoneConfig>(options =>
            {
                options.SourceSmsNumber = "+12157099492";

            });




            services.Configure<SmsStepFunctionHandlerConfig>(options =>
            {
                options.ResourceName = "arn:aws:states:us-east-1:940085449815:stateMachine:SaveMessageTestFunctions";

            });




            var mockStepSender = new Mock<IStepFunctionSender>();

            services.AddTransient<IStepFunctionSender>(x => mockStepSender.Object);


            services.AddSingleton<ISessionLogger>(x => GetMockSessionLogger());

            var consentRepo = GetConsentRepository();

            services.AddTransient<ISmsConsentRepository>(x => consentRepo);

            services.AddSingleton<Func<string, ISmsHandler>>(serviceProvider => key =>
            {
                ISmsHandler smsHandler = this.GetSmsHandler(true);

                return smsHandler;
            });


            services.AddSingleton<Func<NotificationsDispatchTypeEnum, INotificationDispatcher>>(dispatcherFunc => key =>
            {
                INotificationDispatcher dispatcher = GetNotificationDispatcher();

                return dispatcher;
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


            var smsLoggerMock = new Mock<IOutboundMessageLogger>();
            IOutboundMessageLogger outboundLogger = smsLoggerMock.Object;
            services.AddTransient<IOutboundMessageLogger>(x => outboundLogger);

            services.AddTransient<IPhoneInfoRetriever>(x => phoneRetriever);

            services.AddTransient<IStoryRequestProcessor, StoryRequestProcessor>();

            services.AddTransient<ISessionLogger, SessionQueueLogger>();

            services.AddTransient<IMediaLinker, S3MediaLinker>();




            var skillCacheMock = new Mock<ISkillCache>();
            ISkillCache skillCacheRep = skillCacheMock.Object;
            services.AddTransient<ISkillCache>(x => skillCacheRep);

            IWhetstoneQueue queueMock = GetMockWhetstoneQueue();
            services.AddTransient<IWhetstoneQueue>(x => queueMock);


            ISessionStoreManager sessionStoreRep = GetSessionStoreManager();
            services.AddTransient<ISessionStoreManager>(x => sessionStoreRep);


            var twilioVerMock = new Mock<ITwilioVerifier>();
            twilioVerMock.Setup(x => x.ValidateTwilioMessageAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            services.AddTransient<ITwilioVerifier>(x => twilioVerMock.Object);


            services.AddTransient<ISmsSender>(o =>
            {

                var smsSenderMock = new Mock<ISmsSender>();

                smsSenderMock.SetupGet(x => x.ProviderName).Returns(SmsSenderType.Twilio);

                smsSenderMock.Setup(x => x.SendSmsMessageAsync(It.IsAny<SmsMessageRequest>()))
                .ReturnsAsync((SmsMessageRequest msgReq) =>
                {
                    OutboundMessageLogEntry logEntry = new OutboundMessageLogEntry();
                    return logEntry;

                });

                return smsSenderMock.Object;

            });

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

            MockFactory.ApplySocketInterfaces(services);

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

        internal static ISocketConnectionManager GetSocketConnectionManager()
        {
            var connMgr = new Mock<ISocketConnectionManager>();

            Dictionary<string, IAuthenticatedSocket> sockets = new Dictionary<string, IAuthenticatedSocket>();

            connMgr.Setup(x => x.IsLocal).Returns(true);

            connMgr.Setup(x => x.AddSocketAsync(It.IsAny<IAuthenticatedSocket>()))
                .Callback((IAuthenticatedSocket socket) =>
                {
                    sockets.Add(socket.Id, socket);
                }).Returns(Task.FromResult(0));

            _ = connMgr.Setup(x => x.GetSocketByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((string userId, string connectionId) =>
                {
                    sockets.TryGetValue(connectionId, out IAuthenticatedSocket socket);

                    return socket;
                });

            connMgr.Setup(x => x.GetSocketsForUserIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string userId) =>
                {
                    List<IAuthenticatedSocket> lstSockets = new List<IAuthenticatedSocket>();

                    foreach (IAuthenticatedSocket socket in sockets.Values)
                    {
                        if (socket.UserId == userId)
                        {
                            lstSockets.Add(socket);
                        }
                    }

                    return lstSockets;
                });

            connMgr.Setup(x => x.RemoveSocketAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Callback((string userId, string connectionId) =>
                {
                    sockets.Remove(connectionId);
                });

            return connMgr.Object;
        }

        internal static ISocketMessageSender GetSocketMessageSender()
        {
            var sender = new Mock<ISocketMessageSender>();

            sender.Setup(x => x.SendMessageToClient(It.IsAny<IAuthenticatedSocket>(), It.IsAny<string>(), It.IsAny<KeyValuePair<string, object>[]>()))
                .Callback((IAuthenticatedSocket socket, string message, KeyValuePair<string, object>[] parameters) =>
                {
                    //Noop
                });

            return sender.Object;
        }

        internal static IWebAuthorizer GetWebAuthorizer()
        {
            var authorizer = new Mock<IWebAuthorizer>();

            authorizer.Setup(x => x.GetValidJwtFromAuthToken(It.IsAny<string>()))
                .Throws<NotImplementedException>();

            authorizer.Setup(x => x.GetValidUserIdFromAuthToken(It.IsAny<string>()))
                .Returns((string authToken) =>
                {
                    return authToken;
                });

            return authorizer.Object;
        }

        internal static void ApplySocketInterfaces(IServiceCollection services)
        {
            // Create socket support class mocks - we want singletons for the connection manager
            // and socket handler so we can do better testing.
            ISocketConnectionManager connMgr = GetSocketConnectionManager();
            services.AddSingleton<ISocketConnectionManager>(x => connMgr);

            IWebAuthorizer authorizer = GetWebAuthorizer();
            services.AddTransient<IWebAuthorizer>(x => authorizer);

            ISocketMessageSender sender = GetSocketMessageSender();
            services.AddTransient<ISocketMessageSender>(x => sender);

            // The socket handler can be the real one
            services.AddSingleton<ISocketHandler, SocketHandler>();
        }

        internal string GetEncryptedValue()
        {
            string paramValue = null;

            paramValue = "DBKEVALUE";

            return paramValue;


        }


    }
}
