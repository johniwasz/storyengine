using Amazon.DynamoDBv2;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.SocketApi.Repository;
using Whetstone.StoryEngine.SocketApi.Repository.Amazon;

namespace Whetstone.StoryEngine.CoreApi.WebSockets
{
    //
    // We inherit off of the DynamoDB Socket Manager in order to allow ourselves to exercise that code
    // while we're running local. We still need to keep sockets locally because we do need to be able to manage
    // actual Websocket objects that won't be available in cloud implementations
    //
    public class SocketConnectionManager : DynamoDBSocketManager
    {
        public SocketConnectionManager(ILogger<DynamoDBSocketManager> logger, IAmazonDynamoDB ddbClient, IOptions<SocketConfig> socketConfig)
            : base(logger, ddbClient, socketConfig)
        {
            // Our sockets are local so set a flag so they don't clash with cloud sockets
            _isLocal = true;
        }

        private ConcurrentDictionary<string, IAuthenticatedSocket> _sockets = new ConcurrentDictionary<string, IAuthenticatedSocket>();

        public override async Task<IAuthenticatedSocket> GetSocketByIdAsync(string userId, string id)
        {
            await base.GetSocketByIdAsync(userId, id);
            return _sockets[id];
        }

        public override async Task<ICollection<IAuthenticatedSocket>> GetSocketsForUserIdAsync(string userId)
        {
            await base.GetSocketsForUserIdAsync(userId);

            List<IAuthenticatedSocket> sockets = new List<IAuthenticatedSocket>();

            foreach (IAuthenticatedSocket socket in _sockets.Values)
            {
                if (socket.UserId == userId)
                {
                    sockets.Add(socket);
                }
            }

            return sockets;

        }

        public override async Task AddSocketAsync(IAuthenticatedSocket socket)
        {
            await base.AddSocketAsync(socket);
            _sockets.TryAdd(socket.Id, socket);
        }

        public override async Task RemoveSocketAsync(string userId, string id)
        {
            await base.RemoveSocketAsync(userId, id);

            IAuthenticatedSocket socket;
            _sockets.TryRemove(id, out socket);
        }

        public override async Task<ICollection<IAuthenticatedSocket>> GetSocketsForClientIdAsync(string userId, string clientId)
        {
            await base.GetSocketsForUserIdAsync(userId);

            List<IAuthenticatedSocket> sockets = new List<IAuthenticatedSocket>();

            foreach (IAuthenticatedSocket socket in _sockets.Values)
            {
                if (socket.UserId == userId && socket.ClientId == clientId)
                {
                    sockets.Add(socket);
                }
            }

            return sockets;
        }


    }
}
