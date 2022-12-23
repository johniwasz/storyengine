using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.IdentityModel.Tokens;

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
