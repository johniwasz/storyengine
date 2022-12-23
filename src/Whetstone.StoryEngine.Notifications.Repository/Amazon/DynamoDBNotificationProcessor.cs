using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Notifications;
using Whetstone.StoryEngine.SocketApi.Repository;
using System.Net;

namespace Whetstone.StoryEngine.Notifications.Repository.Amazon
{
    public class DynamoDBNotificationProcessor : INotificationProcessor
    {
        private ILogger<DynamoDBNotificationProcessor> _logger;
        private IAmazonDynamoDB _ddbClient;
        private ISocketConnectionManager _connMgr;
        private IServiceProvider _services;

        public readonly string UserIdField = "userId";
        public readonly string NotificationIdField = "notificationId";
        public readonly string ClientIdField = "clientId";
        public readonly string NotificationTypeField = "notificationType";
        public readonly string NotificationField = "notification";
        public readonly string ExpirationTimeField = "expirationTime";

        /// <summary>
        /// DynamoDB table used to store the open connection ids. More advanced use cases could store logged on user map to their connection id to implement direct message chatting.
        /// </summary>
        private readonly string PendingNotificationsTableName;

        public DynamoDBNotificationProcessor( ILogger<DynamoDBNotificationProcessor> logger, IAmazonDynamoDB ddbClient, IOptions<SocketConfig> socketConfig, ISocketConnectionManager connMgr, IServiceProvider services)
        {
            _logger = logger;
            _ddbClient = ddbClient;
            _connMgr = connMgr;
            _services = services;

            PendingNotificationsTableName = socketConfig.Value.PendingNotificationsTableName;
        }

        public async Task ProcessNotification(NotificationRequest request)
        {
            this.ValidateNotificationRequest(request);

            _logger.LogDebug($"Processing RequestType: {request.RequestType}, IsRegisteredUser: {request.UserId}, ClientId: {request.ClientId}, ConnectionId: {request.ConnectionId}");

            if ( request.RequestType == NotificationRequestType.SendNotificationToClient )
            {
                await this.SendNotificationToClient(request);
            }
            else
            {
                await this.SendNotificationsForClient(request);
            }
        }

        private async Task SendNotificationToClient(NotificationRequest request)
        {
            NotificationMessage message = this.NotificationMessageFromNotificationRequest(request);

            ICollection<IAuthenticatedSocket> clientSockets = await this._connMgr.GetSocketsForClientIdAsync(request.UserId, request.ClientId);

            if (clientSockets.Count > 0)
            {
                string messageData = JsonConvert.SerializeObject(message);

                ISocketMessageSender messageSender = _services.GetService<ISocketMessageSender>();
                bool sentSuccessfully = false;

                foreach (IAuthenticatedSocket socket in clientSockets)
                {
                    if ( await this.WriteMessageToSocket(messageData, messageSender, socket) )
                    {
                        sentSuccessfully = true;
                    }
                }

                // If the notification wasn't successfully sent to at least one socket
                // stick it in the pending notifications table
                if (!sentSuccessfully)
                {
                    await this.AddPendingNotificationToTableAsync(message);
                }
            }
            else
            {
                await this.AddPendingNotificationToTableAsync(message);
            }
        }

        private async Task SendNotificationsForClient(NotificationRequest request)
        {
            ICollection<NotificationMessage> clientMessages = await this.GetUserNotificationsForClientId(request.UserId, request.ClientId);

            if ( clientMessages.Count > 0 )
            {
                IAuthenticatedSocket socket = await _connMgr.GetSocketByIdAsync(request.UserId, request.ConnectionId);

                if (socket != null)
                {
                    ISocketMessageSender messageSender = _services.GetService<ISocketMessageSender>();

                    foreach (NotificationMessage message in clientMessages)
                    {
                        string messageData = JsonConvert.SerializeObject(message);

                        // If we successfully write data to the socket, remove the notification otherwise
                        // additional writes will probably fail so just bail out
                        if ( await this.WriteMessageToSocket(messageData, messageSender, socket) )
                        {
                            await this.RemovePendingNotificationAsync(message.UserId, message.NotificationId);
                        }
                        else
                        {
                            _logger.LogDebug("Socket write failed - abandoning message sends");
                            break;
                        }

                    }

                }
                else
                {
                    _logger.LogError($"Unable to find socket for userId: {request.UserId}, connectionId: {request.ConnectionId}");
                }

            }

        }

