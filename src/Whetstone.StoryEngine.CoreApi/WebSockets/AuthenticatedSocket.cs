using System;
using System.Net.WebSockets;

using Whetstone.StoryEngine.SocketApi.Repository;

namespace Whetstone.StoryEngine.CoreApi.WebSockets
{
    public class AuthenticatedSocket : AuthSocketBase
    {

        public AuthenticatedSocket( WebSocket socket, string userId, string authToken, string clientId )
            :base( Guid.NewGuid().ToString(), socket, userId, authToken, clientId )
        {
            if (socket == null)
                throw new InvalidOperationException("socket cannot be null!");
        }
    }
}
