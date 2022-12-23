using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Whetstone.StoryEngine.Models.Admin;

namespace Whetstone.StoryEngine.Security
{
    public class UserConfirmationPendingException : AdminException
    {

        public UserConfirmationPendingException(string publicMessage, string message, Exception innerEx) : base(publicMessage,
            message, innerEx)
        {
        }

        public UserConfirmationPendingException(string publicMessage, string message) : base(publicMessage, message)
        {
        }


        public override LogLevel LogLevel { get; set; } = LogLevel.Warning;


        public override string Title => "User Not Confirmed";

        public override string ErrorCode => SecurityErrorCodes.USER_NOT_CONFIRMED;

        public override int StatusCode => 401;
    }
}