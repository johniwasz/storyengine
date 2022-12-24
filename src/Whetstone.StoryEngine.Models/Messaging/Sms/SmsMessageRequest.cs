using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Messaging.Sms
{
    public enum SmsSenderType
    {
        [EnumMember(Value = "unassigned")]
        Unassigned = 0,
        [EnumMember(Value = "pinpoint")]
        Pinpoint = 1,
        [EnumMember(Value = "sns")]
        Sns = 2,
        [EnumMember(Value = "twilio")]
        Twilio = 3
    }


    /// <summary>
    /// Sent to the handler which dispatches the SMS text message.
    /// </summary>
    /// <remarks>
    /// This is not saved to the database.
    /// </remarks>
    public class SmsMessageRequest
    {


        /// <summary>
        /// Formatted source phone number.
        /// </summary>
        public string SourceNumber { get; set; }


        /// <summary>
        /// Formatted destination phone number.
        /// </summary>
        public string DestinationNumber { get; set; }

        /// <summary>
        /// Contents of the SMS message to send.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Sender-specific tag mappings.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; }

    }
}
