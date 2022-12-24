using System;
using Whetstone.StoryEngine.Models.Admin;

namespace Whetstone.StoryEngine.Security
{

    public class UserNotAuthenticatedException : AdminException
    {

        public UserNotAuthenticatedException(string publicMessage, string message, Exception innerEx) : base(publicMessage, message, innerEx)
        {
        }

        public UserNotAuthenticatedException(string publicMessage, string message) : base(publicMessage, message)
        {
        }


        public override string Title => "User Not Authenticated";

        public override string ErrorCode => SecurityErrorCodes.USER_NOT_AUTHENTICATED;


        public override int StatusCode => 401;
    }
}
