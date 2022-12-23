using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.SocketApi.Repository
{
    public interface ISocketConnectionManager
    {
        bool IsLocal { get; }

        Task<IAuthenticatedSocket> GetSocketByIdAsync(string userId, string id);

        Task<ICollection<IAuthenticatedSocket>> GetSocketsForUserIdAsync(string userId);

        Task UpdateSocketTTL(string userId, string id);

        Task AddSocketAsync(IAuthenticatedSocket socket);

        Task RemoveSocketAsync(string userId, string id);

        Task<ICollection<IAuthenticatedSocket>> GetSocketsForClientIdAsync(string userId, string clientId);
    }
}