        private void ValidateNotificationRequest( NotificationRequest request )
        {
            if (request == null)
                throw new ArgumentNullException("ValidateNotificationRequest - request cannot be null");

            if ( String.IsNullOrEmpty(request.UserId) )
                throw new InvalidOperationException("ValidateNotificationRequest - IsRegisteredUser cannot be empty");

            if (String.IsNullOrEmpty(request.ClientId))
                throw new InvalidOperationException("ValidateNotificationRequest - ClientId cannot be empty");

            if ( request.RequestType == NotificationRequestType.SendNotificationToClient )
            {
                if (String.IsNullOrEmpty(request.NotificationId))
                    throw new InvalidOperationException("ValidateNotificationRequest SendNotificationToClient - NotificationId cannot be empty");

                if (request.NotificationType == NotificationDataType.None)
                    throw new InvalidOperationException($"ValidateNotificationRequest SendNotificationToClient - NotificationType cannot be None");

                if (String.IsNullOrEmpty(request.Data))
                    throw new InvalidOperationException("ValidateNotificationRequest SendNotificationToClient - Data cannot be empty");

            }
            else if ( request.RequestType == NotificationRequestType.SendNotificationsForClient )
            {
                if (String.IsNullOrEmpty(request.ConnectionId))
                    throw new InvalidOperationException("ValidateNotificationRequest SendNotificationsForClient - ConnectionId cannot be empty");

                if (request.NotificationType != NotificationDataType.None)
                    throw new InvalidOperationException($"ValidateNotificationRequest SendNotificationForClient - NotificationType cannot be {request.NotificationType}");

            }
            else
            {
                throw new InvalidOperationException($"ValidateNotificationRequest RequestType: {request.RequestType} not supported");
            }

        }

        private NotificationMessage NotificationMessageFromNotificationRequest( NotificationRequest request )
        {
            if (request.RequestType != NotificationRequestType.SendNotificationToClient)
                throw new InvalidOperationException($"Cannot create NotificationMessage from request type: {request.RequestType}");

            if (request.NotificationType == NotificationDataType.None)
                throw new InvalidOperationException($"Cannot create NotificationMessage from notification type: {request.NotificationType}");

            return new NotificationMessage
            {
                UserId = request.UserId,
                ClientId = request.ClientId,
                NotificationId = request.NotificationId,
                NotificationType = request.NotificationType,
                Data = request.Data
            };

        }

        public virtual async Task AddPendingNotificationToTableAsync(NotificationMessage notification)
        {
            // Expiration time is now UTC plus 5 minutes
            long socketTTL = SocketExpirationHelper.GetNewSocketTTL();
            var ddbRequest = new PutItemRequest
            {
                TableName = PendingNotificationsTableName,
                Item = new Dictionary<string, AttributeValue>
                    {
                        {UserIdField, new AttributeValue{ S = notification.UserId } },
                        {NotificationIdField, new AttributeValue{ S = notification.NotificationId}},
                        {ClientIdField, new AttributeValue{ S = notification.ClientId}},
                        {NotificationTypeField, new AttributeValue{ S = notification.NotificationType.ToString()}},
                        {NotificationField, new AttributeValue{ S = notification.Data}},
                        {ExpirationTimeField, new AttributeValue{ N = $"{socketTTL}" } }
                    }
            };

            _logger.LogDebug("Putting pending notification into DynamoDB");

            await _ddbClient.PutItemAsync(ddbRequest);
        }

