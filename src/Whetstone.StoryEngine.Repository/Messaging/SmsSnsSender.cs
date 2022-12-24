using Amazon;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Messaging.Sms;

namespace Whetstone.StoryEngine.Repository.Messaging
{
    public class SmsSnsSender : ISmsSender
    {

        private readonly ILogger<SmsSnsSender> _logger;

        public SmsSenderType ProviderName => SmsSenderType.Sns;

        private RegionEndpoint _regionEndpoint = null;


        public SmsSnsSender(IOptions<EnvironmentConfig> envConfig, ILogger<SmsSnsSender> logger)
        {


            if (envConfig == null)
                throw new ArgumentNullException($"{nameof(envConfig)}");

            _regionEndpoint = envConfig.Value?.Region ?? throw new ArgumentNullException($"{nameof(envConfig)}", "Region cannot be null");

            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");

        }

        public async Task<OutboundMessageLogEntry> SendSmsMessageAsync(SmsMessageRequest messageRequest)
        {

            if (messageRequest == null)
                throw new ArgumentNullException($"{nameof(messageRequest)}");

            if (string.IsNullOrWhiteSpace(messageRequest.DestinationNumber))
                throw new ArgumentNullException($"{nameof(messageRequest)}", "DestinationNumber cannot be null or empty");

            if (string.IsNullOrWhiteSpace(messageRequest.Message))
                throw new ArgumentNullException($"{nameof(messageRequest)}", "Message cannot be null or empty");


            AmazonSimpleNotificationServiceClient snsClient = new AmazonSimpleNotificationServiceClient(_regionEndpoint);
            PublishRequest pubRequest = new PublishRequest();
            pubRequest.Message = messageRequest.Message;
            pubRequest.PhoneNumber = messageRequest.DestinationNumber;


            // add optional MessageAttributes, for example:
            //   pubRequest.MessageAttributes.Add("AWS.SNS.SMS.SenderID", new MessageAttributeValue
            //      { StringValue = "SenderId", DataType = "String" });
            PublishResponse pubResponse = await snsClient.PublishAsync(pubRequest);


            OutboundMessageLogEntry msgResult = ToMessageResult(pubResponse);

            return msgResult;
        }

        public OutboundMessageLogEntry ToMessageResult(PublishResponse pubResponse)
        {
            OutboundMessageLogEntry res = new OutboundMessageLogEntry();
            res.HttpStatusCode = (int)pubResponse.HttpStatusCode;
            res.LogTime = DateTime.UtcNow;
            res.ServiceMessageId = pubResponse.MessageId;

            if (pubResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogDebug($"Successfully sent sns message {pubResponse.MessageId}");
                res.SendStatus = MessageSendStatus.SentToDispatcher;
            }
            // Too Many Requests
            else if ((int)pubResponse.HttpStatusCode == 429)
            {
                _logger.LogWarning($"Sns message {pubResponse.MessageId} is throttled");
                res.SendStatus = MessageSendStatus.Throttled;
            }
            else
            {
                _logger.LogError($"Unexpected http response {res.HttpStatusCode} sending sns message {pubResponse.MessageId}");
                res.SendStatus = MessageSendStatus.Error;
            }

            return res;
        }

    }
}
