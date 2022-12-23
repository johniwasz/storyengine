using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Whetstone.StoryEngine;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.InboundSmsRepository;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Repository.Phone;
using Xunit;

namespace Whetstone.UnitTests
{
    public class SmsTwilioResponseTest 
    {
        [Fact]
        public async Task SendSmsResponseAsync()
        {
            string inboundMessageText = ResouceFileUtil.GetJsonContents("SmsInboundMessages.InboundSmsMsg01");

            InboundSmsMessage msg = JsonConvert.DeserializeObject<InboundSmsMessage>(inboundMessageText);
            string responseBodyText = null;
            string notificationText = null;
            MockFactory mocker = new MockFactory();

            NameValueCollection nameVals = HttpUtility.ParseQueryString(msg.Body);
            IDictionary<string, string> bodyVals = CommonExtensions.ToDictionary(nameVals);
            string userPhone = bodyVals["From"];
           
 
            mocker.ProcessNotificationFunc = notif =>
            {
                SmsNotificationRequest smsRequest = notif as SmsNotificationRequest;


                notificationText = smsRequest.TextMessages.CleanText();


            };

            mocker.ProcessSessionLogFunc = sl =>
            {
                responseBodyText = sl.ResponseBodyText;

            };




            TitleVersion titleVer = new TitleVersion
            {
                Version = "0.2",
                ShortName = "whetstonetechnologiessms",
                Alias = "ws02",
                DeploymentId = Guid.NewGuid(),
                VersionId = Guid.NewGuid(),
                TitleId = Guid.NewGuid()
            };

            IServiceCollection servCol = mocker.InitServiceCollection( titleVer);

            IServiceProvider servProv = servCol.BuildServiceProvider();

            IPhoneInfoRetriever phoneRet = servProv.GetService<IPhoneInfoRetriever>();

            // Add the prior SMS confirmation to the list of consents
            ISmsConsentRepository smsConsentRepo = servProv.GetRequiredService<ISmsConsentRepository>();
            UserPhoneConsent phoneConsent = new UserPhoneConsent
            {
                IsSmsConsentGranted = true,
                Name = "whetstonetechnologies",
                Phone = new DataPhone()
            };
            phoneConsent.Phone.PhoneNumber = userPhone;
            var phoneInfo = await phoneRet.GetPhoneInfoAsync(userPhone);
            phoneConsent.PhoneId = phoneInfo.Id.Value;
            await smsConsentRepo.SaveConsentAsync(phoneConsent);


            StoryRequest req = new StoryRequest
            {
                SessionContext = new EngineSessionContext()
            };
            req.SessionContext.TitleVersion = titleVer;
            req.UserId = userPhone;
            req.Client = Client.Sms;
            req.Locale = "en-US";

            var userStoreFunc = servProv.GetService<Func<UserRepositoryType, IStoryUserRepository>>();


            IStoryUserRepository userRep = userStoreFunc(UserRepositoryType.DynamoDB);
            DataTitleClientUser curUser = await userRep.GetUserAsync(req);

            // Grant consent for the current sms title
            phoneConsent = new UserPhoneConsent
            {
                IsSmsConsentGranted = true,
                Name = "whetstonetechnologiessms",
                Phone = new DataPhone
                {
                    PhoneNumber = userPhone
                }
            };
            phoneInfo = await phoneRet.GetPhoneInfoAsync(userPhone);
            phoneConsent.PhoneId = phoneInfo.Id.Value;
            phoneConsent.TitleClientUserId = curUser.Id.Value;
            await smsConsentRepo.SaveConsentAsync( phoneConsent);


            ILogger logger = Mock.Of<ILogger>();
            IInboundSmsProcessor inboundSms = Mock.Of<IInboundSmsProcessor>();

            await inboundSms.ProcessInboundSmsMessageAsync(msg);
            curUser = await userRep.GetUserAsync(req);



            Debug.WriteLine(notificationText);

            msg.Body = msg.Body.Replace("YES", "STOP", StringComparison.OrdinalIgnoreCase);
            await inboundSms.ProcessInboundSmsMessageAsync(msg);
            curUser = await userRep.GetUserAsync(req);

            Debug.WriteLine(notificationText);


            msg.Body = msg.Body.Replace("STOP", "RESUME",StringComparison.OrdinalIgnoreCase);
            await inboundSms.ProcessInboundSmsMessageAsync(msg);
            curUser = await userRep.GetUserAsync(req);

            Debug.WriteLine(notificationText);


            msg.Body = msg.Body.Replace("RESUME", "YES", StringComparison.OrdinalIgnoreCase);
            await inboundSms.ProcessInboundSmsMessageAsync(msg);
            curUser = await userRep.GetUserAsync(req);

            Debug.WriteLine(notificationText);

        }


    }
}
