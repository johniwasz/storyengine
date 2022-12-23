using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Whetstone.StoryEngine;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Repository.Messaging;
using System.Web;
using Whetstone.StoryEngine.DependencyInjection;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Whetstone.Queue.TwiliosStatusUpdate
{

    public class TwilioStatusUpdateFunction : EngineLambdaBase
    {
       

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public TwilioStatusUpdateFunction()
        {

        }



#pragma warning disable IDE0060 // Remove unused parameter
        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
#pragma warning restore IDE0060 // Remove unused parameter
        {


            ITwilioStatusCallbackHandler statusCallbackHandler = Services.GetService<ITwilioStatusCallbackHandler>();


            foreach (var message in evnt.Records)
            {
                await ProcessMessageAsync(message, statusCallbackHandler); 
            }
        }

        private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ITwilioStatusCallbackHandler statusCallbackHandler)
        {

            ILogger<TwilioStatusUpdateFunction> logger = Services.GetService<ILogger<TwilioStatusUpdateFunction>>();

            if (message == null)
            {
                logger.LogError("SQS event is null");
                return;
            }


            TwilioStatusUpdateMessage twilioUpdate = GetTwilioStatusUpdateMessage(message);

            await statusCallbackHandler.ProcessTwilioStatusCallbackAsync(twilioUpdate);

            if (!string.IsNullOrWhiteSpace(twilioUpdate.QueueMessageId))
                throw new Exception($"Cannot process message {twilioUpdate.QueueMessageId}");


        }

        private TwilioStatusUpdateMessage GetTwilioStatusUpdateMessage(SQSEvent.SQSMessage message)
        {

            string messageId = message.MessageId;
            
            
            if(message.MessageAttributes==null)
                throw new Exception($"Message id {message.MessageId} has no attributes. Attributes are expected");

            var originalScheme = message.MessageAttributes.ContainsKey(TwilioStatusUpdateMessage.ORIGINAL_SCHEME_ATTRIBUTE) ? message.MessageAttributes[TwilioStatusUpdateMessage.ORIGINAL_SCHEME_ATTRIBUTE] : null;

            var originalHost = message.MessageAttributes.ContainsKey(TwilioStatusUpdateMessage.ORIGINAL_HOST_ATTRIBUTE) ? message.MessageAttributes[TwilioStatusUpdateMessage.ORIGINAL_HOST_ATTRIBUTE] : null;

            var originalPath = message.MessageAttributes.ContainsKey(TwilioStatusUpdateMessage.PATH_ATTRIBUTE) ? message.MessageAttributes[TwilioStatusUpdateMessage.PATH_ATTRIBUTE] : null;

            var validationToken = message.MessageAttributes.ContainsKey(TwilioStatusUpdateMessage.VALIDATION_TOKEN_ATTRIBUTE) ? message.MessageAttributes[TwilioStatusUpdateMessage.VALIDATION_TOKEN_ATTRIBUTE] : null;


            StringBuilder messageAttribError = new StringBuilder();

            if (string.IsNullOrWhiteSpace(originalScheme.StringValue))
                messageAttribError.AppendLine($"expected message attribute {TwilioStatusUpdateMessage.ORIGINAL_SCHEME_ATTRIBUTE} not found");

            if (string.IsNullOrWhiteSpace(originalHost.StringValue))
                messageAttribError.AppendLine($"expected message attribute {TwilioStatusUpdateMessage.ORIGINAL_HOST_ATTRIBUTE} not found");

            if (string.IsNullOrWhiteSpace(originalPath.StringValue))
                messageAttribError.AppendLine($"expected message attributed {TwilioStatusUpdateMessage.PATH_ATTRIBUTE} not found");

            if (string.IsNullOrWhiteSpace(validationToken.StringValue))
                messageAttribError.AppendLine($"expected message attribute {TwilioStatusUpdateMessage.VALIDATION_TOKEN_ATTRIBUTE} not found");

            string attributeCheckResult = messageAttribError.ToString();

            if (!string.IsNullOrWhiteSpace(attributeCheckResult))
                throw new Exception(string.Concat($"Message attribute issues with message id {messageId}:", attributeCheckResult));
            
            UriBuilder originalUrlBuilder = new UriBuilder
            {
                Scheme = originalScheme.StringValue,
                Host = originalHost.StringValue,
                Path = originalPath.StringValue
            };

            ILogger<TwilioStatusUpdateFunction> logger = Services.GetService<ILogger<TwilioStatusUpdateFunction>>();


            // Assemble the URI
            Uri originalUri;
            try
            {
                originalUri = originalUrlBuilder.Uri;
                logger.LogInformation($"Return url is {originalUri.ToString()} for message id {messageId}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing queue message {messageId}: Could not reconstruct url from original scheme: {originalScheme}, originalHost: {originalHost}, path: {originalPath}", ex);
            }

            if (string.IsNullOrWhiteSpace(message.Body))
                throw new Exception($"Message body for queue message {messageId} is null or empty");

            TwilioStatusUpdateMessage twilioStatusMessage = new TwilioStatusUpdateMessage
            {
                QueueMessageId = messageId,
                ValidationToken = validationToken.StringValue
            };

            logger.LogInformation($"Validation token for message id {messageId}: {twilioStatusMessage.ValidationToken}");

            twilioStatusMessage.OriginalUrl = originalUri;

            logger.LogInformation($"Message body for message id {messageId}: {message.Body}");

            twilioStatusMessage.MessageBody = HttpUtility.ParseQueryString(message.Body).ToDictionary();
            

            return twilioStatusMessage;


        }
    }
}
