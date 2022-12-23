using System;
using System.IdentityModel.Tokens.Jwt;

namespace Whetstone.StoryEngine.SocketApi.Repository
{
    public interface IWebAuthorizer
    {
        /// <summary>
        /// Takes in a string authtoken, validates it and if successful, returns a standard userId
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        string GetValidUserIdFromAuthToken(string authToken);

        /// <summary>
        /// Takes in a string authToken, and if valid, returns a JWT.
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        JwtSecurityToken GetValidJwtFromAuthToken(string authToken);
    }
}
