using Amazon.Lambda.Core;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Strategies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Cache;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.Caching;
using Whetstone.StoryEngine.Data.EntityFramework;
using Whetstone.StoryEngine.Data.EntityFramework.EntityManager;
using Whetstone.StoryEngine.MessageSender;
using Whetstone.StoryEngine.MessageSender.SaveMessageTask;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.Repository.Messaging;
using Whetstone.StoryEngine.Repository.Phone;
using Xunit;

namespace Whetstone.StoryEngine.Test
{
    public class EngineTests : TestServerFixture
    {



#pragma warning disable xUnit1013 // Public method should be marked as test
        public static Task ForEachAsync<T>(
#pragma warning restore xUnit1013 // Public method should be marked as test
            IEnumerable<T> source, int dop, Func<T, Task> body)
        {
            return Task.WhenAll(
                from partition in Partitioner.Create(source).GetPartitions(dop)
                select Task.Run(async delegate
                {
                    using (partition)
                        while (partition.MoveNext())
                            await body(partition.Current).ContinueWith(t =>
                            {
                                //  Debug.WriteLine($"Completed iterator {t}");
                                //observe exceptions
                            });

                }));
        }
        [Fact]
        public async Task SlamAnimalFarm()
        {

            List<int> loopiterator = new List<int>();

            for (int i = 0; i < 1000; i++)
                loopiterator.Add(i);

            await ForEachAsync(loopiterator, 10, x =>
            {
                Debug.WriteLine($"Starting execution {x}");

                return AnimalFarmPIEngineTest();


            });



        }

        [Fact]
        public async Task GetVersion()
        {


            IStoryVersionRepository verRepo = Services.GetRequiredService<IStoryVersionRepository>();

            IAppMappingReader appReader = Services.GetRequiredService<IAppMappingReader>();

            var appMapping = await appReader.GetTitleAsync(Client.Alexa, "amzn1.ask.skill.b46248ca-35ad-4ddf-a2f7-333578bf9029", null);

            Assert.True(appMapping.Version.Equals("0.3"));

            appMapping = await appReader.GetTitleAsync(Client.Alexa, "amzn1.ask.skill.b46248ca-35ad-4ddf-a2f7-333578bf9029", "LIVE");


            Assert.True(appMapping.Version.Equals("0.3"));


            appMapping = await appReader.GetTitleAsync(Client.Alexa, "amzn1.ask.skill.c4cabd50-2cd5-4e4c-a03c-a57d4f2a0e5f", "LIVE");

            Assert.True(appMapping.Version.Equals("0.5"));

            ITitleCacheRepository titleCacheRep = Services.GetRequiredService<ITitleCacheRepository>();

            var title = await titleCacheRep.GetStoryTitleAsync(appMapping);


        }



        [Fact]
        public async Task BasicCacheReadWriteTest()
        {
            IDistributedCache appReader = Services.GetRequiredService<IDistributedCache>();
            string text = File.ReadAllText("importfiles/animalfarmpi/prod/animalfarmpi.yaml");

            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();


            StoryTitle retTitle = yamlDeserializer.Deserialize<StoryTitle>(text);

            try
            {

                TimeSpan myTimespan = new TimeSpan(30, 0, 0);


                await appReader.SetAsync("dev", "animalfarmpi", retTitle, new DistributedCacheEntryOptions() { SlidingExpiration = myTimespan });


                StoryTitle val = await appReader.GetAsync<StoryTitle>("dev", "animalfarmpi");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }




        }