        public virtual async Task RemovePendingNotificationAsync(string userId, string notificationId)
        {
            _logger.LogDebug("Removing notification from DynamoDB");

            var ddbRequest = new DeleteItemRequest
            {
                TableName = PendingNotificationsTableName,
                Key = new Dictionary<string, AttributeValue>
                    {
                        {UserIdField, new AttributeValue { S = userId } },
                        {NotificationIdField, new AttributeValue {S = notificationId}}
                    }
            };

            await _ddbClient.DeleteItemAsync(ddbRequest);
        }

        private async Task<ICollection<NotificationMessage>> GetUserNotificationsForClientId( string userId, string clientId )
        {
            List<NotificationMessage> lstMessages = new List<NotificationMessage>();

            // Filter by user id, client Id and expiration time of now
            long socketExpiration = SocketExpirationHelper.GetSocketExpirationNow();
            Dictionary<string, AttributeValue> expressionAttrValues = new Dictionary<string, AttributeValue>
            {
                {":v_UId", new AttributeValue { S = userId } },
                {":v_ClientId", new AttributeValue { S = clientId } },
                {":v_ExpTime", new AttributeValue { N = $"{socketExpiration}" } }
            };

            // Get current connections for the user that have not expired
            var queryRequest = new QueryRequest
            {
                TableName = PendingNotificationsTableName,
                KeyConditionExpression = $"{UserIdField} = :v_UId",
                FilterExpression = $"{ExpirationTimeField} > :v_ExpTime AND {ClientIdField} = :v_ClientId",
                ExpressionAttributeValues = expressionAttrValues,
                ProjectionExpression = $"{UserIdField},{NotificationIdField},{ClientIdField},{NotificationTypeField},{NotificationField}"
            };

            var queryResponse = await _ddbClient.QueryAsync(queryRequest);

            _logger.LogDebug($"GetUserNotificationsForClientId: IsRegisteredUser: {userId}, ClientId: {clientId} returned {queryResponse.Items.Count} notifications.");

            // Generally, there should be only one socket, but since sockets can get torn out from underneath us,
            // we may not have been informed that the socket was disconnected and end up with >1 socket for a unique client id
            // Return them all and let the write code remove any old sockets.

            foreach (var item in queryResponse.Items)
            {
                // These three values are needed to build an authToken
                string uId = item[UserIdField].S;
                string notificationId = item[NotificationIdField].S;
                string cId = item[ClientIdField].S;
                NotificationDataType notificationType = (NotificationDataType) Enum.Parse(typeof(NotificationDataType), item[NotificationTypeField].S);
                string notification = item[NotificationField].S;

                NotificationMessage message = new NotificationMessage
                {
                    UserId = uId,
                    ClientId = cId,
                    NotificationId = notificationId,
                    NotificationType = notificationType,
                    Data = notification
                };

                lstMessages.Add(message);
            }

            return lstMessages;
        }

        private async Task<bool> WriteMessageToSocket(string messageData, ISocketMessageSender messageSender, IAuthenticatedSocket socket)
        {
            bool sentSuccessfully = false;

            try
            {
                await messageSender.SendMessageToClient(socket, messageData);
                await _connMgr.UpdateSocketTTL(socket.UserId, socket.Id);
                sentSuccessfully = true;
            }
            catch (SocketServiceException e)
            {
                // If we get a status of 410 GONE then the connection is no
                // longer available. If this happens, delete the identifier
                // from our connection.
                if (e.StatusCode == HttpStatusCode.Gone)
                {
                    _logger.LogDebug($"Deleting gone connection: {socket.Id}");
                    await _connMgr.RemoveSocketAsync(socket.UserId, socket.Id);
                }
                else
                {
                    _logger.LogDebug($"Error posting message to {socket.Id}: {e.Message}");
                    _logger.LogDebug(e.StackTrace);
                }
            }

            return sentSuccessfully;
        }

    }


}

