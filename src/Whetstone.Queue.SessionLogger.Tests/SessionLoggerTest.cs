using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Whetstone.Queue.SessionLogger;
using Whetstone.StoryEngine;
using Whetstone.StoryEngine.Models.Messaging;
using Xunit;

namespace Whetstone.Queue.SessionLogger.Tests
{
    public class SessionLoggerTest : SessionLoggerTestBase
    {
        [Fact]
        public async Task RequestRecordLoad()
        {

            string messageBody = File.ReadAllText(@"samplesessions\requestrecord.json");

            //RequestRecordMessage sessionQueueMsg = null;

            //sessionQueueMsg = JsonConvert.DeserializeObject<RequestRecordMessage>(messageBody);

            SQSEvent sqsevent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>()
            };
            SQSEvent.SQSMessage msg = new SQSEvent.SQSMessage
            {
                Body = messageBody
            };

            sqsevent.Records.Add(msg);

            SessionQueueFunction queueFunc = new SessionQueueFunction();

            await queueFunc.QueueFunctionHandler(sqsevent, base.GetLambdaContext());
        }

        [Fact]
        public async Task LogSession()
        {

            SessionQueueFunction queueFunc = new SessionQueueFunction();

            using (AmazonSQSClient sqsClient = new AmazonSQSClient(RegionEndpoint.USEast1))
            {
                GetQueueUrlRequest queueRequest = new GetQueueUrlRequest
                {
                    QueueName = "sessionauditdev"
                };



                GetQueueUrlResponse queueResp = await sqsClient.GetQueueUrlAsync(queueRequest);


                ReceiveMessageRequest receiveMessageRequest = new ReceiveMessageRequest
                {
                    QueueUrl = queueResp.QueueUrl,
                    MaxNumberOfMessages = 5,
                    VisibilityTimeout = 10,
                    MessageAttributeNames = new List<string>
            {
                "*"
            }
                };
                ReceiveMessageResponse receiveMessageResponse = await sqsClient.ReceiveMessageAsync(receiveMessageRequest);


                var queuedMessages = receiveMessageResponse.Messages;


                var records = new List<SQSEvent.SQSMessage>();

                DeleteMessageBatchRequest deleteBatch = new DeleteMessageBatchRequest
                {
                    QueueUrl = queueResp.QueueUrl
                };

                foreach (var msg in queuedMessages)
                {
                    string body = msg.Body;

                    SQSEvent.SQSMessage eventMessage = new SQSEvent.SQSMessage
                    {
                        Body = msg.Body,
                        MessageId = msg.MessageId,
                        Md5OfBody = msg.MD5OfBody,
                        ReceiptHandle = msg.ReceiptHandle,

                        MessageAttributes = new Dictionary<string, SQSEvent.MessageAttribute>()
                    };


                    foreach (string attrKey in msg.MessageAttributes.Keys)
                    {
                        SQSEvent.MessageAttribute eventAttrib = new SQSEvent.MessageAttribute();
                        MessageAttributeValue sourceAttirb = msg.MessageAttributes[attrKey];

                        eventAttrib.StringValue = sourceAttirb.StringValue;
                        eventAttrib.DataType = sourceAttirb.DataType;
                        eventAttrib.BinaryListValues = sourceAttirb.BinaryListValues;
                        eventAttrib.BinaryValue = sourceAttirb.BinaryValue;
                        eventAttrib.StringListValues = sourceAttirb.StringListValues;

                        eventMessage.MessageAttributes.Add(attrKey, eventAttrib);

                    }

                    DeleteMessageBatchRequestEntry deleteEntry = new DeleteMessageBatchRequestEntry
                    {
                        Id = msg.MessageId,
                        ReceiptHandle = msg.ReceiptHandle
                    };
                    deleteBatch.Entries.Add(deleteEntry);

                    records.Add(eventMessage);
                }



                SQSEvent sqsevent = new SQSEvent
                {
                    Records = records
                };

                await queueFunc.QueueFunctionHandler(sqsevent, base.GetLambdaContext());
            }

            // DeleteMessageBatchResponse deleteResponse = await sqsClient.DeleteMessageBatchAsync(deleteBatch);

        }



    }
}
