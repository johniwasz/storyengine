using System;
using Whetstone.StoryEngine.Models.Admin;

namespace Whetstone.StoryEngine.Security
{
    public class UnexpectedSecurityException : AdminException
    {

        public UnexpectedSecurityException(string message, Exception innerEx) : base("Unexpected security error", message, innerEx)
        {
        }

        public UnexpectedSecurityException(string publicMessage, string message, Exception innerEx) : base(publicMessage, message, innerEx)
        {
        }

        public UnexpectedSecurityException(string publicMessage, string message) : base(publicMessage, message)
        {
        }

        public UnexpectedSecurityException(string message) : base("Unexpected security error", message)
        {
        }

        public override string Title => "Unexpected Security Exception";

        public override string ErrorCode => SecurityErrorCodes.UNEXPECTED_ERROR;

        public override int StatusCode => 401;
    }
}
