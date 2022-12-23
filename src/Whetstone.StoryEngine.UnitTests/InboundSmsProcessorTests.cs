using Amazon.Lambda.TestUtilities;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Strategies;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.InboundSmsHandler;
using Whetstone.StoryEngine.InboundSmsRepository;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Models.Story.Text;
using Xunit;

namespace Whetstone.UnitTests
{
    public class InboundSmsProcessorTests
    {


        [Fact]
        public async Task ProcessInboundMessage()
        {
            AWSXRayRecorder.Instance.ContextMissingStrategy = ContextMissingStrategy.LOG_ERROR;

            var titleVer = TitleVersionUtil.GetWhetstoneSmsTitle();

            MockFactory mocker = new MockFactory();

            IServiceCollection services = mocker.InitServiceCollection(titleVer);

            services.AddTransient<IInboundSmsProcessor, InboundSmsProcessor>();

            IServiceProvider provider = services.BuildServiceProvider();

            SmsTasks smsTasks = new SmsTasks(provider);

            string inboundMsgText = File.ReadAllText(@"SmsInboundMessages/inboundsmsmsg02.json");

            InboundSmsMessage msg = JsonConvert.DeserializeObject<InboundSmsMessage>(inboundMsgText);

            Exception launchEx = null;


            TestLambdaContext testContext = new TestLambdaContext();
            INotificationRequest notReq = null;

            try
            {
              notReq =  await smsTasks.SmsHandlerTask(msg, testContext);
            
            }
            catch (Exception ex)
            {
                launchEx = ex;
            }
            Assert.Null(launchEx);

            Assert.NotNull(notReq);

            Assert.Equal(NotificationTypeEnum.Sms, notReq.NotificationType);

            SmsNotificationRequest smsReq = notReq as SmsNotificationRequest;

            Assert.NotNull(smsReq);

            Assert.NotNull(smsReq.TextMessages);

            Assert.Single(smsReq.TextMessages);

            var simpleText = smsReq.TextMessages[0];

            string textContents = null;
            if (simpleText is SimpleTextFragment simpleFrag)
            {
                textContents = simpleFrag.Text;

            }

            Assert.Contains("Thanks for your interest in Whetstone Technologies! Please go to the following link to get your FREE whitepaper.", textContents);
        }

    }
}
