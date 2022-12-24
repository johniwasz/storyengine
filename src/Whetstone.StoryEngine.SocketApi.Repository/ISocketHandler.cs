using System.Collections.Generic;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.SocketApi.Repository
{
    public interface ISocketHandler
    {
        ISocketResponse ValidateSocketAuthToken(string authToken, out string userId);
        Task<ISocketResponse> OnConnect(IAuthenticatedSocket authSock);
        Task<ISocketResponse> OnDisconnect(string userId, string connectionId, params KeyValuePair<string, object>[] parameters);
        Task<ISocketResponse> OnReceiveMessage(string userId, string connectionId, string clientMessage, params KeyValuePair<string, object>[] parameters);
    }
}
