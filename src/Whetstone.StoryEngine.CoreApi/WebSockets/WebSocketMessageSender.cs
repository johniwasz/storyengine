using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.WebSockets;

using Microsoft.Extensions.Logging;

using Whetstone.StoryEngine.SocketApi.Repository;
using System.Threading;

namespace Whetstone.StoryEngine.CoreApi.WebSockets
{
    public class WebSocketMessageSender : ISocketMessageSender
    {

        public WebSocketMessageSender()
        {
        }

        public async Task SendMessageToClient(IAuthenticatedSocket authSocket, string message, params KeyValuePair<string, object>[] parameters)
        {
            if (authSocket.Socket.State != WebSocketState.Open)
            {
                throw new SocketServiceException($"Socket id {authSocket.Id} already closed", HttpStatusCode.Gone);
            }

            await authSocket.Socket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.ASCII.GetBytes(message),
                                                                    offset: 0,
                                                                    count: message.Length),
                                    messageType: WebSocketMessageType.Text,
                                    endOfMessage: true,
                                    cancellationToken: CancellationToken.None);
        }

    }
}
