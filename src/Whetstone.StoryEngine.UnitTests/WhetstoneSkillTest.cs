using Google.Cloud.Dialogflow.V2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Google.Repository;
using Whetstone.StoryEngine.Google.Repository.Models;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Story.Ssml;
using Whetstone.StoryEngine.Models.Story.Text;
using Whetstone.StoryEngine.Repository;
using Xunit;

namespace Whetstone.UnitTests
{


    public class WhetstoneSkillTest
    {

        [Theory]
        [MemberData(nameof(FulfillmentData))]
        public async Task WhetstoneCanFulfillIntentTest(string intentName, YesNoMaybeEnum expectedFulfillResponse, Dictionary<string, (string, SlotCanFulFill)> slotExpectations)
        {
            TitleVersion titleVer = TitleVersionUtil.GetWhetstoneTitle();
            Client clientType = Client.Alexa;

            Guid batchUserId = default;

            void processNotifFunc(INotificationRequest notifReq)
            {
                SmsNotificationRequest smsRequest = notifReq as SmsNotificationRequest;

                NotificationSourcePhoneMessageAction sourcePhoneMessageAction = smsRequest.Source as NotificationSourcePhoneMessageAction;

                batchUserId = sourcePhoneMessageAction.Consent.TitleClientUserId;
            }

            void processBatchFunc(OutboundBatchRecord batchReq)
            {
                batchUserId = batchReq.TitleUserId.Value;
                Assert.True(batchReq.AllSent);
            }

            void sessionLogFunc(RequestRecordMessage sl)
            {
                Assert.True(titleVer.DeploymentId.Value.Equals(sl.DeploymentId));

                Debug.WriteLine($"Logging deployment id: {sl.DeploymentId}");

            }

            // wire the delegates together so that the request to send the sms message is captured in the 
            // delegage above.

            var mocker = new MockFactory
            {
                ProcessSmsBatchFunc = processBatchFunc,
                ProcessSessionLogFunc = sessionLogFunc,
                ProcessNotificationFunc = processNotifFunc
            };

            IServiceCollection servCol = mocker.InitServiceCollection(titleVer);

            IServiceProvider servProv = servCol.BuildServiceProvider();

            IStoryRequestProcessor storyRepProc = servProv.GetRequiredService<IStoryRequestProcessor>();


            ISessionLogger sessLogger = servProv.GetRequiredService<ISessionLogger>();

            StorySessionContext context = new StorySessionContext(titleVer, clientType);

            Dictionary<string, string> slotValues = null;

            if (slotExpectations != null)
            {
                if (slotExpectations.Keys.Count > 0)
                    slotValues = new Dictionary<string, string>();

                foreach (string keyName in slotExpectations.Keys)
                {
                    var slotExpects = slotExpectations[keyName];

                    // ReSharper disable once PossibleNullReferenceException
                    slotValues.Add(keyName, slotExpects.Item1);
                }

            }


            StoryRequest request = context.CreateCanFulfillIntentRequest(intentName, slotValues);

            CanFulfillResponse resp = await storyRepProc.CanFulfillIntentAsync(request);

            Assert.True(resp.CanFulfill == expectedFulfillResponse, $"{intentName} fulfillment expected {expectedFulfillResponse} and got {resp.CanFulfill}");


            if (slotValues != null)
            {
                // validate the slot value fulfillment
                foreach (string slotKey in slotValues.Keys)
                {

                    var slotExpects = slotExpectations[slotKey].Item2;

                    SlotCanFulFill slotResult = resp.SlotFulFillment[slotKey];

                    Assert.True(slotExpects.CanFulfill.Equals(slotResult.CanFulfill), $"intent {intentName} with slot {slotKey} expected CanFulfill Value {slotExpects.CanFulfill} and got {slotResult.CanFulfill}");
                    Assert.True(slotExpects.CanUnderstand.Equals(slotResult.CanUnderstand), $"intent {intentName} with slot {slotKey} expected CanUnderstand Value {slotExpects.CanUnderstand} and got {slotResult.CanUnderstand}");

                    if (slotExpects.Value == null)
                        Assert.Null(slotResult.Value);
                    else
                        Assert.True(slotExpects.Value.Equals(slotResult.Value), $"intent {intentName} with slot {slotKey} expected value {slotExpects.Value} and got {slotResult.Value}");



                }


            }

            //Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            await ResponseHelper.WriteResponseAsync(request, resp, sessLogger);


        }



