using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.SocketApi.Repository
{
    public interface ISocketMessageSender
    {
        Task SendMessageToClient(IAuthenticatedSocket authSocket, string message, params KeyValuePair<string, object>[] parameters);
    }
}
