using Amazon.Lambda.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.Repository.Messaging;
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


            // Add some stubbed out test handler and factory to support the unit test and then base SmsHandler
            // that will use the factory and the test handler to send the message.
            EmptySmsSender.SetupUnitTestSmsDependencies(rootCol);

            IServiceProvider servProv = rootCol.BuildServiceProvider();

            ISmsHandler handler = servProv.GetService<ISmsHandler>();

            var context = new TestLambdaContext();

            OutboundBatchRecord outboundMessage = SmsMessageLoggerTest.GetSmsOutboundMessage();

            var outMsg = await handler.SendOutboundSmsMessagesAsync(outboundMessage);

            IOutboundMessageLogger outboundMessageLogger = servProv.GetService<IOutboundMessageLogger>();

            await outboundMessageLogger.UpdateOutboundMessageBatchAsync(outMsg);




        }


    }
}
