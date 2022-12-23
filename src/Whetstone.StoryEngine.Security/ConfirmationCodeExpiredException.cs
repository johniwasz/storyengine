using System;
using System.Collections.Generic;
using System.Text;
using Whetstone.StoryEngine.Models.Admin;

namespace Whetstone.StoryEngine.Security
{
    public class ConfirmationCodeExpiredException : AdminException
    {

        public ConfirmationCodeExpiredException(string publicMessage, string message, Exception innerEx) : base(publicMessage, message, innerEx)
        {
        }

        public ConfirmationCodeExpiredException(string publicMessage, string message) : base(publicMessage, message)
        {
        }

        public override string Title => "Confirmation Code Expired";

        public override string ErrorCode => SecurityErrorCodes.USER_CONFIRMATION_CODE_EXPIRED;

        public override int StatusCode => 401;
    }
}
