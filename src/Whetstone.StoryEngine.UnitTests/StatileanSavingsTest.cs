using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Tracking;
using Whetstone.StoryEngine.Repository;
using Xunit;

namespace Whetstone.UnitTests
{
    public class StatileanSavingsTest
    {

        [Fact]
        public async Task StatileanSkillServiceBadNumberPathTest()
        {
            TitleVersion titleVer = new TitleVersion();
            Client clientType = Client.Alexa;
            titleVer.ShortName = "statileansavings";
            titleVer.Version = "0.2";
            titleVer.DeploymentId = Guid.NewGuid();
            titleVer.VersionId = Guid.NewGuid();

            var mocker = new MockFactory();
            Guid batchUserId = default;
            string consentTypeName = null;


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
            mocker.ProcessSmsBatchFunc = processBatchFunc;

            mocker.ProcessSessionLogFunc = sessionLogFunc;

            mocker.ProcessNotificationFunc = processNotifFunc;

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

            // Confirm over 18
            request = context.CreateIntentRequest("YesIntent");
            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("StatileanInsuranceCheck"), $"Expected node StatileanInsuranceCheck. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on YesIntent should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            curUser = await userRepo.GetUserAsync(request);


            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);

            // confirm medical insurance
            request = context.CreateIntentRequest("YesIntent");
            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("StatileanRegularDiscount"), $"Expected node StatileanRegularDiscount. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on YesIntent should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);

            // provide phone number
            Dictionary<string, string> phoneNumberSlot = new Dictionary<string, string>();
            string phoneNumber = "1234";
            phoneNumberSlot.Add("phonenumber", phoneNumber);
            request = context.CreateIntentRequest("PhoneNumberIntent", phoneNumberSlot);
            resp = await storyRepProc.ProcessStoryRequestAsync(request);


