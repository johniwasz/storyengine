using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.SocketApi.Repository.Amazon
{
    public class ApiGatewayMessageSender : ISocketMessageSender
    {
        /// <summary>
        /// Factory func to create the AmazonApiGatewayManagementApiClient. This is needed to created per endpoint of the a connection. It is a factory to make it easy for tests
        /// to moq the creation.
        /// </summary>
        Func<string, IAmazonApiGatewayManagementApi> ApiGatewayManagementApiClientFactory { get; }

        private readonly ILogger<ApiGatewayMessageSender> _logger;
        private readonly string _endpoint;

        public ApiGatewayMessageSender(ILogger<ApiGatewayMessageSender> logger, IOptions<SocketConfig> socketConfig)
        {
            this.ApiGatewayManagementApiClientFactory = (Func<string, AmazonApiGatewayManagementApiClient>)((endpoint) =>
            {
                return new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
                {
                    ServiceURL = endpoint
                });
            });

            _logger = logger;
            _endpoint = socketConfig.Value.SocketWriteEndpoint;
        }

        public async Task SendMessageToClient(IAuthenticatedSocket authSocket, string message, params KeyValuePair<string, object>[] parameters)
        {
            try
            {

                // Construct the IAmazonApiGatewayManagementApi which will be used to send the message to.
                var apiClient = ApiGatewayManagementApiClientFactory(_endpoint);
                var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(message));

                var postConnectionRequest = new PostToConnectionRequest
                {
                    ConnectionId = authSocket.Id,
                    Data = stream
                };

                _logger.LogDebug($"Posting message to socket: {authSocket.Id}");
                stream.Position = 0;
                await apiClient.PostToConnectionAsync(postConnectionRequest);
            }
            catch (AmazonServiceException ex)
            {
                throw new SocketServiceException(ex.Message, ex.StatusCode, ex);
            }
        }

        private static string GetStringParameter(string key, KeyValuePair<string, object>[] parameters)
        {
            string str = null;

            foreach (KeyValuePair<string, object> kvp in parameters)
            {
                if (kvp.Key == key)
                {
                    str = kvp.Value.ToString();
                    break;
                }

            }

            return str;

        }

    }
}
