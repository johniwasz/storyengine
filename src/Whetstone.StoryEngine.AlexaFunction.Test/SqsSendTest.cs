using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Whetstone.StoryEngine.AlexaFunction.Test
{
    public class SqsSendTest
    {

        [Fact]
        public async Task SendSqsMessageTest()
        {
            string sqsText = File.ReadAllText("importfiles/sqsmsg.json");


            SendMessageRequest sendRequest = JsonConvert.DeserializeObject<SendMessageRequest>(sqsText);

            RegionEndpoint endpoint = RegionEndpoint.USEast1;

            try
            {
                using (AmazonSQSClient sqsClient = new AmazonSQSClient(endpoint))
                {
                    SendMessageResponse sendResponse = await sqsClient.SendMessageAsync(sendRequest);
                }
                //    _logger.LogInformation(
                //        $"WhetstoneQueue sent in Message id {sendResponse.MessageId} to queue url {sendRequest.QueueUrl} and received https status code {sendResponse.HttpStatusCode}");
                //}
            }
            catch (Exception ex)
            {
                string requestmessage = JsonConvert.SerializeObject(sendRequest);
                throw new Exception($"Error sending queue message using queue url {sendRequest.QueueUrl}", ex);
            }

        }
    }
}
