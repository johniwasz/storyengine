using System;
using System.Collections.Generic;
using Xunit;
using Amazon.Pinpoint;
using System.Threading.Tasks;
using Amazon.Pinpoint.Model;
using System.Diagnostics;
using System.Globalization;
using Whetstone.StoryEngine.Repository.Messaging;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.OutboutSmsSender;
using Microsoft.Extensions.Options;
using Moq;
using Amazon;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Messaging.Sms;

namespace Whetstone.StoryEngine.Test.Messaging
{

    public class PinpointTests
    {



        //[Fact]
        //public async Task UsePinPointHandlerAsync()
        //{
        //    var outMessage = StepFunctionLaunchTest.GetSmsOutboundMessage("dev");


        //    EnvironmentConfig envConfig = new EnvironmentConfig();

        //    envConfig.Region = RegionEndpoint.USEast1;

        //    IOptions<EnvironmentConfig> envConfigOpts = Options.Create<EnvironmentConfig>(envConfig);
            
        //    ISmsSender pinpointSender = new SmsPinpointSender(envConfigOpts);


        //    var msgConfigMock = new Mock<IOptionsSnapshot<MessagingConfig>>();

        //    msgConfigMock.Setup(x => x.Get(It.IsAny<string>()))
        //        .Returns((string envKey) =>
        //        {

        //            MessagingConfig msgConfig = new MessagingConfig()
        //            {
        //                MessageSendDelayInterval = 1500,
        //                ThrottleRetryLimit = 0


        //            };


        //            return msgConfig;

        //        });


        //    Func<string, ISmsSender> pinpointAccessor = new Func<string, ISmsSender>( x=> 
        //    {
        //        return new SmsPinpointSender(envConfigOpts);

        //    });

        //    IPhoneInfoRetriever phoneRetriever = Mock.Of<IPhoneInfoRetriever>();


        //    ISmsHandler pinHandler = new SmsDirectSendHandler(msgConfigMock.Object, pinpointAccessor, phoneRetriever);


        //    OutboundSmsBatchRecord updatedMessage = null;

        //    // outMessage.SmsToNumber = "+12675551212";

        //  //  outMessage.SmsToNumber = "+16105551212";
           
        //    // outMessage.SmsFromNumber = "54876";
           
        //   // outMessage.SmsFromNumber = "+17344283758";


        //    try
        //    {

        //        updatedMessage = await pinHandler.SendOutboundSmsMessagesAsync(outMessage);
        //    }
        //    catch(Exception ex)
        //    {

        //        Debug.WriteLine(ex);

        //    }
        

        //}


        [Fact]
        public async Task PinpointSmsCreateAppAsync()
        {
            AmazonPinpointClient pinClient = new AmazonPinpointClient(Amazon.RegionEndpoint.USEast1);

            CreateAppRequest createApp = new CreateAppRequest();
            createApp.CreateApplicationRequest = new CreateApplicationRequest();
            createApp.CreateApplicationRequest.Name = "SampleSend";



            CreateAppResponse appResp = await pinClient.CreateAppAsync(createApp);

           string newAppId = appResp.ApplicationResponse.Id;


        }


