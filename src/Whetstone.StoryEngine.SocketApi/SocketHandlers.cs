using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

using Amazon.Runtime;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;

using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Notifications;
using Whetstone.StoryEngine.SocketApi.Repository;
using Whetstone.StoryEngine.Repository.Amazon;
using Whetstone.StoryEngine.Notifications.Repository;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

//
// Source URLs on how to work with API Gateway, Websockets and .NET:
// Overview of securing web sockets in an API Gateway:
// https://medium.com/swlh/implementing-secure-web-sockets-with-aws-api-gateway-cognito-dynamodb-and-lambda-b38e02314b42
// Example of doing a custom authorizer lambda using .NET Core 2.0
// https://github.com/RecursiveLoop/GrandmasRecipes
// Example of putting a custom authorizer in place using using Cloud Formation
// https://cloudonaut.io/api-gateway-custom-authorization-with-lambda-dynamodb-and-cloudformation/
//


namespace Whetstone.StoryEngine.SocketApi
{
    public class SocketHandlers
    {
        private readonly ISocketHandler _socketHandler;

        private readonly ILogger _logger = null;

        private readonly string _endpoint = null;

        private readonly INotificationProcessor _notificationProcessor;

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public SocketHandlers( ILogger logger, ISocketHandler socketHandler, SocketConfig socketConfig, INotificationProcessor notificationProcessor )
        {
            if (socketHandler == null)
                throw new ArgumentNullException("SocketHandlers constructor socketHandler param cannot be null");

            if (socketConfig == null)
                throw new ArgumentNullException("SocketHandlers constructor socketConfig param cannot be null");

            if (socketConfig.SocketWriteEndpoint == null)
                throw new ArgumentNullException("SocketHandlers constructor socketConfig.SocketWriteEndpoint param cannot be null");

            if (notificationProcessor == null)
                throw new ArgumentNullException("SocketHandlers constructor notificationProcessor param cannot be null");

            _socketHandler = socketHandler;
            _logger = logger;
            _endpoint = socketConfig.SocketWriteEndpoint;
            _notificationProcessor = notificationProcessor;

            _logger.LogDebug($"API Gateway management endpoint: {_endpoint}");

        }

        #region Connect/Disconnect
        public async Task<APIGatewayProxyResponse> OnConnectHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var connectionId = request.RequestContext.ConnectionId;
            this.LogAuthorizationContext(request, context);

            IAuthenticatedSocket authSocket = CreateAuthSocketFromRequest(request, context);
            ISocketResponse response = await _socketHandler.OnConnect(authSocket);

            if ( response.StatusCode == HttpStatusCode.OK )
            {
                //
                // Tell the notification processor to send along any pending notifications
                // for the client Id
                //
                NotificationRequest notificationReq = new NotificationRequest
                {
                    UserId = authSocket.UserId,
                    ClientId = authSocket.ClientId,
                    ConnectionId = authSocket.Id,
                    RequestType = NotificationRequestType.SendNotificationsForClient
                };

                // This is an async fire and forget method
                await _notificationProcessor.ProcessNotification(notificationReq);
            }

            return new APIGatewayProxyResponse
            {
                StatusCode = (int) response.StatusCode,
                Body = response.Body
            };

        }

        public async Task<APIGatewayProxyResponse> OnDisconnectHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var connectionId = request.RequestContext.ConnectionId;
            var userId = this.GetUserIdFromRequest(request);

            this.LogAuthorizationContext(request, context);

            ISocketResponse response = await _socketHandler.OnDisconnect(userId, connectionId);

            return new APIGatewayProxyResponse
            {
                StatusCode = (int) response.StatusCode,
                Body = response.Body
            };

        }
        
            #endregion

            #region SendMessage
        public async Task<APIGatewayProxyResponse> SendMessageHandler(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var connectionId = request.RequestContext.ConnectionId;
            var userId = this.GetUserIdFromRequest(request);

            // Construct the API Gateway endpoint that incoming message will be broadcasted to.
            var domainName = request.RequestContext.DomainName;
            var stage = request.RequestContext.Stage;

            ISocketResponse response = await _socketHandler.OnReceiveMessage(userId, connectionId, request.Body);

            return new APIGatewayProxyResponse
            {
                StatusCode = (int) response.StatusCode,
                Body = response.Body
            };

        }
        #endregion

        private void LogAuthorizationContext(APIGatewayProxyRequest request, ILambdaContext context)
        {
            if (request.RequestContext.Authorizer != null)
            {
                StringBuilder sb = new StringBuilder("Authorizer values:\n");
                sb.Append($"Principal Id: {request.RequestContext.Authorizer["principalId"]}\n");
                foreach (string key in request.RequestContext.Authorizer.Keys)
                {
                    sb.Append($"{key}: {request.RequestContext.Authorizer[key]}\n");
                }

                _logger.LogDebug(sb.ToString());
            }
            else
            {
                _logger.LogDebug("Authorizer value is empty");
            }
        }

        private string GetUserIdFromRequest(APIGatewayProxyRequest request)
        {
            return request.RequestContext.Authorizer["principalId"].ToString();
        }

        private IAuthenticatedSocket CreateAuthSocketFromRequest(APIGatewayProxyRequest request, ILambdaContext context)
        {
            string connectionId = request.RequestContext.ConnectionId;
            string userId = request.RequestContext.Authorizer["principalId"].ToString();
            string authToken = request.RequestContext.Authorizer["Auth"].ToString();
            string clientId = request.RequestContext.Authorizer["ClientId"].ToString();

            this.LogAuthorizationContext(request, context);

            return new AuthSocketBase(connectionId, userId, authToken, clientId);
        }

    }

}