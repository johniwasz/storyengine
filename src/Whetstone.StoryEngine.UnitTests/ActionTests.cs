using Amazon;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Whetstone.StoryEngine;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Tracking;
using Whetstone.StoryEngine.Repository.Actions;

using Xunit;

namespace Whetstone.UnitTests
{
    public class ActionTests
    {
        private void SetEnvironmentVariables()
        {
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.AWSDEFAULTREGION, RegionEndpoint.USEast1.SystemName);
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.BOOTSTRAP, "/storyengine/dev/bootstrap");


            //System.Environment.SetEnvironmentVariable("REDISCACHESERVER", @"dev-cache-sanjtest.prgrxr.ng.0001.use1.cache.amazonaws.com");


            //System.Environment.SetEnvironmentVariable("ENCRYPTIONKEYNAME", "alias/ExternalSystemSecretsKey");

            //System.Environment.SetEnvironmentVariable("REDISCACHESERVER", @"localhost");

        }

        [Fact]
        public async Task ApplyPhoneActionWithMockSmsHandlerTest()
        {
            SetEnvironmentVariables();


            string titleId = "SampleTitle";
            string phoneSlot = "userphone";

            // Setup environment variables

            var mocker = new MockFactory();

            IServiceCollection serviceCol = mocker.InitServiceCollection(titleId);

            var provider = serviceCol.BuildServiceProvider();

            Func<NodeActionEnum, INodeActionProcessor> actionFunc = provider.GetService<Func<NodeActionEnum, INodeActionProcessor>>();
            PhoneMessageActionProcessor phoneProcessor = (PhoneMessageActionProcessor)actionFunc(NodeActionEnum.PhoneMessage);
            Assert.NotNull(phoneProcessor);

            (StoryRequest, List<IStoryCrumb>) storyParams = BuildStoryRequest(titleId, "2675551212", phoneSlot);

            StoryRequest request = storyParams.Item1;

            List<IStoryCrumb> crumbs = storyParams.Item2;

            string confirmationSlotName = "confirmationSlot";
            SelectedItem selItem = new SelectedItem();
            selItem.Name = confirmationSlotName;
            selItem.Value = "confirmation";
            crumbs.Add(selItem);

            PhoneMessageActionData phoneActionData = new PhoneMessageActionData
            {
                Messages = new List<PhoneMessage>()
            };

            PhoneMessage phoneMessage = new PhoneMessage
            {
                Message = "This is a text message from the OutboundSmsProcessor Lambda"
            };
            phoneActionData.Messages.Add(phoneMessage);
            phoneActionData.PhoneInfo = new PhoneInfo
            {
                SmsService = SmsSenderType.Twilio,
                SourcePhone = "+18005551212"
            };
            phoneActionData.IsPermanent = false;
            phoneActionData.PhoneNumberSlot = phoneSlot;
            phoneActionData.ConfirmationNameSlot = confirmationSlotName;

            string phoneSendMessage = await phoneProcessor.ApplyActionAsync(request, crumbs, phoneActionData);

            Assert.True(phoneSendMessage.Contains("This is a text message from the OutboundSmsProcessor Lambda"), "unexpected return string");

        }





        public static (StoryRequest, List<IStoryCrumb>) BuildStoryRequest(string titleId, string phoneNumber, string phoneSlot)
        {
            StoryRequest req = new StoryRequest();
            string strGuid = Guid.NewGuid().ToString();
            req.SessionContext = new EngineSessionContext();

            req.SessionContext.TitleVersion = new TitleVersion();
            req.SessionContext.TitleVersion.ShortName = titleId;
            req.SessionContext.TitleVersion.Version = "1.0";
            req.SessionContext.TitleVersion.TitleId = Guid.NewGuid();
            req.SessionContext.EngineUserId = Guid.NewGuid();

            req.ApplicationId = "amzn1.ask.skill.2704cc00-6641-4530-a076-b65ed8a0b2d6";
            req.SessionId = "amzn1.echo-api.session.897f2b5d-0178-46f9-989d-9c547c3bfa56";
            req.UserId = "amzn1.ask.account.AEYRFEC6CZGACDAJ5TDUNKBZMVONI7I4DGBCLMILPQBTIH3QDMSF4WFXYRV27TJ7FMZA4CYCIBZXMXOYRUUNKQTBGYJHC4MAFFJU6SPQSKORUSDLREJTOQFVSYKONFL5BLFKIUK2NOXPJHFYEX57B46QR42FWR4FLVWHHN5GY3RIYT6PUGIATM3FUL4FZ62JIIFDA3NL2TB5IUQ";
            req.RequestId = "amzn1.echo-api.request." + strGuid;
            req.Locale = "en-US";

            List<IStoryCrumb> crumbs = new List<IStoryCrumb>();



            SelectedItem selItem = new SelectedItem();
            selItem.Name = phoneSlot;
            selItem.Value = phoneNumber;
            //  selItem.Value = "6105551212";
            // selItem.Value = "2065273556";
            crumbs.Add(selItem);



            return (req, crumbs);
        }






    }
}
