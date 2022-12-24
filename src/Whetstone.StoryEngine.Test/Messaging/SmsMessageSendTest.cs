using Amazon;
using Amazon.Lambda.TestUtilities;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using MartinCostello.Testing.AwsLambdaTestServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.EntityFramework;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.MessageSender;
using Whetstone.StoryEngine.MessageSender.SaveMessageTask;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Story.Text;
using Whetstone.StoryEngine.Repository.Messaging;
using Whetstone.StoryEngine.Repository.Phone;
using Xunit;

namespace Whetstone.StoryEngine.Test.Messaging
{
    public class SmsMessageSendTest : TestServerFixture
    {

        [Fact]
        public async Task DispatchNativeCRxMessage()
        {
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.BOOTSTRAP, "/storyengine/dev/bootstrap");
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.AWSDEFAULTREGION, "us-east-1");
            System.Environment.SetEnvironmentVariable(Bootstrapping.DBUSERTYPE, "SmsUser");

            string smsTextRequest = File.ReadAllText("Messages/crxdispatch.json");
            IOutboundNotificationRecord crxDispatchMessage = JsonConvert.DeserializeObject<IOutboundNotificationRecord>(smsTextRequest);

            MessageSenderTasksFunction sendFunc = new MessageSenderTasksFunction();

            var lambdaContext = new TestLambdaContext();

