using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;
using Twilio;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Whetstone.StoryEngine.Repository.Messaging;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.OutboutSmsSender;
using Whetstone.StoryEngine.Models.Story;
using Microsoft.Extensions.Options;
using Twilio.Security;
using System.Web;
using Whetstone.StoryEngine.Models.Configuration;
using Moq;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Caching.Memory;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Repository.Phone;
using Whetstone.StoryEngine.InboundSmsHandler;
using Microsoft.Extensions.Logging;

namespace Whetstone.StoryEngine.Test.Messaging
{

    public class TwilioTests : TestServerFixture
    {

        [Fact]
        public async Task ProcessRawInboundSmsRequestAsync()
        {
            string sampleMessage = await File.ReadAllTextAsync("Messages/inboundmsg01.json");
            InboundSmsMessage inboundMessage = JsonConvert.DeserializeObject<InboundSmsMessage>(sampleMessage);


            SmsTasks func = new SmsTasks();
            ILambdaContext context = Mock.Of<ILambdaContext>();

            await func.SmsHandlerTask(inboundMessage, context);


        }


        private ISmsSender senderDelegate(SmsSenderType key)
        {
            switch (key)
            {
                case SmsSenderType.Twilio:
                    var moqSnapshot = new Mock<IOptionsSnapshot<TwilioConfig>>();
                    var moqSecureStore = new Mock<ISecretStoreReader>();
                    var moqMemCache = new Mock<IMemoryCache>();
                    var moqLogger = new Mock<ILogger<SmsTwilioSender>>();

                    return new SmsTwilioSender(moqSnapshot.Object, moqSecureStore.Object, moqMemCache.Object, moqLogger.Object);
                default:
                    throw new KeyNotFoundException();
            }
        }



        // SmsFunction

        [Fact]
        public async Task SendStopRequestAsync()
        {
            //string sampleMessage = GetJsonContents("twiliomsg01");

            string sampleMessage = await File.ReadAllTextAsync("Messages/inboundstopmsg.json");
            InboundSmsMessage inboundMessage = JsonConvert.DeserializeObject<InboundSmsMessage>(sampleMessage);



            SmsTasks func = new SmsTasks();
            ILambdaContext context = Mock.Of<ILambdaContext>();

            await func.SmsHandlerTask(inboundMessage, context);


        }

        [Fact]
        public async Task ProcessInboundSmsRequestAsync()
        {
            // Grant the required consent
           // string env = "dev";



            //string sampleMessage = GetJsonContents("twiliomsg01");

            string sampleMessage = await File.ReadAllTextAsync("Messages/inboundmsg01.json");
            InboundSmsMessage inboundMessage = JsonConvert.DeserializeObject<InboundSmsMessage>(sampleMessage);

            IAppMappingReader appMappingReader = this.Services.GetService<IAppMappingReader>();
            ISmsConsentRepository consentRepo = this.Services.GetService<ISmsConsentRepository>();
            IPhoneInfoRetriever phoneInfo = this.Services.GetService<IPhoneInfoRetriever>();

            Func<UserRepositoryType, IStoryUserRepository> userRepoFunc =
                this.Services.GetService<Func<UserRepositoryType, IStoryUserRepository>>();


            IStoryUserRepository userRepo = userRepoFunc(UserRepositoryType.DynamoDB);

            TitleVersion titleVer = await appMappingReader.GetTitleAsync(Models.Client.Alexa, "amzn1.ask.skill.c4cabd50-2cd5-4e4c-a03c-a57d4f2a0e5f", null);
            

            // Get the phone number from the message
            NameValueCollection bodyMessages = HttpUtility.ParseQueryString(inboundMessage.Body);
            Dictionary<string, string> bodyDict = bodyMessages.ToDictionary();
            string fromNumber = bodyDict["From"];

            DataPhone sourcePhone = await phoneInfo.GetPhoneInfoAsync( fromNumber);

            StoryRequest storyReq = new StoryRequest();
   
            storyReq.SessionContext = new EngineSessionContext();
            storyReq.SessionContext.TitleVersion = titleVer;
            storyReq.Client = Models.Client.Alexa;
            storyReq.UserId = fromNumber;

            DataTitleClientUser curUser = await userRepo.GetUserAsync(storyReq);
             
            UserPhoneConsent phoneConsent = new UserPhoneConsent();
            phoneConsent.EngineRequestId = Guid.NewGuid();
            phoneConsent.IsSmsConsentGranted= true;
            phoneConsent.TitleVersionId = titleVer.VersionId.Value;
            phoneConsent.Name = "whetstonetechnologies";
            phoneConsent.PhoneId = sourcePhone.Id.Value;
            phoneConsent.TitleClientUserId = curUser.Id.Value;
            phoneConsent.SmsConsentDate = DateTime.UtcNow;

            phoneConsent = await consentRepo.SaveConsentAsync(phoneConsent);


           UserPhoneConsent foundConsent = await consentRepo.GetConsentAsync(phoneConsent.Name, phoneConsent.PhoneId);


            SmsTasks func = new SmsTasks();
            ILambdaContext context = Mock.Of<ILambdaContext>();

            await func.SmsHandlerTask(inboundMessage, context);


        }




