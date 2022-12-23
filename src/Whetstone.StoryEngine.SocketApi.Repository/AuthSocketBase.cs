using System;
using System.Net.WebSockets;

namespace Whetstone.StoryEngine.SocketApi.Repository
{
    public class AuthSocketBase : IAuthenticatedSocket
    {
        protected readonly string _Id;
        protected readonly WebSocket _Socket;

        protected readonly string _UserId;
        protected readonly string _ClientId;
        protected readonly string _AuthToken;


        public string Id
        {
            get
            {
                return _Id;
            }
        }

        public WebSocket Socket
        {
            get
            {
                return _Socket;
            }
        }

        public string UserId
        {
            get
            {
                return _UserId;
            }
        }


        public string AuthToken
        {
            get
            {
                return _AuthToken;
            }
        }

        public string ClientId
        {
            get
            {
                return _ClientId;
            }
        }

        public AuthSocketBase(string socketId, string userId, string authToken, string clientId) :
            this(socketId, null, userId, authToken, clientId)
        {
        }

        public AuthSocketBase(string socketId, WebSocket socket, string userId, string authToken, string clientId)
        {
            if (String.IsNullOrEmpty(userId))
                throw new InvalidOperationException("userId cannot be null!");

            if (String.IsNullOrEmpty(authToken))
                throw new InvalidOperationException("authToken cannot be null!");

            if (String.IsNullOrEmpty(clientId))
                throw new InvalidOperationException("clientId cannot be null!");

            _Id = socketId;
            _Socket = socket;
            _UserId = userId;
            _AuthToken = authToken;
            _ClientId = clientId;
        }

        public bool HasSocket()
        {
            return _Socket != null;
        }

        public bool IsAuthenticated()
        {
            return !String.IsNullOrEmpty(this.UserId) && !String.IsNullOrEmpty(this.AuthToken);
        }
    }
}
