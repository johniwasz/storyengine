using Amazon.Lambda.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Repository;
using Xunit;

namespace Whetstone.UnitTests
{
    public class SmsMessageSendTest
    {

        [Fact]
        public async Task SmsMessageSendAsync()
        {
            // Theoretically prevents AWS X-Ray from throwing exceptions when an X-Ray segment isn't setup
            // although this doesn't currently appear to be working
            System.Environment.SetEnvironmentVariable("AWS_XRAY_CONTEXT_MISSING", "LOG_ERROR");


            var mocker = new MockFactory();

            IServiceCollection rootCol = mocker.InitServiceCollection("animalfarmpi");

            MessagingConfig msgConfig = new MessagingConfig();
            msgConfig.ThrottleRetryLimit = 2;
            msgConfig.MessageSendDelayInterval = 1200;

            rootCol.Configure<MessagingConfig>(
                options =>
                {
                    options.MessageSendDelayInterval = 2;

                    options.ThrottleRetryLimit = 1200;
                });

            IServiceProvider servProv = rootCol.BuildServiceProvider();

            ISmsHandler handler = servProv.GetService<ISmsHandler>();

            

            OutboundBatchRecord outboundMessage = SmsMessageLoggerTest.GetSmsOutboundMessage();

            var outMsg = await handler.SendOutboundSmsMessagesAsync(outboundMessage);

        }


    }
}
