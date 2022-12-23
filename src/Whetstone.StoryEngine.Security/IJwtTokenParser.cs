using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Security
{
    public interface IJwtTokenParser
    {
        JwtSecurityToken ParseAuthToken(string token, bool validate = false);
    }
}
