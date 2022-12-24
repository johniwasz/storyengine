using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Messaging;
using Xunit;

namespace Whetstone.StoryEngine.Test.Messaging
{

    public class SnsMessageTests
    {

        [Fact]
        public async Task SendSimpleSnsMessageAsync()
        {



            AmazonSimpleNotificationServiceClient snsClient = new AmazonSimpleNotificationServiceClient(RegionEndpoint.USEast1);
            PublishRequest pubRequest = new PublishRequest();
            pubRequest.Message = "Here's a text message";
            pubRequest.PhoneNumber = "+12675551212";


            // add optional MessageAttributes, for example:
            //   pubRequest.MessageAttributes.Add("AWS.SNS.SMS.SenderID", new MessageAttributeValue
            //      { StringValue = "SenderId", DataType = "String" });
            PublishResponse pubResponse = await snsClient.PublishAsync(pubRequest);

            PublishRequest pubRequestSecond = new PublishRequest();
            pubRequestSecond.MessageAttributes.Add("RequestId", new MessageAttributeValue()
            {

                StringValue = Guid.NewGuid().ToString(),
                DataType = "String"

            });


            pubRequestSecond.Message = "Here's another text message";
            pubRequestSecond.PhoneNumber = "+12675551212";
            pubResponse = await snsClient.PublishAsync(pubRequest);

            OutboundMessageLogEntry res = ToMessageResult(pubResponse);



        }


        public OutboundMessageLogEntry ToMessageResult(PublishResponse pubResponse)
        {
            OutboundMessageLogEntry res = new OutboundMessageLogEntry();
            res.HttpStatusCode = (int)pubResponse.HttpStatusCode;
            res.LogTime = DateTime.UtcNow;
            res.ServiceMessageId = pubResponse.MessageId;

            if (pubResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                res.SendStatus = MessageSendStatus.SentToDispatcher;
            }
            else if (pubResponse.HttpStatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                res.SendStatus = MessageSendStatus.Throttled;
            }
            else
            {
                res.SendStatus = MessageSendStatus.Error;
            }

            return res;
        }

    }



}