        [Fact]
        public async Task TestPhone()
        {
            var c = new CultureInfo("en-US");
            var r = new RegionInfo(c.LCID);
            string name = r.Name;
            string regionName = r.TwoLetterISORegionName;

            AmazonPinpointClient pinClient = new AmazonPinpointClient(Amazon.RegionEndpoint.USEast1);

            PhoneNumberValidateRequest phoneValidateReq = new PhoneNumberValidateRequest();
            
            NumberValidateRequest valRequest = new NumberValidateRequest();
            valRequest.IsoCountryCode = regionName;
            //   valRequest.PhoneNumber = "2158852358";
            // valRequest.PhoneNumber = "+16105551212";
            valRequest.PhoneNumber = "2675551212";


            var retVal = PhoneUtility.ValidateFormat(valRequest.PhoneNumber, regionName);


            phoneValidateReq.NumberValidateRequest = valRequest;

           var valResponseWrapper = await  pinClient.PhoneNumberValidateAsync(phoneValidateReq);
            NumberValidateResponse valResponse = valResponseWrapper.NumberValidateResponse;


            Debug.WriteLine(String.Format("Original Phone Number: {0}", valResponse.OriginalPhoneNumber));
            Debug.WriteLine(String.Format("E164 Formatted Phone Number: {0}", valResponse.CleansedPhoneNumberE164));
            Debug.WriteLine(String.Format("National Formatted Phone Number: {0}", valResponse.CleansedPhoneNumberNational));

            Debug.WriteLine(String.Format("Carrier: {0}", valResponse.Carrier));
            Debug.WriteLine(String.Format("Phone Type: {0}", valResponse.PhoneType));
            Debug.WriteLine(String.Format("Phone Type Code: {0}", valResponse.PhoneTypeCode));

            Debug.WriteLine(String.Format("City: {0}", valResponse.City));
            Debug.WriteLine(String.Format("County: {0}", valResponse.County));
            Debug.WriteLine(String.Format("Zip Code: {0}", valResponse.ZipCode));
            Debug.WriteLine(String.Format("Country: {0}", valResponse.County));
            Debug.WriteLine(String.Format("Timezone: {0}", valResponse.Timezone));


        }


        [Fact]
        public async Task AddEndpoint()
        {
            AmazonPinpointClient pinClient = new AmazonPinpointClient(Amazon.RegionEndpoint.USEast1);

            SendMessagesRequest testMessage = new SendMessagesRequest();

            testMessage.ApplicationId = "256060e139054c6e8402b44506b0d9f5";
            testMessage.MessageRequest = new MessageRequest();
          //  testMessage.MessageRequest.Endpoints = new Dictionary<string, EndpointSendConfiguration>();
            testMessage.MessageRequest.Addresses = new Dictionary<string, AddressConfiguration>();
            AddressConfiguration addrConfig = new AddressConfiguration();
            addrConfig.ChannelType = "SMS";

            string johnNumber = "+12675551212";
            //string sanjNumber = "+16105551212";
            //string marianneNumber = "+19084075551";

            string longCode = "+17344283758";
            string shortCode = "54876";

            string usedCode = longCode;
            string usedNumber = johnNumber;

            testMessage.MessageRequest.Addresses.Add(usedNumber, addrConfig);

            testMessage.MessageRequest.MessageConfiguration = new DirectMessageConfiguration();
            testMessage.MessageRequest.MessageConfiguration.SMSMessage = new SMSMessage();

            testMessage.MessageRequest.MessageConfiguration.SMSMessage.MessageType = MessageType.PROMOTIONAL;
            testMessage.MessageRequest.MessageConfiguration.SMSMessage.Body = $"Message sent using {shortCode}";

            testMessage.MessageRequest.MessageConfiguration.SMSMessage.OriginationNumber = usedCode;
            //548-76
            //testMessage.MessageRequest.MessageConfiguraton.SMSMessage.Body = "You can buy me a six pack later.";


            SendMessagesResponse sendResponse = null;

            try
            {
                sendResponse = await pinClient.SendMessagesAsync(testMessage);

                foreach(string endpointKey in sendResponse.MessageResponse.Result.Keys)
                {
                    var result = sendResponse.MessageResponse.Result[endpointKey];

                    Debug.WriteLine(result.DeliveryStatus);
                }
                

            }
            catch
            {
                throw;

            }
          //  EndpointSendConfiguration endPointDest = new EndpointSendConfiguration();


            //+ 12675551212
        }

        [Fact]
        public async Task PinpointSmsSendAsync()
        {
            AmazonPinpointClient pinClient = new AmazonPinpointClient(Amazon.RegionEndpoint.USEast1);

            SendMessagesRequest req = new SendMessagesRequest();
            req.ApplicationId = "256060e139054c6e8402b44506b0d9f5";

            req.MessageRequest = new MessageRequest();
            req.MessageRequest.Endpoints = new Dictionary<string, EndpointSendConfiguration>();
            EndpointSendConfiguration endConfig = new EndpointSendConfiguration();


            await pinClient.SendMessagesAsync(req);




        }

    }
}
