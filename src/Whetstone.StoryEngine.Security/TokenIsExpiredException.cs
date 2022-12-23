using System;
using System.Collections.Generic;
using System.Text;
using Whetstone.StoryEngine.Models.Admin;

namespace Whetstone.StoryEngine.Security
{
    public class TokenIsExpiredException : AdminException
    {

        public TokenIsExpiredException(string publicMessage, string message, Exception innerEx) : base(publicMessage, message, innerEx)
        {
        }

        public TokenIsExpiredException(string publicMessage, string message) : base(publicMessage, message)
        {
        }


        public override string Title => "Token Expired";


        public override string ErrorCode => SecurityErrorCodes.TOKEN_EXPIRED;

        public override int StatusCode => 401;
    }
}
