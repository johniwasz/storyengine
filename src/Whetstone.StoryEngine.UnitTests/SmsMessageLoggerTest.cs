using Amazon.Lambda.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Repository.Messaging;
using Xunit;

namespace Whetstone.UnitTests
{
    public class SmsMessageLoggerTest
    {
        //[Fact]
        //public async Task SmsOutboundMessageAsyncTest()
        //{
        //    // Invoke the lambda function and confirm the string was upper cased.

        //    var mocker = new MockFactory();
        //    var serviceCol = mocker.InitServiceCollection("sometitle", "dev");

        //    var servProv = serviceCol.BuildServiceProvider();

        //    //serviceProvider
        //    //    .Setup(x => x.GetService(typeof(IOutboundMessageLogger)))
        //    //    .Returns(Mock.Of<IOutboundMessageLogger>);

        //    var function = new Function(servProv);
        //    var context = new TestLambdaContext();


        //    OutboundSmsBatchRecord outboundMessage = GetSmsOutboundMessage();



        //   var outMessage = await function.FunctionHandler(outboundMessage, context);


        //}


        internal static OutboundBatchRecord GetSmsOutboundMessage()
        {
            OutboundBatchRecord outMessage = new OutboundBatchRecord
            {
                SmsToNumberId = Guid.NewGuid(),

                SmsFromNumberId = Guid.NewGuid(),

                EngineRequestId = Guid.NewGuid(),

                Id = Guid.NewGuid(),


                Messages = new List<OutboundMessagePayload>()
            };

            OutboundMessagePayload firstMessage = new OutboundMessagePayload();

            firstMessage.Message = "This is a text message";

            firstMessage.Results = new List<OutboundMessageLogEntry>();

            OutboundMessageLogEntry msgResult = new OutboundMessageLogEntry();
            msgResult.ExtendedStatus = "TXTSTAT";
            msgResult.HttpStatusCode = 200;
            msgResult.IsException = false;
            msgResult.LogTime = DateTime.UtcNow;

            firstMessage.Results.Add(msgResult);

            outMessage.Messages.Add(firstMessage);

            OutboundMessagePayload secondMessage = new OutboundMessagePayload();

            secondMessage.Message = "This is the second text";

            secondMessage.Results = new List<OutboundMessageLogEntry>();

            OutboundMessageLogEntry badResult = new OutboundMessageLogEntry
            {
                ExtendedStatus = "ERR",
                HttpStatusCode = 500,
                IsException = true,
                LogTime = DateTime.UtcNow
            };
            secondMessage.Results.Add(badResult);


            OutboundMessageLogEntry goodResult = new OutboundMessageLogEntry();
            goodResult.IsException = false;
            goodResult.HttpStatusCode = 200;
            goodResult.LogTime = DateTime.UtcNow;
            goodResult.ExtendedStatus = "SUCCESS";
            secondMessage.Results.Add(goodResult);

            outMessage.Messages.Add(secondMessage);


            return outMessage;
        }

    }
}
