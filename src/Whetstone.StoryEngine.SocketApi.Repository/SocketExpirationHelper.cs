using System;

namespace Whetstone.StoryEngine.SocketApi.Repository
{
    public static class SocketExpirationHelper
    {
        public static readonly int SOCKET_DEFAULT_TTL = 10;

        public static long GetNewSocketTTL()
        {
            return DateTimeOffset.UtcNow.AddMinutes(SOCKET_DEFAULT_TTL).ToUnixTimeSeconds();
        }

        public static long GetSocketExpirationNow()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

    }
}
