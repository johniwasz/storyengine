using System.IdentityModel.Tokens.Jwt;

namespace Whetstone.StoryEngine.Security
{
    public interface IJwtTokenParser
    {
        JwtSecurityToken ParseAuthToken(string token, bool validate = false);
    }
}
