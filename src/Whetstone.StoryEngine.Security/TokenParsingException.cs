using System;
using System.Collections.Generic;
using System.Text;
using Whetstone.StoryEngine.Models.Admin;

namespace Whetstone.StoryEngine.Security
{
    class TokenParsingException : AdminException
    {

        public TokenParsingException(string publicMessage, string message, Exception innerEx) : base(publicMessage, message, innerEx)
        {
        }

        public TokenParsingException(string publicMessage, string message) : base(publicMessage, message)
        {
        }


        public override string Title => "Token Could Not Be Parsed";


        public override string ErrorCode => SecurityErrorCodes.TOKEN_PARSING_FAILED;

        public override int StatusCode => 401;
    }
}
