using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Whetstone.StoryEngine.Security.Amazon
{
    public static class CognitoSigningKey
    {


        public static SymmetricSecurityKey ComputeKey(string userPoolClientSecret)
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(userPoolClientSecret));
        }
    }
}
