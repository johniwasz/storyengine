using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Security;
using Whetstone.StoryEngine.SocketApi.Repository.Notifications;
using Whetstone.StoryEngine.SocketApi.Repository.Responses;

namespace Whetstone.StoryEngine.SocketApi.Repository
{
    public class SocketHandler : ISocketHandler
    {
        private ILogger<SocketHandler> _logger;
        private ISocketConnectionManager _connMgr;
        private IServiceProvider _services;

        private readonly string SENDMESSAGE_COMMAND = "sendMessage";

        public SocketHandler(ILogger<SocketHandler> logger, ISocketConnectionManager connMgr, IServiceProvider services)
        {
            _logger = logger;
            _connMgr = connMgr;
            _services = services;
        }

        public ISocketResponse ValidateSocketAuthToken(string authToken, out string userId)
        {
            SocketResponse socketResponse = new SocketResponse()
            {
                StatusCode = HttpStatusCode.Forbidden,
                Body = $"Forbidden"
            };

            IWebAuthorizer authorizer = _services.GetService<IWebAuthorizer>();
            userId = null;

            if (!String.IsNullOrEmpty(authToken))
            {
                try
                {
                    userId = authorizer.GetValidUserIdFromAuthToken(authToken);
                }
                catch (TokenIsExpiredException)
                {
                    _logger.LogError("Caught TokenIsExpiredException");
                    socketResponse.StatusCode = HttpStatusCode.Unauthorized;
                    socketResponse.Body = "TokenExpired";
                }
                catch (TokenValidationException tokenException)
                {
                    _logger.LogError("Auth Token Exception: " + tokenException.ToString());
                    socketResponse.StatusCode = HttpStatusCode.Unauthorized;
                    socketResponse.Body = "TokenInvalid";
                }

                if (!String.IsNullOrEmpty(userId))
                {
                    socketResponse.StatusCode = HttpStatusCode.OK;
                    socketResponse.Body = "OK";
                }

            }

            return socketResponse;
        }

