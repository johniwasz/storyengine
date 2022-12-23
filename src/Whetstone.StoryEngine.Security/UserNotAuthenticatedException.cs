using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Serialization;

namespace Whetstone.StoryEngine.Security
{
   
    public class UserNotAuthenticatedException: AdminException
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