        //  [InlineDataAttribute("HearMoreIntent", YesNoMaybeEnum.No)]
        //  [InlineDataAttribute("ContactWhetstone", YesNoMaybeEnum.Yes, new Dictionary<string, string>() { { "key", "val" } })]


        public static IEnumerable<object[]> FulfillmentData =>
            new List<object[]>
            {
                new object[] { "HearMoreIntent", YesNoMaybeEnum.No, null },
                new object[] { "ContactWhetstone", YesNoMaybeEnum.Yes, new Dictionary<string, (string, SlotCanFulFill)>() { { "badkey", ( "badvalue", new SlotCanFulFill() { CanUnderstand = YesNoMaybeEnum.No, CanFulfill = YesNoMaybeEnum.No, Value  = null}) } } },
                new object[] { "ContactWhetstone", YesNoMaybeEnum.Yes, new Dictionary<string, (string, SlotCanFulFill)>() { { "whetstonename", ("Wet stone", new SlotCanFulFill() { CanUnderstand = YesNoMaybeEnum.Yes, CanFulfill = YesNoMaybeEnum.Yes, Value  = null}) } } },
             new object[] { "ContactWhetstone", YesNoMaybeEnum.Yes, new Dictionary<string, (string, SlotCanFulFill)>() { { "whetstonename", ("bogusvalue", new SlotCanFulFill() { CanUnderstand = YesNoMaybeEnum.Yes, CanFulfill = YesNoMaybeEnum.No, Value  = null}) } } }
            };


        [Fact]
        public async Task WhetstoneSkillBixbySimulatorServiceTest()
        {
            TitleVersion titleVer = TitleVersionUtil.GetWhetstoneTitle();

            Client clientType = Client.Bixby;


            Guid batchUserId = default;
            //string consentTypeName = null;


            void processNotifFunc(INotificationRequest notifReq)
            {
                SmsNotificationRequest smsRequest = notifReq as SmsNotificationRequest;

                NotificationSourcePhoneMessageAction sourcePhoneMessageAction = smsRequest.Source as NotificationSourcePhoneMessageAction;

                batchUserId = sourcePhoneMessageAction.Consent.TitleClientUserId;


            }

            void processBatchFunc(OutboundBatchRecord batchReq)
            {
                batchUserId = batchReq.TitleUserId.Value;
                Assert.True(batchReq.AllSent);
            }

            void sessionLogFunc(RequestRecordMessage sl)
            {
                Assert.True(titleVer.DeploymentId.Value.Equals(sl.DeploymentId));

                Debug.WriteLine($"Logging deployment id: {sl.DeploymentId}");

            }

            // wire the delegates together so that the request to send the sms message is captured in the 
            // delegage above.
            var mocker = new MockFactory
            {
                ProcessSmsBatchFunc = processBatchFunc,

                ProcessSessionLogFunc = sessionLogFunc,

                ProcessNotificationFunc = processNotifFunc
            };

            IServiceCollection servCol = mocker.InitServiceCollection(titleVer);

            IServiceProvider servProv = servCol.BuildServiceProvider();

            IStoryRequestProcessor storyRepProc = servProv.GetRequiredService<IStoryRequestProcessor>();

            IMediaLinker linker = servProv.GetRequiredService<IMediaLinker>();


            var userStoreFunc = servProv.GetService<Func<UserRepositoryType, IStoryUserRepository>>();

            // Check the user state to see if the phone number is valid or not.
            IStoryUserRepository userRepo = userStoreFunc(UserRepositoryType.DynamoDB);

            ISessionLogger sessLogger = servProv.GetRequiredService<ISessionLogger>();

            StorySessionContext context = new StorySessionContext(titleVer, clientType);

            Dictionary<string, string> whestoneSlots = new Dictionary<string, string>
            {
                { "whetstonename", "SoniBridge" }
            };

            StoryRequest request = context.CreateIntentRequest("LearnAboutWhetstone", whestoneSlots);// context.CreateLaunchRequest();

            StoryResponse resp = await storyRepProc.ProcessStoryRequestAsync(request);
            DataTitleClientUser curUser = await userRepo.GetUserAsync(request);
            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("SoniBridgeInfo"), $"Expected node SoniBridgeInfo. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on LearnAboutWhetstone should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            curUser = await userRepo.GetUserAsync(request);


            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);