        public async Task<ISocketResponse> OnConnect(IAuthenticatedSocket authSock)
        {
            try
            {
                _logger.LogDebug($"ConnectionId: {authSock.Id}");

                await _connMgr.AddSocketAsync(authSock);

                return new SocketResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Body = "Connected."
                };
            }
            catch (Exception e)
            {
                _logger.LogDebug("Error connecting: " + e.Message);
                _logger.LogDebug(e.StackTrace);
                return new SocketResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Body = $"Failed to connect: {e.Message}"
                };
            }
        }

        public async Task<ISocketResponse> OnDisconnect(string userId, string connectionId, params KeyValuePair<string, object>[] parameters)
        {
            try
            {
                _logger.LogDebug($"ConnectionId: {connectionId}");

                await _connMgr.RemoveSocketAsync(userId, connectionId);

                return new SocketResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Body = "Disconnected."
                };
            }
            catch (Exception e)
            {
                _logger.LogDebug("Error disconnecting: " + e.Message);
                _logger.LogDebug(e.StackTrace);
                return new SocketResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Body = $"Failed to disconnect: {e.Message}"
                };
            }
        }

        public async Task<ISocketResponse> OnReceiveMessage(string userId, string connectionId, string clientMessage, params KeyValuePair<string, object>[] parameters)
        {
            try
            {
                if (parameters == null)
                    throw new InvalidOperationException("OnSendMessage missing parameters!");

                ISocketMessageSender socketSender = _services.GetService<ISocketMessageSender>();

                // The body will look something like this: {"message":"sendMessage", "authToken": "auth", "clientMsgId": 1, "data":"What are you doing?"}
                dynamic jsonObject = JsonConvert.DeserializeObject<ExpandoObject>(clientMessage);

                // Make sure the user is authenticated
                string authTokenProperty = this.GetJsonStringProperty(jsonObject, "authToken");

                ISocketResponse socketResponse = this.ValidateAuthTokenForUserId(userId, authTokenProperty);

                if (socketResponse.StatusCode == HttpStatusCode.OK)
                {

                    long clientMsgId;
                    string clientMsg;

                    socketResponse = this.ParseSendMessage(jsonObject, out clientMsgId, out clientMsg);

                    if (socketResponse.StatusCode == HttpStatusCode.OK)
                    {
                        string requestId = Guid.NewGuid().ToString();

                        socketResponse = await this.ProcessSendMessageResult(userId, connectionId, requestId, clientMsgId, socketSender, parameters);

                        if (socketResponse.StatusCode == HttpStatusCode.OK)
                        {
                            socketResponse = await this.ProcessSendMessage(userId, connectionId, requestId, clientMsg, socketSender, parameters);
                        }
                    }

                }

                return socketResponse;
            }
            catch (Exception e)
            {
                _logger.LogDebug("Error disconnecting: " + e.Message);
                _logger.LogDebug(e.StackTrace);
                return new SocketResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Body = $"Failed to receive message: {e.Message}"
                };
            }
        }

        private ISocketResponse ValidateAuthTokenForUserId(string userId, string authToken)
        {
            string tokenUser = null;

            ISocketResponse socketResponse = this.ValidateSocketAuthToken(authToken, out tokenUser);

            if (socketResponse.StatusCode == HttpStatusCode.OK)
            {
                if (tokenUser != userId)
                {
                    _logger.LogDebug($"Message TokenUser: {tokenUser} does not match socket {userId}");
                    socketResponse.StatusCode = HttpStatusCode.Unauthorized;
                    socketResponse.Body = $"Unauthorized";
                }
            }

            return socketResponse;

        }

        private ISocketResponse ParseSendMessage(dynamic message, out long clientMsgId, out string clientMessage)
        {
            SocketResponse socketResponse = new SocketResponse
            {
                StatusCode = HttpStatusCode.BadRequest
            };

            clientMsgId = 0L;
            clientMessage = null;

            // Validate that we recognize the incoming message.
            string messageProperty = this.GetJsonStringProperty(message, "message");

            if (!String.IsNullOrEmpty(messageProperty))
            {
                if (messageProperty == SENDMESSAGE_COMMAND)
                {
                    clientMsgId = this.GetJsonLongProperty(message, "clientMsgId");

                    if (clientMsgId > 0)
                    {
                        clientMessage = this.GetJsonStringProperty(message, "data");

                        if (!String.IsNullOrEmpty(clientMessage))
                        {
                            socketResponse.StatusCode = HttpStatusCode.OK;
                        }
                        else
                        {
                            socketResponse.Body = "Missing Data Property";

                        }
                    }
                    else
                    {
                        socketResponse.Body = $"Invalid ClientMsgId {clientMsgId}";

                    }

                }
                else
                {
                    socketResponse.Body = $"Unrecognized message type: {messageProperty}";

                }
            }
            else
            {
                socketResponse.Body = $"Unable to find message element";
            }

            return socketResponse;
        }

        private async Task<ISocketResponse> ProcessSendMessageResult(string userId, string connectionId, string requestId, long clientMsgId, ISocketMessageSender sender, params KeyValuePair<string, object>[] parameters)
        {
            SocketResponse socketResponse = new SocketResponse();

            SendMessageResponse sendMsgResponse = new SendMessageResponse
            {
                StatusCode = HttpStatusCode.OK,
                RequestId = requestId,
                ClientMsgId = clientMsgId,
                Body = String.Empty
            };

            string response = JsonConvert.SerializeObject(sendMsgResponse);

            IAuthenticatedSocket socket = await _connMgr.GetSocketByIdAsync(userId, connectionId);

            if (socket != null)
            {
                await sender.SendMessageToClient(socket, response, parameters);
                await _connMgr.UpdateSocketTTL(userId, connectionId);
                socketResponse.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                socketResponse.StatusCode = HttpStatusCode.InternalServerError;
                socketResponse.Body = $"Socket connection: {connectionId} not found.";
            }

            return socketResponse;

        }

        private async Task<ISocketResponse> ProcessSendMessage(string userId, string connectionId, string requestId, string clientMessage, ISocketMessageSender sender, params KeyValuePair<string, object>[] parameters)
        {
            SendMessageNotification notification = new SendMessageNotification
            {
                StatusCode = HttpStatusCode.OK,
                RequestId = requestId,
                UserId = userId,
                Body = clientMessage
            };

            string notificationData = JsonConvert.SerializeObject(notification);

            var lstConnectedSockets = await _connMgr.GetSocketsForUserIdAsync(userId);

            // Loop through all of the connections and broadcast the message out to the connections.
            var count = 0;
            foreach (var item in lstConnectedSockets)
            {
                try
                {
                    _logger.LogDebug($"Post to connection {count}: {item.Id}");
                    await sender.SendMessageToClient(item, notificationData, parameters);
                    await _connMgr.UpdateSocketTTL(item.UserId, item.Id);
                    count++;
                }
                catch (SocketServiceException e)
                {
                    // If we get a status of 410 GONE then the connection is no
                    // longer available. If this happens, delete the identifier
                    // from our connection.
                    if (e.StatusCode == HttpStatusCode.Gone)
                    {
                        _logger.LogDebug($"Deleting gone connection: {item.Id}");
                        await _connMgr.RemoveSocketAsync(item.UserId, item.Id);
                    }
                    else
                    {
                        _logger.LogDebug($"Error posting message to {item.Id}: {e.Message}");
                        _logger.LogDebug(e.StackTrace);
                    }
                }
            }

            return new SocketResponse
            {
                StatusCode = HttpStatusCode.OK,
                Body = "Data sent to " + count + " connection" + (count == 1 ? "" : "s")
            };

        }

        private string GetJsonStringProperty(ExpandoObject jsonObject, string propertyName)
        {
            string strValue = null;

            IDictionary<string, object> dict = (IDictionary<string, object>)jsonObject;

            if (dict.ContainsKey(propertyName))
            {
                strValue = dict[propertyName].ToString();
            }

            return strValue;
        }

        private long GetJsonLongProperty(ExpandoObject jsonObject, string propertyName)
        {
            long lValue = 0;

            IDictionary<string, object> dict = (IDictionary<string, object>)jsonObject;

            if (dict.ContainsKey(propertyName))
            {
                lValue = (long)dict[propertyName];//long.Parse(dict[propertyName].ToString());
            }


            return lValue;
        }

    }
}