        [Fact]
        public async Task WhetstoneBixbySkillTest()
        {
            ISmsConsentRepository consentRep = Services.GetRequiredService<ISmsConsentRepository>();

            UserPhoneConsent generatedConsent = null;
            var smsHandlerMock = new Mock<INotificationDispatcher>();


            this.ServiceCollection.AddTransient<IOutboundMessageLogger, OutboundMessageDatabaseLogger>();

            smsHandlerMock.Setup(x => x.DispatchNotificationAsync(It.IsAny<INotificationRequest>()))
               .Returns(async (INotificationRequest notifReq) =>
               {
                   ILambdaContext mockContext = Mock.Of<ILambdaContext>();

                   MessageSaveFunction saveFunc = new MessageSaveFunction(this.Services);



                   MessageSenderTasksFunction senderFunc = new MessageSenderTasksFunction(this.Services);
                   var sendResult = await saveFunc.SaveNotificationRequest(notifReq, mockContext);
                   var messageResult = await senderFunc.DispatchMessageAsync(sendResult, mockContext);
               });

            this.ServiceCollection.AddSingleton<Func<NotificationsDispatchTypeEnum, INotificationDispatcher>>(serviceProvider => handlerKey =>
            {
                INotificationDispatcher retSmsHandler = smsHandlerMock.Object;
                return retSmsHandler;
            });


            var consentMockRep = new Mock<ISmsConsentRepository>();

            consentMockRep.Setup(x => x.SaveConsentAsync(It.IsAny<UserPhoneConsent>()))
                .ReturnsAsync((UserPhoneConsent phoneConsent) =>
                {
                    consentRep.SaveConsentAsync(phoneConsent);
                    generatedConsent = phoneConsent;
                    return phoneConsent;
                });


            this.ServiceCollection.AddSingleton<ISmsConsentRepository>(consentMockRep.Object);

            this.Services = this.ServiceCollection.BuildServiceProvider();

            Stopwatch totalRunTime = new Stopwatch();
            totalRunTime.Start();

            IStoryRequestProcessor requestProcessor = Services.GetRequiredService<IStoryRequestProcessor>();


            IAppMappingReader appReader = Services.GetRequiredService<IAppMappingReader>();
            ISessionLogger sessionLogger = Services.GetService<ISessionLogger>();


            string clientId = "whetstonetechnologies.whetstonetechnologies";

            Client clientType = Models.Client.Bixby;

            TitleVersion titleVer = null;

            try
            {
                titleVer = await appReader.GetTitleAsync(clientType, clientId, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }


            EngineClientContext engineContext = new EngineClientContext(titleVer, clientId, clientType, "en-US");

            StoryRequest req = null;
            StoryResponse resp = null;

            Dictionary<string, string> slotsValues = new Dictionary<string, string>();
            slotsValues.Add("whetstonename", "sonibridge");

            //
            // For Bixby, we come in with a LearnAboutWhetstone intent then the client collects a phone number out of band.
            // We pass that phone number using the special BixbyPhoneNumberIntent on the current node, which then sends us
            // down the normal confirmation to text message path.
            //

            req = engineContext.GetIntentRequest("LearnAboutWhetstone", slotsValues, StoryRequestType.Intent, null);

            // For Bixby, this is the equivalent of starting a new session
            req.IsNewSession = true;

            resp = await requestProcessor.ProcessStoryRequestAsync(req);
            await WriteResponseAsync(req, resp, clientType);

            slotsValues.Clear();
            slotsValues.Add("phonenumber", "6105551212");
            req = engineContext.GetIntentRequest("BixbyPhoneNumberIntent", slotsValues, StoryRequestType.Intent, resp.SessionContext);
            resp = await requestProcessor.ProcessStoryRequestAsync(req);
            await WriteResponseAsync(req, resp, clientType);

            req = engineContext.GetIntentRequest("YesIntent", StoryRequestType.Intent, resp.SessionContext);
            resp = await requestProcessor.ProcessStoryRequestAsync(req);
            await WriteResponseAsync(req, resp, clientType);


            totalRunTime.Stop();

            // Try to get the phone consent 

            var retrievedConsent = await consentRep.GetConsentAsync(generatedConsent.Id.Value);

            IUserContextRetriever userRet = Services.GetRequiredService<IUserContextRetriever>();

            ILogger<SmsConsentDatabaseRepository> consentLogger = Services.GetRequiredService<ILogger<SmsConsentDatabaseRepository>>();


            ISmsConsentRepository dbRepo = new SmsConsentDatabaseRepository(userRet, consentLogger);

            await dbRepo.SaveConsentAsync(retrievedConsent);

            Debug.WriteLine(totalRunTime.ElapsedMilliseconds);

        }


        [Fact]
        public async Task WhetstoneSkillTest()
        {
            ISmsConsentRepository consentRep = Services.GetRequiredService<ISmsConsentRepository>();

            UserPhoneConsent generatedConsent = null;
            var smsHandlerMock = new Mock<INotificationDispatcher>();


            this.ServiceCollection.AddTransient<IOutboundMessageLogger, OutboundMessageDatabaseLogger>();

            smsHandlerMock.Setup(x => x.DispatchNotificationAsync(It.IsAny<INotificationRequest>()))
               .Returns(async (INotificationRequest notifReq) =>
                {
                    ILambdaContext mockContext = Mock.Of<ILambdaContext>();
                    MessageSaveFunction saveFunc = new MessageSaveFunction(this.Services);
                    var sendResult = await saveFunc.SaveNotificationRequest(notifReq, mockContext);

                    MessageSenderTasksFunction senderFunc = new MessageSenderTasksFunction(this.Services);
                    var messageResult = await senderFunc.DispatchMessageAsync(sendResult, mockContext);
                });

            this.ServiceCollection.AddSingleton<Func<NotificationsDispatchTypeEnum, INotificationDispatcher>>(serviceProvider => handlerKey =>
            {
                INotificationDispatcher retSmsHandler = smsHandlerMock.Object;
                return retSmsHandler;
            });


            var consentMockRep = new Mock<ISmsConsentRepository>();

            consentMockRep.Setup(x => x.SaveConsentAsync(It.IsAny<UserPhoneConsent>()))
                .ReturnsAsync((UserPhoneConsent phoneConsent) =>
                {
                    consentRep.SaveConsentAsync(phoneConsent);
                    generatedConsent = phoneConsent;
                    return phoneConsent;
                });


            this.ServiceCollection.AddSingleton<ISmsConsentRepository>(consentMockRep.Object);

            this.Services = this.ServiceCollection.BuildServiceProvider();

            Stopwatch totalRunTime = new Stopwatch();
            totalRunTime.Start();

            IStoryRequestProcessor requestProcessor = Services.GetRequiredService<IStoryRequestProcessor>();


            IAppMappingReader appReader = Services.GetRequiredService<IAppMappingReader>();
            ISessionLogger sessionLogger = Services.GetService<ISessionLogger>();


            string clientId = "amzn1.ask.skill.c4cabd50-2cd5-4e4c-a03c-a57d4f2a0e5f";

            Client clientType = Models.Client.Alexa;

            TitleVersion titleVer = null;

            try
            {
                titleVer = await appReader.GetTitleAsync(clientType, clientId, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }


            EngineClientContext engineContext = new EngineClientContext(titleVer, clientId, clientType, "en-US");


            StoryRequest req = engineContext.GetLaunchRequest();

            StoryResponse resp = await requestProcessor.ProcessStoryRequestAsync(req);

            await WriteResponseAsync(req, resp, clientType);




            Dictionary<string, string> slotsValues = new Dictionary<string, string>();
            slotsValues.Add("whetstonename", "sonibridge");


            req = engineContext.GetIntentRequest("LearnAboutWhetstone", slotsValues, StoryRequestType.Intent, resp.SessionContext);
            resp = await requestProcessor.ProcessStoryRequestAsync(req);
            await WriteResponseAsync(req, resp, clientType);


            req = engineContext.GetIntentRequest("YesIntent", StoryRequestType.Intent, resp.SessionContext);
            resp = await requestProcessor.ProcessStoryRequestAsync(req);
            await WriteResponseAsync(req, resp, clientType);


            //req = engineContext.GetIntentRequest("YesIntent", StoryRequestType.Intent, resp.SessionContext);
            //resp = await requestProcessor.ProcessStoryRequestAsync(req);
            //await WriteResponseAsync(req, resp, clientType);


            slotsValues.Clear();
            slotsValues.Add("phonenumber", "2158852358");

            req = engineContext.GetIntentRequest("PhoneNumberIntent", slotsValues, StoryRequestType.Intent, resp.SessionContext);
            resp = await requestProcessor.ProcessStoryRequestAsync(req);
            await WriteResponseAsync(req, resp, clientType);

            req = engineContext.GetIntentRequest("YesIntent", StoryRequestType.Intent, resp.SessionContext);
            resp = await requestProcessor.ProcessStoryRequestAsync(req);
            await WriteResponseAsync(req, resp, clientType);

            slotsValues = new Dictionary<string, string>();
            slotsValues.Add("phonenumber", "2675551212");
            req = engineContext.GetIntentRequest("PhoneNumberIntent", slotsValues, StoryRequestType.Intent, resp.SessionContext);
            resp = await requestProcessor.ProcessStoryRequestAsync(req);
            await WriteResponseAsync(req, resp, clientType);

            req = engineContext.GetIntentRequest("YesIntent", StoryRequestType.Intent, resp.SessionContext);
            resp = await requestProcessor.ProcessStoryRequestAsync(req);
            await WriteResponseAsync(req, resp, clientType);


            totalRunTime.Stop();

            // Try to get the phone consent 

            var retrievedConsent = await consentRep.GetConsentAsync(generatedConsent.Id.Value);

            IUserContextRetriever userRet = Services.GetRequiredService<IUserContextRetriever>();
            ILogger<SmsConsentDatabaseRepository> consentLogger = Services.GetRequiredService<ILogger<SmsConsentDatabaseRepository>>();


            ISmsConsentRepository dbRepo = new SmsConsentDatabaseRepository(userRet, consentLogger);

            await dbRepo.SaveConsentAsync(retrievedConsent);

            Debug.WriteLine(totalRunTime.ElapsedMilliseconds);

        }

        [Fact]
        public async Task AnimalFarmPIEngineTest()
        {
            AWSXRayRecorder.Instance.ContextMissingStrategy = ContextMissingStrategy.LOG_ERROR;
            try
            {

                IStoryRequestProcessor requestProcessor = Services.GetRequiredService<IStoryRequestProcessor>();


                IAppMappingReader appReader = Services.GetRequiredService<IAppMappingReader>();


                string clientId = "amzn1.ask.skill.92304d4d-42a5-4371-9b13-97b4a79b9ad0";

                Client clientType = Models.Client.Alexa;

                ITitleCacheRepository titlecache = Services.GetService<ITitleCacheRepository>();
                IUserContextRetriever userContextRetriever = Services.GetService<IUserContextRetriever>();

                ILogger<DataAppMappingReader> appLogger = Services.GetService<ILogger<DataAppMappingReader>>();

                DataAppMappingReader dataAppMapping = new DataAppMappingReader(userContextRetriever, titlecache, appLogger);

                TitleVersion titleVer = await appReader.GetTitleAsync(clientType, clientId, null);
                //RequestRecordMessage queueMessage = null;

                //SessionAuditConfig sessionConfig = new SessionAuditConfig();
                //sessionConfig.SessionAuditQueue = "somequeuename";

                //var auditOptsMock = new Mock<IOptionsSnapshot<SessionAuditConfig>>();
                //auditOptsMock.Setup(m => m.Value).Returns(sessionConfig);
                //auditOptsMock.Setup(m => m.Get(It.IsAny<string>()))
                //    .Returns((string name) =>
                //    { 
                //        return sessionConfig;

                //    });


                //var queueMock = new Mock<IWhetstoneQueue>();

                //queueMock.Setup(x => x.AddMessageToQueueAsync<RequestRecordMessage>(It.IsAny<string>(), It.IsAny<RequestRecordMessage>()))
                //.Callback((string queueName, RequestRecordMessage requestRecord) =>
                //{
                //    queueMessage = requestRecord;

                //}).Returns(Task.FromResult(0));


                EngineClientContext engineContext = new EngineClientContext(titleVer, clientId, clientType, "it-IT");


                StoryRequest req = engineContext.GetLaunchRequest();

                StoryResponse resp = await requestProcessor.ProcessStoryRequestAsync(req);


                await WriteResponseAsync(req, resp, clientType);


                req = engineContext.GetIntentRequest("StartInvestigationIntent", StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);

                //  await dataSessionLogger.LogRequestAsync(queueMessage);
                await WriteResponseAsync(req, resp, clientType);


                req = engineContext.GetIntentRequest("DarkAndStormyIntent", StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);


                await WriteResponseAsync(req, resp, clientType);


                req = engineContext.GetIntentRequest(ReservedIntents.YesIntent.Name, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);


                req = engineContext.GetIntentRequest(ReservedIntents.NoIntent.Name, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);


                Dictionary<string, string> location = new Dictionary<string, string> { { "FarmLocations", "tractor" } };
                req = engineContext.GetIntentRequest("GoToLocationIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);


                location = new Dictionary<string, string>();
                location.Add("location", "terrence");
                req = engineContext.GetIntentRequest("GoToLocationIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);


                location = new Dictionary<string, string>();
                location.Add("character", "rat");
                req = engineContext.GetIntentRequest("TalkToIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);


                location = new Dictionary<string, string>();
                location.Add("location", "kitchen");
                req = engineContext.GetIntentRequest("GoToLocationIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);


                // End the session.
                req = engineContext.GetStopIntent();
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);



                // Relaunch the session.
                string userId = engineContext.UserId;

                engineContext = new EngineClientContext(titleVer, clientId, userId, clientType, "it-IT");

                req = engineContext.GetLaunchRequest();

                resp = await requestProcessor.ProcessStoryRequestAsync(req);


                await WriteResponseAsync(req, resp, clientType);



                // I should be able to restart the story.
                req = engineContext.GetResumeRequest();
                resp = await requestProcessor.ProcessStoryRequestAsync(req);

                await WriteResponseAsync(req, resp, clientType);

                location = new Dictionary<string, string>
            {
                { "verb", "look" },
                { "item", "fridge" }
            };
                req = engineContext.GetIntentRequest("VerbTheItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);



                location = new Dictionary<string, string>
            {
                { "verb", "try" },
                { "item", "cabbage" }
            };
                req = engineContext.GetIntentRequest("VerbTheItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);

                location = new Dictionary<string, string>
            {
                { "verb", "close" },
                { "item", "fridge" }
            };
                req = engineContext.GetIntentRequest("VerbTheItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);

                location = new Dictionary<string, string>();
                location.Add("verb", "call");
                location.Add("character", "delivery service");
                req = engineContext.GetIntentRequest("VerbTheCharacterIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);


                location = new Dictionary<string, string>();
                location.Add("location", "outside");
                req = engineContext.GetIntentRequest("GotoLocationIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);


                location = new Dictionary<string, string>();
                location.Add("location", "barn");
                req = engineContext.GetIntentRequest("GotoLocationIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);


                location = new Dictionary<string, string>();
                location.Add("location", "milfred");
                req = engineContext.GetIntentRequest("GotoLocationIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);


                location = new Dictionary<string, string>();
                location.Add("item", "record");
                req = engineContext.GetIntentRequest("TurnOffItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);

                location = new Dictionary<string, string>();
                location.Add("location", "garden");
                req = engineContext.GetIntentRequest("GoToLocationIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);


                location = new Dictionary<string, string>();
                location.Add("item", "catnip");
                req = engineContext.GetIntentRequest("TakeItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);

                location = new Dictionary<string, string>();
                location.Add("location", "barn");
                req = engineContext.GetIntentRequest("GotoLocationIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);

                location = new Dictionary<string, string>();
                location.Add("item", "mackerel");
                req = engineContext.GetIntentRequest("TakeItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);

                location = new Dictionary<string, string>();
                location.Add("item", "airhorn");
                req = engineContext.GetIntentRequest("TakeItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);

                location = new Dictionary<string, string>();
                location.Add("verb", "play");
                location.Add("item", "airhorn");
                req = engineContext.GetIntentRequest("VerbTheItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);



                location = new Dictionary<string, string>();
                location.Add("location", "barn");
                req = engineContext.GetIntentRequest("GoToLocationIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);


                location = new Dictionary<string, string>();
                location.Add("location", "pond");
                req = engineContext.GetIntentRequest("GoToLocationIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);


                location = new Dictionary<string, string>();
                location.Add("item", "bread");
                req = engineContext.GetIntentRequest("TakeItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);

                location = new Dictionary<string, string>();
                location.Add("verb", "look");
                location.Add("item", "wastebasket");
                req = engineContext.GetIntentRequest("VerbTheItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);

                location = new Dictionary<string, string>();
                location.Add("item", "bag");
                req = engineContext.GetIntentRequest("TakeItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);


                location = new Dictionary<string, string>();
                location.Add("item", "turnips");
                req = engineContext.GetIntentRequest("TakeItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);


                location = new Dictionary<string, string>();
                location.Add("item", "turnips");
                location.Add("verb", "throw");
                location.Add("character", "farmer");
                req = engineContext.GetIntentRequest("VerbTheItemAtTheCharacterIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);


                location = new Dictionary<string, string>();
                location.Add("item", "turnips");
                location.Add("verb", "throw");
                location.Add("character", "farmer");
                req = engineContext.GetIntentRequest("VerbTheItemAtTheCharacterIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);


                location = new Dictionary<string, string>();
                location.Add("verb", "feed");
                location.Add("item", "ducks");
                req = engineContext.GetIntentRequest("VerbTheItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);

                location = new Dictionary<string, string>();
                location.Add("verb", "open");
                location.Add("item", "briefcase");
                req = engineContext.GetIntentRequest("VerbTheItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);

                req = engineContext.GetIntentRequest("MakeSandwichIntent", StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);


                location = new Dictionary<string, string>();
                location.Add("item", "sandwich");
                location.Add("verb", "give");
                location.Add("character", "farmer");
                req = engineContext.GetIntentRequest("VerbTheItemToTheCharacterIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);


                location = new Dictionary<string, string>();
                location.Add("item", "sandwich");
                location.Add("verb", "give");
                location.Add("character", "terrence");
                req = engineContext.GetIntentRequest("VerbTheItemToTheCharacterIntent", location, StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);


                req = engineContext.GetIntentRequest("EndGameIntent", StoryRequestType.Intent, resp.SessionContext);
                resp = await requestProcessor.ProcessStoryRequestAsync(req);
                await WriteResponseAsync(req, resp, clientType);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
        }


    }

    [ExcludeFromCodeCoverage]
    public class EngineClientContext
    {


        public string ClientId { get; set; }


        public Client ClientType { get; set; }


        public string Locale { get; set; }

        public string SessionId { get; set; }

        public string UserId { get; set; }



        public TitleVersion TitleVersion { get; set; }

        public Guid EngineSessionId { get; set; }

        public EngineClientContext(TitleVersion titleVer, string clientId, Client clientType, string locale)
        {
            TitleVersion = titleVer;
            ClientId = clientId;
            ClientType = clientType;
            Locale = locale;
            UserId = Guid.NewGuid().ToString("N");
            SessionId = Guid.NewGuid().ToString("N");

            EngineSessionId = Guid.NewGuid();
        }

        public EngineClientContext(TitleVersion titleVer, string clientId, string userId, Client clientType, string locale)
        {
            TitleVersion = titleVer;
            ClientId = clientId;
            ClientType = clientType;
            Locale = locale;
            UserId = userId;
            SessionId = Guid.NewGuid().ToString("N");

            EngineSessionId = Guid.NewGuid();
        }


        public StoryRequest GetIntentRequest(string intent, StoryRequestType requestType, EngineSessionContext sessionContext)
        {
            return GetIntentRequest(intent, null, requestType, sessionContext);

        }




        public StoryRequest GetIntentRequest(string intent, Dictionary<string, string> slotValues, StoryRequestType requestType, EngineSessionContext sessionContext)
        {

            StoryRequest req = null;

            switch (requestType)
            {
                case StoryRequestType.Intent:
                    req = CreateIntentRequest(intent, slotValues, sessionContext);
                    break;
                case StoryRequestType.CanFulfillIntent:
                    req = CreateCanFulfillIntentRequest(intent, slotValues);
                    break;
                case StoryRequestType.Launch:
                    req = GetLaunchRequest();
                    break;
                case StoryRequestType.Resume:
                    req = GetResumeRequest();
                    break;
                case StoryRequestType.Stop:
                    req = GetStopIntent();
                    break;

            }



            return req;
        }

        internal StoryRequest GetStopIntent()
        {
            StoryRequest req = BootstrapRequest();

            req.RequestType = StoryRequestType.Stop;
            req.IsNewSession = false;

            return req;
        }

        private StoryRequest CreateCanFulfillIntentRequest(string intent, Dictionary<string, string> slotValues)
        {
            StoryRequest req = BootstrapRequest();

            req.RequestType = StoryRequestType.CanFulfillIntent;
            req.Intent = intent;
            req.Slots = slotValues;
            req.IsNewSession = false;

            return req;
        }

        private StoryRequest CreateIntentRequest(string intent, Dictionary<string, string> slotValues, EngineSessionContext sessionContext)
        {
            StoryRequest req = BootstrapRequest();


            req.RequestType = StoryRequestType.Intent;
            req.Intent = intent;
            req.Slots = slotValues;
            req.IsNewSession = false;
            if (sessionContext != null)
                req.SessionContext = sessionContext;


            return req;
        }

        public StoryRequest GetLaunchRequest()
        {
            StoryRequest req = BootstrapRequest();

            req.RequestType = StoryRequestType.Launch;
            req.IsNewSession = true;

            return req;
        }


        public StoryRequest GetResumeRequest()
        {
            StoryRequest req = BootstrapRequest();

            req.RequestType = StoryRequestType.Resume;
            req.IsNewSession = true;

            return req;
        }



        private StoryRequest BootstrapRequest()
        {

            StoryRequest req = new StoryRequest();
            req.SessionContext = new EngineSessionContext();
            req.SessionContext.TitleVersion = TitleVersion;
            req.SessionContext.EngineSessionId = EngineSessionId;
            if (ClientType == Client.Bixby)
            {
                req.UserId = SessionId;
                req.IsGuest = true;
            }
            else
            {
                req.IsGuest = false;
                req.UserId = UserId;
            }
            req.Client = ClientType;
            req.EngineRequestId = Guid.NewGuid();
            req.RequestId = Guid.NewGuid().ToString("N");
            req.SessionId = SessionId;

            req.ApplicationId = ClientId;
            req.RequestTime = DateTime.Now;
            req.Locale = Locale;
            return req;


        }
    }
}