            // Return Yes.
            request = context.CreateIntentRequest("YesIntent");
            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("SoniBridgeDemo"), $"Expected node SoniBridgeDemoDemo. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on LearnAboutWhetstone should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            curUser = await userRepo.GetUserAsync(request);


            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);

            // return a phone number
            whestoneSlots.Clear();
            whestoneSlots.Add("phonenumber", "2158852358");

            request = context.CreateIntentRequest("PhoneNumberIntent", whestoneSlots);

            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("GetPhoneNumberVerification"), $"Expected node GetPhoneNumberVerification. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on GetPhoneNumberVerification should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);



            request = context.CreateIntentRequest("YesIntent");


            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("CannotGetSmsMessageNode"), $"Expected node CannotGetSmsMessageNode. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on CannotGetSmsMessageNode should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);

            whestoneSlots.Clear();
            whestoneSlots.Add("phonenumber", "1234");
            request = context.CreateIntentRequest("PhoneNumberIntent", whestoneSlots);

            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("BadPhoneFormatNode"), $"Expected node BadPhoneFormatNode. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on BadPhoneFormatNode should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);


            request = context.CreatePauseRequest();
            resp = await storyRepProc.ProcessStoryRequestAsync(request);
            // Not sure what to do yet to process a pause response.


            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("BadPhoneFormatNode"), $"Expected node BadPhoneFormatNode. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on BadPhoneFormatNode should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);

            whestoneSlots.Clear();
            whestoneSlots.Add("phonenumber", "2675551212");
            request = context.CreateIntentRequest("PhoneNumberIntent", whestoneSlots);

            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("GetPhoneNumberVerification"), $"Expected node GetPhoneNumberVerification. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on GetPhoneNumberVerification should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);