            curUser = await userRepo.GetUserAsync(request);
            //PhoneDiscountVerification
            Assert.True(curUser.CurrentNodeName.Equals("BadPhoneFormatNode"), $"Expected node BadPhoneFormatNode. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on YesIntent should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);

            var crumbs = curUser.TitleState;

            Assert.True(crumbs.Count == 5, "Expected five crumbs");


            // Convert the crumbs into selected items and validate.
            List<SelectedItem> selItems = new List<SelectedItem>();
            foreach (IStoryCrumb crumb in crumbs)
            {
                if (crumb is SelectedItem)
                {
                    if (crumb is SelectedItem selItem)
                        selItems.Add(selItem);
                }
            }

            Assert.True(selItems.Count == 5, "Expected five items in the selected items");

            SelectedItem consentTypeItem = selItems.FirstOrDefault(x => x.Name.Equals("consentnameslot", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(consentTypeItem);

            consentTypeName = consentTypeItem.Value;


            // Find the mobile phone
            SelectedItem phoneNumItem = selItems.FirstOrDefault(x => x.Name.Equals("phonenumber"));
            Assert.True(phoneNumItem != null, "phonenumber selected item not found");
            Assert.True(phoneNumItem.Value.Equals(phoneNumber), $"{phoneNumber} value expected, but not found in phone number slot");

            string validNumberSlotName = "isphonenumbervalid";
            SelectedItem phoneFormatItem = selItems.FirstOrDefault(x => x.Name.Equals(validNumberSlotName));
            Assert.True(phoneFormatItem != null, $"{validNumberSlotName} selected item not found");
            Assert.True(phoneFormatItem.Value.Equals("false"), $"{validNumberSlotName} should be false");


            // send a land line
            phoneNumberSlot = new Dictionary<string, string>();
            phoneNumber = "2155551212";
            phoneNumberSlot.Add("phonenumber", phoneNumber);
            request = context.CreateIntentRequest("PhoneNumberIntent", phoneNumberSlot);
            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            // Validate that the phone number is a good format, but cannot get sms
            curUser = await userRepo.GetUserAsync(request);
            //PhoneDiscountVerification
            Assert.True(curUser.CurrentNodeName.Equals("CannotGetSmsMessageNode"), $"Expected node CannotGetSmsMessageNode. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on YesIntent should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);


            phoneNumberSlot = new Dictionary<string, string>();
            phoneNumber = "2674442342";
            phoneNumberSlot.Add("phonenumber", phoneNumber);
            request = context.CreateIntentRequest("PhoneNumberIntent", phoneNumberSlot);
            resp = await storyRepProc.ProcessStoryRequestAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("PhoneDiscountVerification"), $"Expected node PhoneDiscountVerification. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on YesIntent should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);


            // Confirm phone number
            request = context.CreateIntentRequest("YesIntent");
            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            // Process confirmation result -- this should be the end of the session.

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("SendDiscountCodeNode"), $"Expected node SendDiscountCodeNode. Found {curUser.CurrentNodeName}");
            Assert.True(!resp.ForceContinueSession, "Force continue session on YesIntent should be false");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);

            Assert.True(batchUserId.Equals(curUser.Id), "message request does not contain expected user id");
            Assert.NotNull(consentTypeName);

            Assert.True(consentTypeName.Equals("statilean"), $"consentTypeName is not 'statilean' as expected. It is {consentTypeName}");

        }



        [Fact]
        public async Task StatileanSkillServiceHappyPathTest()
        {
            TitleVersion titleVer = new TitleVersion();
            Client clientType = Client.Alexa;
            titleVer.ShortName = "statileansavings";
            titleVer.Version = "0.2";
            titleVer.DeploymentId = Guid.NewGuid();
            titleVer.TitleId = Guid.NewGuid();

            var mocker = new MockFactory();
            Guid batchUserId = default;
            string consentTypeName = null;

            void processNotifFunc(INotificationRequest notifReq)
            {
                SmsNotificationRequest smsRequest = notifReq as SmsNotificationRequest;

                NotificationSourcePhoneMessageAction sourcePhoneMessageAction = smsRequest.Source as NotificationSourcePhoneMessageAction;

                batchUserId = sourcePhoneMessageAction.Consent.TitleClientUserId;


            }

            void processBatchFunc(OutboundBatchRecord batchReq)
            {
                //  consentTypeName = batchReq.ConsentName;
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
            mocker.ProcessSmsBatchFunc = processBatchFunc;

            mocker.ProcessSessionLogFunc = sessionLogFunc;

            mocker.ProcessNotificationFunc = processNotifFunc;

            IServiceCollection servCol = mocker.InitServiceCollection(titleVer);


            IServiceProvider servProv = servCol.BuildServiceProvider();

            ISessionLogger sessionLogger = servProv.GetRequiredService<ISessionLogger>();


            IStoryRequestProcessor storyRepProc = servProv.GetRequiredService<IStoryRequestProcessor>();

            IMediaLinker linker = servProv.GetRequiredService<IMediaLinker>();


            var userStoreFunc = servProv.GetService<Func<UserRepositoryType, IStoryUserRepository>>();


            // Check the user state to see if the phone number is valid or not.
            IStoryUserRepository userRepo = userStoreFunc(UserRepositoryType.DynamoDB);


            StorySessionContext context = new StorySessionContext(titleVer, clientType);

            StoryRequest request = context.CreateLaunchRequest();

            StoryResponse resp = await storyRepProc.ProcessStoryRequestAsync(request);
            DataTitleClientUser curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("WelcomeNewUser"), $"Expected launch request to use WelcomeNewUser node. It returned {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on launch request should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessionLogger);

            // Confirm over 18
            request = context.CreateIntentRequest("YesIntent");
            request.SessionContext = resp.SessionContext;
            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("StatileanInsuranceCheck"), $"Expected node StatileanInsuranceCheck. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on YesIntent should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            curUser = await userRepo.GetUserAsync(request);


            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessionLogger);

            // confirm medical insurance
            request = context.CreateIntentRequest("YesIntent");
            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("StatileanRegularDiscount"), $"Expected node StatileanRegularDiscount. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on YesIntent should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessionLogger);

            // provide phone number
            Dictionary<string, string> phoneNumberSlot = new Dictionary<string, string>();
            string phoneNumber = "2675551212";
            phoneNumberSlot.Add("phonenumber", phoneNumber);
            request = context.CreateIntentRequest("PhoneNumberIntent", phoneNumberSlot);
            resp = await storyRepProc.ProcessStoryRequestAsync(request);


            curUser = await userRepo.GetUserAsync(request);
            //PhoneDiscountVerification
            Assert.True(curUser.CurrentNodeName.Equals("PhoneDiscountVerification"), $"Expected node PhoneDiscountVerification. Found {curUser.CurrentNodeName}");
            Assert.True(resp.ForceContinueSession, "Force continue session on YesIntent should be true");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessionLogger);

            var crumbs = curUser.TitleState;

            Assert.True(crumbs.Count == 5, "Expected five crumbs");


            // Convert the crumbs into selected items and validate.
            List<SelectedItem> selItems = new List<SelectedItem>();
            foreach (IStoryCrumb crumb in crumbs)
            {
                if (crumb is SelectedItem)
                {
                    if (crumb is SelectedItem selItem)
                        selItems.Add(selItem);
                }
            }

            Assert.True(selItems.Count == 5, "Expected five items in the selected items");

            SelectedItem consentTypeItem = selItems.FirstOrDefault(x => x.Name.Equals("consentnameslot", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(consentTypeItem);

            consentTypeName = consentTypeItem.Value;


            // Find the mobile phone
            SelectedItem phoneNumItem = selItems.FirstOrDefault(x => x.Name.Equals("phonenumber"));
            Assert.True(phoneNumItem != null, "phonenumber selected item not found");
            Assert.True(phoneNumItem.Value.Equals(phoneNumber), $"{phoneNumber} value expected, but not found in phone number slot");

            string validNumberSlotName = "isphonenumbervalid";
            SelectedItem phoneFormatItem = selItems.FirstOrDefault(x => x.Name.Equals(validNumberSlotName));
            Assert.True(phoneFormatItem != null, $"{validNumberSlotName} selected item not found");
            Assert.True(phoneFormatItem.Value.Equals("true"), $"{validNumberSlotName} should be true");



            // Confirm phone number
            request = context.CreateIntentRequest("YesIntent");
            resp = await storyRepProc.ProcessStoryRequestAsync(request);

            // Process confirmation result -- this should be the end of the session.

            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals("SendDiscountCodeNode"), $"Expected node SendDiscountCodeNode. Found {curUser.CurrentNodeName}");
            Assert.True(!resp.ForceContinueSession, "Force continue session on YesIntent should be false");
            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessionLogger);

            Assert.True(batchUserId.Equals(curUser.Id), "message request does not contain expected user id");
            Assert.NotNull(consentTypeName);

            Assert.True(consentTypeName.Equals("statilean"), $"consentTypeName is not 'statilean' as expected. It is {consentTypeName}");




        }


    }
}