            await sendFunc.DispatchMessageAsync(crxDispatchMessage, lambdaContext);

        }



        [Fact]
        public async Task DispatchNativeMessage()
        {
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.BOOTSTRAP, "/storyengine/dev/bootstrap");
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.AWSDEFAULTREGION, "us-east-1");
            System.Environment.SetEnvironmentVariable(Bootstrapping.DBUSERTYPE, "SmsUser");

            string smsTextRequest = File.ReadAllText("Messages/whetstoneoutbound.json");
            SmsNotificationRequest notifReq = JsonConvert.DeserializeObject<SmsNotificationRequest>(smsTextRequest);

            var lambdaContext = new TestLambdaContext();
            MessageSaveFunction saveFunc = new MessageSaveFunction();
            IOutboundNotificationRecord outRequest = await saveFunc.SaveNotificationRequest(notifReq, lambdaContext);


            //string dispatchText = File.ReadAllText("Messages/dispatchmessage.json");
            //IOutboundNotificationRecord outRequest = JsonConvert.DeserializeObject<IOutboundNotificationRecord>(dispatchText);
            MessageSenderTasksFunction sendFunc = new MessageSenderTasksFunction();



            await sendFunc.DispatchMessageAsync(outRequest, lambdaContext);

        }


        [Fact]
        public async Task SaveNativeMessage()
        {
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.BOOTSTRAP, "/storyengine/dev/bootstrap");
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.AWSDEFAULTREGION, "us-east-1");
            System.Environment.SetEnvironmentVariable(Bootstrapping.DBUSERTYPE, "SmsUser");
            string smsTextRequest = File.ReadAllText("Messages/outboundmessage.json");
            SmsNotificationRequest notifReq = JsonConvert.DeserializeObject<SmsNotificationRequest>(smsTextRequest);
            MessageSaveFunction saveFunc = new MessageSaveFunction();

            var lambdaContext = new TestLambdaContext();


            using (LambdaTestServer server = new LambdaTestServer())
            {
                using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(100)))
                {

                    await server.StartAsync(cancellationTokenSource.Token);


                    // string json = JsonConvert.SerializeObject(value);

                    string jsonResponse;
                    LambdaTestContext context = await server.EnqueueAsync(smsTextRequest);

                    using (var httpClient = server.CreateClient())
                    {

                        // Act
                        await MessageSaveFunction.RunAsync(httpClient, cancellationTokenSource.Token).ContinueWith(async x =>
                        {
                            // Assert
                            Assert.True(context.Response.TryRead(out LambdaTestResponse response));
                            Assert.True(response.IsSuccessful);

                            jsonResponse = await response.ReadAsStringAsync();

                            // Assert.Equal(new[] { 3, 2, 1 }, actual);
                        });

                    }
                }
            }


        }


        [Fact]
        public async Task ProcessSmsMessage()
        {

            //string smsTextRequest = File.ReadAllText("Messages/TwilioOutboundMessage.json");
            string smsTextRequest = File.ReadAllText("Messages/smsresponsemessage.json");

            SmsNotificationRequest notifReq = JsonConvert.DeserializeObject<SmsNotificationRequest>(smsTextRequest);

            IUserContextRetriever userContextRet = Services.GetRequiredService<IUserContextRetriever>();

            IPhoneInfoRetriever phoneInfoRet = Services.GetRequiredService<IPhoneInfoRetriever>();


            ISmsConsentRepository consentFunc(SmsConsentRepositoryType consentRepKey)
            {

                ISmsConsentRepository consentRep = Services.GetRequiredService<SmsConsentDatabaseRepository>();

                return consentRep;
            }

            ILogger<OutboundMessageDatabaseLogger> msgLogger = Services.GetService<ILogger<OutboundMessageDatabaseLogger>>();

            IOutboundMessageLogger dbLogger = new OutboundMessageDatabaseLogger(userContextRet, phoneInfoRet, consentFunc, msgLogger);

            await dbLogger.ProcessNotificationRequestAsync(notifReq);
        }

        [Fact]
        public async Task TestSendMessageFunctionProd()
        {


            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.BOOTSTRAP, "/storyengine/Prod/bootstrap");
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.AWSDEFAULTREGION, "us-west-2");
            System.Environment.SetEnvironmentVariable(Bootstrapping.DBUSERTYPE, "SmsUser");

            MessageSaveFunction sendFunc = new MessageSaveFunction();

            string notificationPayload = File.ReadAllText("Messages/smsCrxRequest.json");

            INotificationRequest smsPushRequest = JsonConvert.DeserializeObject<SmsNotificationRequest>(notificationPayload);

            var lambdaContext = new TestLambdaContext();

            await sendFunc.SaveNotificationRequest(smsPushRequest, lambdaContext);


        }

        [Fact]
        public async Task TestSendNotificationProd()
        {


            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.BOOTSTRAP, "/storyengine/dev/bootstrap");
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.AWSDEFAULTREGION, "us-east-1");
            System.Environment.SetEnvironmentVariable(Bootstrapping.DBUSERTYPE, "SmsUser");


            string smsTextRequest = File.ReadAllText("Messages/sampleprodnotif.json");

            string shortName = "statileansavings";
            Client userClient = Client.Alexa;

            SmsNotificationRequest notReq = JsonConvert.DeserializeObject<SmsNotificationRequest>(smsTextRequest);
            notReq.SmsProvider = SmsSenderType.Twilio;
            var servProv = GetServiceProvider();

            IPhoneInfoRetriever phoneRetriever = servProv.GetService<IPhoneInfoRetriever>();
            DataPhone phoneInfo = await phoneRetriever.GetPhoneInfoAsync("+12675551212");


            ITitleReader titleReader = servProv.GetService<ITitleReader>();


            ITitleRepository titleRep = servProv.GetService<ITitleRepository>();

            IAppMappingReader appMapper = servProv.GetService<IAppMappingReader>();

            var titleDeployments = await titleRep.GetAllTitleDeploymentsAsync();
            var titleAdmin = titleDeployments.FirstOrDefault(x => x.ShortName.Equals(shortName));

            TitleVersionAdmin firstVer = titleAdmin.Versions.FirstOrDefault();
            TitleVersionDeploymentBasic deployment = firstVer.Deployments.FirstOrDefault(x => x.ClientType == userClient);


            TitleVersion titleVer = new TitleVersion
            {
                ShortName = shortName
            };
            Guid userId = Guid.NewGuid();

            NotificationSourcePhoneMessageAction notSource = (NotificationSourcePhoneMessageAction)notReq.Source;
            notSource.Consent = new UserPhoneConsent()
            {
                IsSmsConsentGranted = true,
                TitleVersionId = titleVer.VersionId.GetValueOrDefault(Guid.NewGuid()),
                TitleClientUserId = userId,
                SmsConsentDate = DateTime.UtcNow


            };

            IStoryUserRepository userRep = servProv.GetService<IStoryUserRepository>();

            StoryRequest userStoryReq = new StoryRequest
            {
                Client = userClient,
                IsNewSession = true,
                SessionContext = new EngineSessionContext
                {
                    EngineSessionId = Guid.NewGuid(),
                    TitleVersion = new TitleVersion() { Alias = titleVer.Alias, ShortName = titleAdmin.ShortName, TitleId = titleAdmin.Id, Version = firstVer.Version, VersionId = firstVer.Id }
                },

                UserId = userId.ToString()
            };

            Guid versionId = userStoryReq.SessionContext.TitleVersion.VersionId.Value;

            //IStoryVersionRepository versionRep = servProv.GetService<IStoryVersionRepository>();
            //List<DataTitleVersionDeployment> deployments = await versionRep.GetDeploymentsAsync(versionId);


            var storyUser = await userRep.GetUserAsync(userStoryReq);


            notReq.DestinationNumberId = phoneInfo.Id;

            // Invoke the lambda function and confirm the string was upper cased.
            var context = new TestLambdaContext();

            var function = new MessageSaveFunction();

            IOutboundNotificationRecord outRecord = null;
            try
            {
                outRecord = await function.SaveNotificationRequest(notReq, context);
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }

            if (outRecord != null)
            {
                MessageSenderTasksFunction senderFunc = new MessageSenderTasksFunction();


                IOutboundNotificationRecord outRecResponse = await senderFunc.DispatchMessageAsync(outRecord, context);
            }
        }

        [Fact]
        public async Task SendSmsOutboundMessageAsync()
        {

            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.BOOTSTRAP, "/storyengine/dev/bootstrap");
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.AWSDEFAULTREGION, "us-east-1");
            System.Environment.SetEnvironmentVariable(Bootstrapping.DBUSERTYPE, "SmsUser");

            string notificationPayload = File.ReadAllText("Messages/notificationRequest.json");

            SmsNotificationRequest smsPushRequest = JsonConvert.DeserializeObject<SmsNotificationRequest>(notificationPayload);

            var servProv = GetServiceProvider();

            IOutboundMessageLogger outboundLogger = servProv.GetRequiredService<IOutboundMessageLogger>();
            await outboundLogger.ProcessNotificationRequestAsync(smsPushRequest);



        }

        [Fact]
        public async Task TestSendNotification()
        {
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.BOOTSTRAP, "/storyengine/dev/bootstrap");
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.AWSDEFAULTREGION, "us-east-1");
            System.Environment.SetEnvironmentVariable(Bootstrapping.DBUSERTYPE, "SmsUser");

            INotificationRequest smsRequest = GetRequest();


            string smsTextRequest = JsonConvert.SerializeObject(smsRequest);
            SmsNotificationRequest notReq = JsonConvert.DeserializeObject<SmsNotificationRequest>(smsTextRequest);

            var servProv = GetServiceProvider();

            IPhoneInfoRetriever phoneRetriever = servProv.GetService<IPhoneInfoRetriever>();
            DataPhone phoneInfo = await phoneRetriever.GetPhoneInfoAsync("+12675551212");

            notReq.DestinationNumberId = phoneInfo.Id;

            //  IPhoneInfoRetriever phoneRetriever = new TwilioPhoneInfoRetriever()

            var function = new MessageSaveFunction();
            // Invoke the lambda function and confirm the string was upper cased.
            var context = new TestLambdaContext();
            IOutboundNotificationRecord outRecord = null;
            try
            {
                outRecord = await function.SaveNotificationRequest(notReq, context);
            }
            catch (Exception ex)
            {
                Debug.Write(ex);

            }

            if (outRecord != null)
            {

                MessageSenderTasksFunction taskFunc = new MessageSenderTasksFunction();

                IOutboundNotificationRecord outRecResponse = await taskFunc.DispatchMessageAsync(outRecord, context);
            }




        }

        private IServiceProvider GetServiceProvider()
        {


            var config = Bootstrapping.BuildConfiguration();

            IServiceCollection servCol = new ServiceCollection();

            BootstrapConfig bootConfig = config.Get<BootstrapConfig>();

            Bootstrapping.ConfigureServices(servCol, config, bootConfig);

            var servProv = servCol.BuildServiceProvider();

            return servProv;
        }

        [Fact]
        public async Task SendStepFunctionAsync()
        {
            INotificationRequest smsReq = GetRequest();

            string stepFunctionName = "arn:aws:states:us-east-1:940085449815:stateMachine:OutboundSmsDev";
            RegionEndpoint endpoint = RegionEndpoint.USEast1;

            StartExecutionRequest executionRequest = new StartExecutionRequest
            {
                StateMachineArn = stepFunctionName,
                Name = Guid.NewGuid().ToString(),
                Input = JsonConvert.SerializeObject(smsReq)
            };

            try
            {
                using (AmazonStepFunctionsClient stepFunctionClient = new AmazonStepFunctionsClient(endpoint))
                {

                    StartExecutionResponse sendResponse =
                        await stepFunctionClient.StartExecutionAsync(executionRequest);

                    // _logger.LogInformation($"Scheduled Sms StepFunction with name {executionRequest.Name} to StepFunction {stepFunctionName} and received https status code {sendResponse.HttpStatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error sending ms StepFunction with name {executionRequest.Name} to StepFunction {stepFunctionName}",
                    ex);
            }


        }


        private INotificationRequest GetRequest()
        {
            SmsNotificationRequest smsRequest = new SmsNotificationRequest
            {
                DestinationNumberId = new Guid("178dea38-4c47-43f0-971e-63d5120e9642"),

                SourceNumber = "+12157099492",
                SmsProvider = SmsSenderType.Twilio
            };

            NotificationSourcePhoneMessageAction phoneMessageActionSource = new NotificationSourcePhoneMessageAction
            {
                Consent = new UserPhoneConsent()
                {
                    Name = "statilean",
                    // phoneMessageActionSource.EngineRequestId = new Guid("4d6c62da-25dd-4d7c-bfeb-e52373bb5485");
                    EngineRequestId = Guid.NewGuid(),
                    TitleClientUserId = new Guid("f2b64952-2976-4192-81f1-3dd1bedf6733"),
                    TitleVersionId = new Guid("a1b73399-21f5-4c36-8fda-8d9023732bac"),
                    SmsConsentDate = DateTime.UtcNow,
                    Id = Guid.NewGuid(),
                    IsSmsConsentGranted = true
                }
            };

            smsRequest.Source = phoneMessageActionSource;
            smsRequest.TextMessages = new List<TextFragmentBase>
            {
                new SimpleTextFragment("This is a text message")
            };

            return smsRequest;

        }
    }
}