            request = context.CreateIntentRequest("YesIntent");


            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("SendContactUsLink"), $"Expected node SendContactUsLink. Found {curUser.CurrentNodeName}");
            Assert.False(resp.ForceContinueSession, "Force continue session on SendContactUsLink should be false");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);


        }


        [Fact]
        public async Task WhetstoneSkillServiceFacebookMessengerTest()
        {
            TitleVersion titleVer = TitleVersionUtil.GetWhetstoneTitle();

            Client clientType = Client.FacebookMessenger;
            string expectedNode = null;
            Guid batchUserId = default;
            //string consentTypeName = null;


            void processNotifFunc(INotificationRequest notifReq)
            {
                SmsNotificationRequest smsRequest = notifReq as SmsNotificationRequest;

                NotificationSourcePhoneMessageAction sourcePhoneMessageAction = smsRequest.Source as NotificationSourcePhoneMessageAction;

                batchUserId = sourcePhoneMessageAction.Consent.TitleClientUserId;


            }

            void processBatchFunc(OutboundBatchRecord batchReq)
            {
                batchUserId = batchReq.TitleUserId.Value;
                Assert.True(batchReq.AllSent);
            }

            void sessionLogFunc(RequestRecordMessage sl)
            {
                Assert.True(titleVer.DeploymentId.Value.Equals(sl.DeploymentId));

                Debug.WriteLine($"Logging deployment id: {sl.DeploymentId}");

            }

            SurfaceCapabilities surfaceCaps = new SurfaceCapabilities
            {
                HasAudio = true
            };

            // wire the delegates together so that the request to send the sms message is captured in the 
            // delegage above.
            var mocker = new MockFactory
            {
                ProcessSmsBatchFunc = processBatchFunc,

                ProcessSessionLogFunc = sessionLogFunc,

                ProcessNotificationFunc = processNotifFunc
            };

            IServiceCollection servCol = mocker.InitServiceCollection(titleVer);

            IServiceProvider servProv = servCol.BuildServiceProvider();

            IStoryRequestProcessor storyRepProc = servProv.GetRequiredService<IStoryRequestProcessor>();

            IMediaLinker linker = servProv.GetRequiredService<IMediaLinker>();
            ILogger logger = mocker.GetLogger();


            var userStoreFunc = servProv.GetService<Func<UserRepositoryType, IStoryUserRepository>>();

            // Check the user state to see if the phone number is valid or not.
            IStoryUserRepository userRepo = userStoreFunc(UserRepositoryType.DynamoDB);

            ISessionLogger sessLogger = servProv.GetRequiredService<ISessionLogger>();

            StorySessionContext context = new StorySessionContext(titleVer, clientType);

            StoryRequest request = context.CreateLaunchRequest();

            StoryResponse resp = await storyRepProc.ProcessStoryRequestAsync(request);
            DataTitleClientUser curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("WelcomeNewUser"), $"Expected launch request to use WelcomeNewUser node. It returned {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on launch request should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);
            WebhookResponse webResp = resp.ToFacebookMessengerResponse(linker, logger, "prefix://");

            // pass a Sonibridge intent.


            // LearnMoreIntent
            request = context.CreateIntentRequest("LearnMoreIntent");
            resp = await storyRepProc.ProcessStoryRequestAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("SoniBridgeInfo"), $"Expected node SoniBridgeInfo. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on WhetstoneInfo should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            Assert.True(resp.Suggestions?.Count == 2, $"Expected two suggestions from node SoniBridgeInfo");

            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);
            webResp = resp.ToFacebookMessengerResponse(linker, logger, "prefix://");


            request = context.CreateIntentRequest("YesIntent");
            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            expectedNode = "ReturnLink";
            Assert.True(curUser.CurrentNodeName.Equals(expectedNode), $"Expected node {expectedNode}. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, $"Force continue session on {expectedNode} should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            Assert.True(resp.LocalizedResponse.GeneratedTextResponse.Count == 2, $"Expected three text responses for node {expectedNode}. Found {resp.LocalizedResponse.GeneratedTextResponse.Count}");
            Assert.True(resp.LocalizedResponse.GeneratedTextResponse[0] is SimpleTextFragment, $"Expected first speech fragment responses on node {expectedNode} to be SimpleTextFragment");
            Assert.True(resp.LocalizedResponse.GeneratedTextResponse[1] is SimpleTextFragment, $"Expected second speech fragment responses on node {expectedNode} to be SimpleTextFragment");


            Assert.True(resp.Suggestions?.Count == 3, $"Expected three suggestions from node {expectedNode}");

            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);
            webResp = resp.ToFacebookMessengerResponse(linker, logger, "prefix://");

            Dictionary<string, string> whestoneSlots = new Dictionary<string, string>
            {
                { "whetstonename", "SoniBridge" }
            };

            request = context.CreateIntentRequest("HearMoreIntent", whestoneSlots);
            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            expectedNode = "WhetstoneInfo";
            Assert.True(curUser.CurrentNodeName.Equals(expectedNode), $"Expected node {expectedNode}. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, $"Force continue session on {expectedNode} should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            Assert.True(resp.LocalizedResponse.GeneratedTextResponse.Count == 3, $"Expected three text responses for node {expectedNode}. Found {resp.LocalizedResponse.SpeechResponses.Count}");
            Assert.True(resp.LocalizedResponse.GeneratedTextResponse[0] is SimpleTextFragment, $"Expected first speech fragment responses on node {expectedNode} to be SimpleTextFragment");
            Assert.True(resp.LocalizedResponse.GeneratedTextResponse[1] is AudioTextFragment, $"Expected second speech fragment responses on node {expectedNode} to be AudioTextFragment");
            Assert.True(resp.LocalizedResponse.GeneratedTextResponse[2] is SimpleTextFragment, $"Expected first speech fragment responses on node {expectedNode} to be SimpleTextFragment");



            Assert.True(resp.LocalizedResponse.SpeechResponses.Count == 2, $"Expected two speech fragment responses on node WhetstoneInfo. Found {resp.LocalizedResponse.SpeechResponses.Count}");
            Assert.True(resp.LocalizedResponse.SpeechResponses[0] is AudioFile, $"Expected first speech fragment responses on node WhetstoneInfo to be AudioFile");
            Assert.True(resp.LocalizedResponse.SpeechResponses[1] is PlainTextSpeechFragment, $"Expected second speech fragment responses on node WhetstoneInfo to be SsmlSpeechFragment");

            Assert.True(resp.Suggestions?.Count == 2, $"Expected three suggestions from node {expectedNode}");

            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);
            webResp = resp.ToFacebookMessengerResponse(linker, logger, "prefix://");

            // Return No
            request = context.CreateIntentRequest("NoIntent");
            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            expectedNode = "EndOfWhetstone";
            Assert.True(curUser.CurrentNodeName.Equals(expectedNode), $"Expected node {expectedNode}. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, $"Force continue session on {expectedNode} should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);
            webResp = resp.ToFacebookMessengerResponse(linker, logger, "prefix://");


            // Return Contact Us
            request = context.CreateIntentRequest("GetContactInfo");
            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            expectedNode = "ContactUs";
            Assert.True(curUser.CurrentNodeName.Equals(expectedNode), $"Expected node {expectedNode}. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, $"Force continue session on {expectedNode} should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);
            webResp = resp.ToFacebookMessengerResponse(linker, logger, "prefix://");

            whestoneSlots = new Dictionary<string, string>
            {
                { "whetstonename", "SoniBridge" }
            };

            request = context.CreateIntentRequest("LearnAboutWhetstone", whestoneSlots);

            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            expectedNode = "SoniBridgeInfo";
            Assert.True(curUser.CurrentNodeName.Equals(expectedNode), $"Expected node {expectedNode}. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, $"Force continue session on {expectedNode} should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);
            webResp = resp.ToFacebookMessengerResponse(linker, logger, "prefix://");


            var diagResp = resp.ToDialogFlowResponse(surfaceCaps, linker, logger, request.UserId, "PREFIX");



        }


        [Fact]
        public async Task WhetstoneSkillServiceGoogleHomeTest()
        {
            TitleVersion titleVer = TitleVersionUtil.GetWhetstoneTitle();

            Client clientType = Client.GoogleHome;


            Guid batchUserId = default;
            //string consentTypeName = null;


            void processNotifFunc(INotificationRequest notifReq)
            {
                SmsNotificationRequest smsRequest = notifReq as SmsNotificationRequest;

                NotificationSourcePhoneMessageAction sourcePhoneMessageAction = smsRequest.Source as NotificationSourcePhoneMessageAction;

                batchUserId = sourcePhoneMessageAction.Consent.TitleClientUserId;


            }

            void processBatchFunc(OutboundBatchRecord batchReq)
            {
                batchUserId = batchReq.TitleUserId.Value;
                Assert.True(batchReq.AllSent);
            }

            void sessionLogFunc(RequestRecordMessage sl)
            {
                Assert.True(titleVer.DeploymentId.Value.Equals(sl.DeploymentId));

                Debug.WriteLine($"Logging deployment id: {sl.DeploymentId}");

            }

            // wire the delegates together so that the request to send the sms message is captured in the 
            // delegage above.
            var mocker = new MockFactory
            {
                ProcessSmsBatchFunc = processBatchFunc,

                ProcessSessionLogFunc = sessionLogFunc,

                ProcessNotificationFunc = processNotifFunc
            };

            SurfaceCapabilities surfaceCaps = new SurfaceCapabilities
            {
                HasAudio = true
            };

            IServiceCollection servCol = mocker.InitServiceCollection(titleVer);

            IServiceProvider servProv = servCol.BuildServiceProvider();

            IStoryRequestProcessor storyRepProc = servProv.GetRequiredService<IStoryRequestProcessor>();

            IMediaLinker linker = servProv.GetRequiredService<IMediaLinker>();

            ILogger logger = mocker.GetLogger();


            var userStoreFunc = servProv.GetService<Func<UserRepositoryType, IStoryUserRepository>>();

            // Check the user state to see if the phone number is valid or not.
            IStoryUserRepository userRepo = userStoreFunc(UserRepositoryType.DynamoDB);

            ISessionLogger sessLogger = servProv.GetRequiredService<ISessionLogger>();

            StorySessionContext context = new StorySessionContext(titleVer, clientType);

            StoryRequest request = context.CreateLaunchRequest();

            StoryResponse resp = await storyRepProc.ProcessStoryRequestAsync(request);
            DataTitleClientUser curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("WelcomeNewUser"), $"Expected launch request to use WelcomeNewUser node. It returned {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on launch request should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);


            Dictionary<string, string> whestoneSlots = new Dictionary<string, string>
            {
                { "whetstonename", "SoniBridge" }
            };

            request = context.CreateIntentRequest("HearMoreIntent", whestoneSlots);
            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("WhetstoneInfo"), $"Expected node WhetstoneInfo. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on WhetstoneInfo should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            Assert.True(resp.LocalizedResponse.SpeechResponses.Count == 2, $"Expected two speech fragment responses on node WhetstoneInfo. Found {resp.LocalizedResponse.SpeechResponses.Count}");
            Assert.True(resp.LocalizedResponse.SpeechResponses[0] is AudioFile, $"Expected first speech fragment responses on node WhetstoneInfo to be AudioFile");
            Assert.True(resp.LocalizedResponse.SpeechResponses[1] is SsmlSpeechFragment, $"Expected second speech fragment responses on node WhetstoneInfo to be SsmlSpeechFragment");


            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);

            //HearMoreIntent

            // Confirm over 18
            request = context.CreateIntentRequest("LearnAboutWhetstone", whestoneSlots);
            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("SoniBridgeInfo"), $"Expected node SoniBridgeInfo. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on LearnAboutWhetstone should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            curUser = await userRepo.GetUserAsync(request);


            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);


            // Return Yes.
            request = context.CreateIntentRequest("YesIntent");
            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("SoniBridgeDemo"), $"Expected node SoniBridgeDemo. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on LearnAboutWhetstone should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            curUser = await userRepo.GetUserAsync(request);


            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);

            // return a phone number
            whestoneSlots.Clear();
            whestoneSlots.Add("phonenumber", "2158852358");

            request = context.CreateIntentRequest("PhoneNumberIntent", whestoneSlots);

            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("GetPhoneNumberVerification"), $"Expected node GetPhoneNumberVerification. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on GetPhoneNumberVerification should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);



            request = context.CreateIntentRequest("YesIntent");


            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("CannotGetSmsMessageNode"), $"Expected node CannotGetSmsMessageNode. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on CannotGetSmsMessageNode should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);

            whestoneSlots.Clear();
            whestoneSlots.Add("phonenumber", "1234");
            request = context.CreateIntentRequest("PhoneNumberIntent", whestoneSlots);

            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("BadPhoneFormatNode"), $"Expected node BadPhoneFormatNode. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on BadPhoneFormatNode should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);


            request = context.CreatePauseRequest();
            resp = await storyRepProc.ProcessStoryRequestAsync(request);
            // Not sure what to do yet to process a pause response.


            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("BadPhoneFormatNode"), $"Expected node BadPhoneFormatNode. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on BadPhoneFormatNode should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);

            whestoneSlots.Clear();
            whestoneSlots.Add("phonenumber", "2675551212");
            request = context.CreateIntentRequest("PhoneNumberIntent", whestoneSlots);

            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("GetPhoneNumberVerification"), $"Expected node GetPhoneNumberVerification. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on GetPhoneNumberVerification should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);


            request = context.CreateIntentRequest("YesIntent");


            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("SendContactUsLink"), $"Expected node SendContactUsLink. Found {curUser.CurrentNodeName}");
            Assert.False(resp.ForceContinueSession, "Force continue session on EndOfWhetstone should be false");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);


            var diagResp = resp.ToDialogFlowResponse(surfaceCaps, linker, logger, request.UserId, "PREFIX");
        }


        [Fact]
        public async Task WhetstoneSkillServiceGoogleHomeShortTest()
        {
            TitleVersion titleVer = TitleVersionUtil.GetWhetstoneTitle();

            Client clientType = Client.GoogleHome;


            Guid batchUserId = default;
            //string consentTypeName = null;


            void processNotifFunc(INotificationRequest notifReq)
            {
                SmsNotificationRequest smsRequest = notifReq as SmsNotificationRequest;

                NotificationSourcePhoneMessageAction sourcePhoneMessageAction = smsRequest.Source as NotificationSourcePhoneMessageAction;

                batchUserId = sourcePhoneMessageAction.Consent.TitleClientUserId;


            }

            void processBatchFunc(OutboundBatchRecord batchReq)
            {
                batchUserId = batchReq.TitleUserId.Value;
                Assert.True(batchReq.AllSent);
            }

            void sessionLogFunc(RequestRecordMessage sl)
            {
                Assert.True(titleVer.DeploymentId.Value.Equals(sl.DeploymentId));

                Debug.WriteLine($"Logging deployment id: {sl.DeploymentId}");

            }

            // wire the delegates together so that the request to send the sms message is captured in the 
            // delegage above.
            var mocker = new MockFactory
            {
                ProcessSmsBatchFunc = processBatchFunc,

                ProcessSessionLogFunc = sessionLogFunc,

                ProcessNotificationFunc = processNotifFunc
            };

            SurfaceCapabilities surfaceCaps = new SurfaceCapabilities
            {
                HasAudio = true
            };

            IServiceCollection servCol = mocker.InitServiceCollection(titleVer);

            IServiceProvider servProv = servCol.BuildServiceProvider();

            IStoryRequestProcessor storyRepProc = servProv.GetRequiredService<IStoryRequestProcessor>();

            IMediaLinker linker = servProv.GetRequiredService<IMediaLinker>();

            ILogger logger = mocker.GetLogger();


            var userStoreFunc = servProv.GetService<Func<UserRepositoryType, IStoryUserRepository>>();

            // Check the user state to see if the phone number is valid or not.
            IStoryUserRepository userRepo = userStoreFunc(UserRepositoryType.DynamoDB);

            ISessionLogger sessLogger = servProv.GetRequiredService<ISessionLogger>();

            StorySessionContext context = new StorySessionContext(titleVer, clientType);

            StoryRequest request = context.CreateLaunchRequest();

            StoryResponse resp = await storyRepProc.ProcessStoryRequestAsync(request);
            DataTitleClientUser curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("WelcomeNewUser"), $"Expected launch request to use WelcomeNewUser node. It returned {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on launch request should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);

            // Return No.
            request = context.CreateIntentRequest("NoIntent");
            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("EndOfWhetstone"), $"Expected node EndOfWhetstone. Found {curUser.CurrentNodeName}");
            Assert.True(!resp.ForceContinueSession, "Force continue session on LearnAboutWhetstone should be false");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);

        }

        [Fact]
        public async Task EyeoftheElderGodsTest()
        {
            TitleVersion titleVer = TitleVersionUtil.GetEOTEGTitle();

            Client clientType = Client.GoogleHome;

            void sessionLogFunc(RequestRecordMessage sl)
            {
                Assert.True(titleVer.DeploymentId.Value.Equals(sl.DeploymentId));

                Debug.WriteLine($"Logging deployment id: {sl.DeploymentId}");

            }

            // wire the delegates together so that the request to send the sms message is captured in the 
            // delegage above.
            var mocker = new MockFactory
            {
                ProcessSessionLogFunc = sessionLogFunc
            };

            IServiceCollection servCol = mocker.InitServiceCollection(titleVer);

            IServiceProvider servProv = servCol.BuildServiceProvider();

            IStoryRequestProcessor storyRepProc = servProv.GetRequiredService<IStoryRequestProcessor>();

            IMediaLinker linker = servProv.GetRequiredService<IMediaLinker>();


            var userStoreFunc = servProv.GetService<Func<UserRepositoryType, IStoryUserRepository>>();

            // Check the user state to see if the phone number is valid or not.
            IStoryUserRepository userRepo = userStoreFunc(UserRepositoryType.DynamoDB);

            ISessionLogger sessLogger = servProv.GetRequiredService<ISessionLogger>();

            StorySessionContext context = new StorySessionContext(titleVer, clientType);

            StoryRequest request = context.CreateLaunchRequest();

            StoryResponse resp = await storyRepProc.ProcessStoryRequestAsync(request);
            DataTitleClientUser curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("WelcomeNewUser"), $"Expected launch request to use WelcomeNewUser node. It returned {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on launch request should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);


            // BeginIntent
            request = context.CreateIntentRequest("BeginIntent");
            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("A1"), $"Expected node A1. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on LearnAboutWhetstone should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            curUser = await userRepo.GetUserAsync(request);


            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);

            Dictionary<string, string> slots = new Dictionary<string, string>
            {
                { "verb", "answer" },
                { "item", "phone" }
            };
            // Return Yes.
            request = context.CreateIntentRequest("VerbTheItemIntent", slots);
            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("A2"), $"Expected node A2. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on LearnAboutWhetstone should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            curUser = await userRepo.GetUserAsync(request);

            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);

            // return a phone number
            slots.Clear();
            slots.Add("location", "ceremony");

            request = context.CreateIntentRequest("GotoLocationIntent", slots);

            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("A4"), $"Expected node A4. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on GetPhoneNumberVerification should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);



            request = context.CreateIntentRequest("GoOnIntent");


            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("A5"), $"Expected node A5. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on CannotGetSmsMessageNode should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);

            slots.Clear();
            slots.Add("verb", "read");
            slots.Add("action", "later");

            request = context.CreateIntentRequest("VerbTheActionIntent", slots);

            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("A7"), $"Expected node BadPhoneFormatNode. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on BadPhoneFormatNode should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");

            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);


        }


    }
}
