using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Admin
{
    public class BadFormatException : AdminException
    {

        public BadFormatException(string publicMessage, string message, Exception innerEx) : base(publicMessage, message, innerEx)
        {
        }

        public BadFormatException(string publicMessage, string message) : base(publicMessage, message)
        {
        }


        public override string Title => "Bad Request";


        public override string ErrorCode => AdminErrorCodes.INVALID_REQUEST;

        public override int StatusCode => 400;
    }
}
