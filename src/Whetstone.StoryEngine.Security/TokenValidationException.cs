using System;
using Whetstone.StoryEngine.Models.Admin;

namespace Whetstone.StoryEngine.Security
{
    public class TokenValidationException : AdminException
    {

        public TokenValidationException(string publicMessage, string message, Exception innerEx) : base(publicMessage, message, innerEx)
        {
        }

        public TokenValidationException(string publicMessage, string message) : base(publicMessage, message)
        {
        }


        public override string Title => "Invalid Token";

        public override string ErrorCode => SecurityErrorCodes.TOKEN_VALIDATION_FAILED;

        public override int StatusCode => 401;
    }
}
