using System.Net.WebSockets;

namespace Whetstone.StoryEngine.SocketApi.Repository
{
    public interface IAuthenticatedSocket
    {
        string Id { get; }
        WebSocket Socket { get; }

        string UserId { get; }
        string ClientId { get; }
        string AuthToken { get; }

        bool IsAuthenticated();
        bool HasSocket();
    }
}
