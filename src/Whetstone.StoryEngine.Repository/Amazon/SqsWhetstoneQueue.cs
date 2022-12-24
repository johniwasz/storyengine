using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Repository.Messaging;

namespace Whetstone.StoryEngine.Repository.Amazon
{
    public class SqsWhetstoneQueue : IWhetstoneQueue
    {
        private readonly ILogger<SqsWhetstoneQueue> _logger;

        private readonly RegionEndpoint _endpoint;


        public SqsWhetstoneQueue(IOptions<EnvironmentConfig> envConfig, ILogger<SqsWhetstoneQueue> logger)
        {

            _endpoint = envConfig.Value?.Region ?? throw new ArgumentNullException(nameof(envConfig), "Region property cannot be null");


            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task AddMessageToQueueAsync<T>(string queuUrl, T message)
        {
            if (string.IsNullOrWhiteSpace(queuUrl))
                throw new ArgumentException(nameof(queuUrl));

            SendMessageRequest sendRequest = new SendMessageRequest
            {
                QueueUrl = queuUrl,
                MessageBody = JsonConvert.SerializeObject(message),
                MessageAttributes = new Dictionary<string, MessageAttributeValue>()
            };



            try
            {
                using (AmazonSQSClient sqsClient = new AmazonSQSClient(_endpoint))
                {
                    SendMessageResponse sendResponse = await sqsClient.SendMessageAsync(sendRequest);

                    _logger.LogInformation(
                        $"WhetstoneQueue sent in Message id {sendResponse.MessageId} to queue url {sendRequest.QueueUrl} and received https status code {sendResponse.HttpStatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error sending queue message using queue url {sendRequest.QueueUrl}", ex);
            }
        }

        public Task<string> GetMessageFromQueueAsync(string queueName)
        {
            throw new NotImplementedException();
        }
    }
}
