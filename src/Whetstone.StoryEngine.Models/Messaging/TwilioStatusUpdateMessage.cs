using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Messaging
{
    public class TwilioStatusUpdateMessage
    {
        /// <summary>
        /// Attribute name applied to the SQS message to indicate the original host that Twilio used to post the status update callback.
        /// </summary>
        public readonly static string ORIGINAL_HOST_ATTRIBUTE = "OriginalHost";


        /// <summary>
        /// Attribute name applied to the SQS message to indicate the original scheme that Twilio used to post the status update callback.
        /// </summary>
        /// <remarks>
        /// This should be "https"
        /// </remarks>
        public readonly static string ORIGINAL_SCHEME_ATTRIBUTE = "OriginalScheme";


        /// <summary>
        /// Attribute name applied to the SQS message to indicate the HTTP path that Twilio used to post the status update callback.
        /// </summary>
        public readonly static string PATH_ATTRIBUTE = "Path";

        /// <summary>
        /// Attribute name applied to the SQS message with the validation token used to authenticate that the message originated from Twilio.
        /// </summary>
        public readonly static string VALIDATION_TOKEN_ATTRIBUTE = "ValidationToken";


        /// <summary>
        /// Validation token provided by Twilio status update.
        /// </summary>
        public string ValidationToken { get; set; }

        /// <summary>
        /// Post Url supplied by Twilio status check response
        /// </summary>
        public Uri OriginalUrl { get; set; }

        /// <summary>
        /// Values from the body of the Twilio callback message
        /// </summary>
        public Dictionary<string, string> MessageBody { get; set; }

        /// <summary>
        /// This is the unique queue message id. This is used to ensure that duplicate message updates are not added to the database.
        /// </summary>
        /// <remarks>This is logged to the sms_message_log table to ensure that the message is not entered into the logs more than once.</remarks>
        public string QueueMessageId { get; set; }
    }
}