        [Fact]
        public async Task VerifyTwilioAsync()
        {

            //string sampleMessage = GetJsonContents("twiliomsg01");

            string sampleMessage = await File.ReadAllTextAsync("Messages/inboundmsg01.json");
            InboundSmsMessage inboundMessage = JsonConvert.DeserializeObject<InboundSmsMessage>(sampleMessage);


            ITwilioVerifier twilVer = Services.GetRequiredService<ITwilioVerifier>();

            NameValueCollection nameValCol = HttpUtility.ParseQueryString(inboundMessage.Body);
            IDictionary<string, string> formDict = nameValCol.ToDictionary();



            bool isValid = await twilVer.ValidateTwilioMessageAsync(inboundMessage.Path, inboundMessage.Headers, formDict, inboundMessage.Alias);

        }




        [Fact]
        public async Task VerifyTwilioProdAsync()
        {

            //string sampleMessage = GetJsonContents("twiliomsg01");

            string sampleMessage = await File.ReadAllTextAsync("Messages/inboundprodsms.json");
            InboundSmsMessage inboundMessage = JsonConvert.DeserializeObject<InboundSmsMessage>(sampleMessage);


            ITwilioVerifier twilVer = Services.GetRequiredService<ITwilioVerifier>();

            NameValueCollection nameValCol = HttpUtility.ParseQueryString(inboundMessage.Body);
            IDictionary<string, string> formDict = nameValCol.ToDictionary();



            bool isValid = await twilVer.ValidateTwilioMessageAsync(inboundMessage.Path, inboundMessage.Headers, formDict, inboundMessage.Alias);

        }

        [Fact]
        public async Task UseTwilioHandlerAsync()
        {
            var outMessage = StepFunctionLaunchTest.GetSmsOutboundMessage();


            var moqSnapshot = new Mock<IOptionsSnapshot<MessagingConfig>>();

            moqSnapshot.Setup(x => x.Get(It.IsAny<string>()))
                .Returns((MessagingConfig configType) =>
                {
                    return new MessagingConfig
                    {
                        MessageSendDelayInterval = 1500,
                        ThrottleRetryLimit = 0

                    };
                });




            Func<SmsSenderType, ISmsSender> smsSenderFunc = Mock.Of<Func<SmsSenderType, ISmsSender>>();

        

            IPhoneInfoRetriever phoneRetriever = Mock.Of<IPhoneInfoRetriever>();

            ILogger<SmsDirectSendHandler> directSendLogger = Mock.Of<ILogger<SmsDirectSendHandler>>();


            ISmsHandler smsHandler = new SmsDirectSendHandler(moqSnapshot.Object, smsSenderFunc, phoneRetriever, directSendLogger);


            try
            {

                await smsHandler.SendOutboundSmsMessagesAsync(outMessage);
            }
            catch (Exception ex)
            {

                Debug.WriteLine(ex);

            }


        }

       
      

    }
}
