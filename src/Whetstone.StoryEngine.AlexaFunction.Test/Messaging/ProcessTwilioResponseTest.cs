using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Amazon;
using Twilio.Security;
using Whetstone.Queue.TwiliosStatusUpdate;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.WebLibrary;
using Xunit;

namespace Whetstone.StoryEngine.AlexaFunction.Test.Messaging
{
    public class ProcessTwilioResponseTest : LambdaTestBase
    {


        [Fact]
        public async Task ProcessTwilioResponseMessageAsync()
        {

            AmazonSQSClient sqsClient = new AmazonSQSClient(RegionEndpoint.USEast1);

            GetQueueUrlRequest queueRequest = new GetQueueUrlRequest();
            queueRequest.QueueName = "dev-twiliosmsstatuscallback";



            GetQueueUrlResponse queueResp = await sqsClient.GetQueueUrlAsync(queueRequest);


            ReceiveMessageRequest receiveMessageRequest = new ReceiveMessageRequest();

            receiveMessageRequest.QueueUrl = queueResp.QueueUrl;
            receiveMessageRequest.MaxNumberOfMessages = 1;
            receiveMessageRequest.VisibilityTimeout = 10;
            receiveMessageRequest.MessageAttributeNames = new List<string>();
            receiveMessageRequest.MessageAttributeNames.Add(TwilioStatusUpdateMessage.ORIGINAL_HOST_ATTRIBUTE);
            receiveMessageRequest.MessageAttributeNames.Add(TwilioStatusUpdateMessage.PATH_ATTRIBUTE);
            receiveMessageRequest.MessageAttributeNames.Add(TwilioStatusUpdateMessage.ORIGINAL_SCHEME_ATTRIBUTE);
            receiveMessageRequest.MessageAttributeNames.Add(TwilioStatusUpdateMessage.VALIDATION_TOKEN_ATTRIBUTE);
            string origSchemeText = null;
            string origHostText = null;
            string origPathText = null;
            string origValTokenText = null;

            ReceiveMessageResponse receiveMessageResponse = await sqsClient.ReceiveMessageAsync(receiveMessageRequest);


            var queuedMessages = receiveMessageResponse.Messages;


            var records = new List<SQSEvent.SQSMessage>();

            DeleteMessageBatchRequest deleteBatch = new DeleteMessageBatchRequest();
            deleteBatch.QueueUrl = queueResp.QueueUrl;

            foreach (var msg in queuedMessages)
            {
                string body = msg.Body;

                SQSEvent.SQSMessage eventMessage = new SQSEvent.SQSMessage();

                eventMessage.Body = msg.Body;
                eventMessage.MessageId = msg.MessageId;
                eventMessage.Md5OfBody = msg.MD5OfBody;
                eventMessage.ReceiptHandle = msg.ReceiptHandle;


                if (eventMessage.MessageAttributes == null)
                    eventMessage.MessageAttributes = new Dictionary<string, SQSEvent.MessageAttribute>();

                MessageAttributeValue origHostVal = msg.MessageAttributes[TwilioStatusUpdateMessage.ORIGINAL_HOST_ATTRIBUTE];
                SQSEvent.MessageAttribute origHost = new SQSEvent.MessageAttribute();
                origHost.DataType = origHostVal.DataType;
                origHostText = origHostVal.StringValue;
                origHost.StringValue = origHostVal.StringValue;
                eventMessage.MessageAttributes.Add(TwilioStatusUpdateMessage.ORIGINAL_HOST_ATTRIBUTE, origHost);


                MessageAttributeValue origSchemeVal = msg.MessageAttributes[TwilioStatusUpdateMessage.ORIGINAL_SCHEME_ATTRIBUTE];
                SQSEvent.MessageAttribute origScheme = new SQSEvent.MessageAttribute();
                origScheme.DataType = origSchemeVal.DataType;
                origSchemeText = origSchemeVal.StringValue;
                origScheme.StringValue = origSchemeVal.StringValue;
                eventMessage.MessageAttributes.Add(TwilioStatusUpdateMessage.ORIGINAL_SCHEME_ATTRIBUTE, origScheme);


                MessageAttributeValue origPathVal = msg.MessageAttributes[TwilioStatusUpdateMessage.PATH_ATTRIBUTE];
                SQSEvent.MessageAttribute origPath = new SQSEvent.MessageAttribute();
                origPath.DataType = origPathVal.DataType;
                origPath.StringValue = origPathVal.StringValue;
                origPathText = origPathVal.StringValue;
                eventMessage.MessageAttributes.Add(TwilioStatusUpdateMessage.PATH_ATTRIBUTE, origPath);

                MessageAttributeValue validationTokenVal = msg.MessageAttributes[TwilioStatusUpdateMessage.VALIDATION_TOKEN_ATTRIBUTE];
                SQSEvent.MessageAttribute validationtokenAttrib = new SQSEvent.MessageAttribute();
                validationtokenAttrib.DataType = validationTokenVal.DataType;
                validationtokenAttrib.StringValue = validationTokenVal.StringValue;
                origValTokenText = validationTokenVal.StringValue;
                eventMessage.MessageAttributes.Add(TwilioStatusUpdateMessage.VALIDATION_TOKEN_ATTRIBUTE, validationtokenAttrib);


                UriBuilder uriBuilder = new UriBuilder();
                uriBuilder.Scheme = origSchemeText;
                uriBuilder.Host = origHostText;
                uriBuilder.Path = origPathText;

                string uriText = uriBuilder.ToString();

                //TODO get auth token from configuration
                //TODO get auth token from configuration

                string authToken = "";

                RequestValidatorClone requestValidator = new RequestValidatorClone(authToken);



                var queryVals = HttpUtility.ParseQueryString(msg.Body).ToDictionary();
                



                bool isValid = requestValidator.Validate(uriText, queryVals, origValTokenText);

                //if(msg.MessageAttributes!=null)
                //    eventMessage.MessageAttributes = msg.MessageAttributes;

                DeleteMessageBatchRequestEntry deleteEntry = new DeleteMessageBatchRequestEntry();
                deleteEntry.Id = msg.MessageId;
                deleteEntry.ReceiptHandle = msg.ReceiptHandle;
                deleteBatch.Entries.Add(deleteEntry);

                records.Add(eventMessage);
            }




            TwilioStatusUpdateFunction twilioFunc = new TwilioStatusUpdateFunction();

            

            SQSEvent sqsevent = new SQSEvent();
            sqsevent.Records = records;

            await twilioFunc.FunctionHandler(sqsevent, base.GetLambdaContext());





        }
    }
}
