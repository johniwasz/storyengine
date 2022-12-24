using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Notifications;
using Whetstone.StoryEngine.Notifications.Repository;
using Whetstone.StoryEngine.SocketApi.Repository;

namespace Whetstone.StoryEngine.CoreApi.WebSockets
{
    public class WebSocketManagerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISocketHandler _socketHandler;
        private readonly ILogger<WebSocketManagerMiddleware> _logger;
        private readonly IServiceProvider _services;
        private readonly INotificationProcessor _notificationProcessor;

        public WebSocketManagerMiddleware(RequestDelegate next,
                                            IServiceProvider services)
        {
            _next = next;
            _services = services;

            _socketHandler = _services.GetService<ISocketHandler>();
            _logger = _services.GetService<ILogger<WebSocketManagerMiddleware>>();
            _notificationProcessor = services.GetService<INotificationProcessor>();
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next.Invoke(context);
                return;
            }

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            AuthenticatedSocket authSocket = this.AuthenticateSocket(context, socket);

            if (authSocket != null)
            {
                await _socketHandler.OnConnect(authSocket);

                //
                // Tell the notification processor to send along any pending notifications
                // for the client Id and then go into listening mode.
                //
                NotificationRequest notificationReq = new NotificationRequest
                {
                    UserId = authSocket.UserId,
                    ClientId = authSocket.ClientId,
                    ConnectionId = authSocket.Id,
                    RequestType = NotificationRequestType.SendNotificationsForClient
                };

                _ = _notificationProcessor.ProcessNotification(notificationReq);

                await Receive(authSocket.Socket, async (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        // Read the message and then hand it off to the socket handler to send it
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await _socketHandler.OnReceiveMessage(authSocket.UserId, authSocket.Id, message);
                        return;
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _socketHandler.OnDisconnect(authSocket.UserId, authSocket.Id);
                        return;
                    }
                    else
                    {
                        throw new NotImplementedException($"Unsupported Socket Message: {result.MessageType}");
                    }

                });
            }
            else
            {
                await socket.CloseAsync(closeStatus: WebSocketCloseStatus.InternalServerError,
                                    statusDescription: "Forbidden",
                                    cancellationToken: CancellationToken.None);
            }
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                                                        cancellationToken: CancellationToken.None);

                handleMessage(result, buffer);
            }
        }

        private AuthenticatedSocket AuthenticateSocket(HttpContext context, WebSocket socket)
        {
            AuthenticatedSocket authSocket = null;

            string authToken = this.GetRequestQueryParam(context.Request, "auth");
            string apiKey = this.GetRequestQueryParam(context.Request, "api");
            string clientId = this.GetRequestQueryParam(context.Request, "clientId");

            if (!String.IsNullOrEmpty(authToken) && !String.IsNullOrEmpty(apiKey) && !String.IsNullOrEmpty(clientId))
            {
                string userId = null;

                ISocketResponse socketResponse = _socketHandler.ValidateSocketAuthToken(authToken, out userId);

                if (socketResponse.StatusCode == HttpStatusCode.OK)
                {
                    authSocket = new AuthenticatedSocket(socket, userId, authToken, clientId);
                }
                else
                {
                    _logger.LogError($"Unable to authenticate user. Status: {socketResponse.StatusCode}, Message: {socketResponse.Body}");
                }
            }
            else
            {
                _logger.LogDebug("authToken, apiKey and clientId must have values");
            }

            return authSocket;

        }

        private string GetRequestQueryParam(HttpRequest request, string queryParam)
        {
            string paramValue = String.Empty;

            if (request.Query.Count > 0 &&
                request.Query.ContainsKey(queryParam))
            {
                // URL Decode the parameter value
                paramValue = request.Query[queryParam];
                paramValue = System.Web.HttpUtility.UrlDecode(paramValue);
                _logger.LogDebug($"{queryParam} query value: {paramValue}");

            }
            else
            {
                _logger.LogDebug($"No {queryParam} query value found.");
            }

            return paramValue;
        }

    }
}
