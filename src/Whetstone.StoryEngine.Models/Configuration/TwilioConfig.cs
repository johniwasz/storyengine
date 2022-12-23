using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Configuration
{

    /// <summary>
    /// This is used by the ISmsMessageSender Twilio implementation to pass along the callback url for the status update.
    /// </summary>
    public class TwilioConfig
    {

        public TwilioConfig()
        {

        }


        public TwilioConfig(string statusCallbackUrl)
        {
            StatusCallbackUrl = statusCallbackUrl;

        }

        /// <summary>
        /// The callback url Twilio uses to submit a status update of a requested message.
        /// </summary>
        /// <remarks>This is not required. If it is null or empty, then no status callback url will be set.</remarks>
        [YamlMember(Alias = "statusCallbackUrl")]
        public string StatusCallbackUrl { get; set; }


        [YamlMember(Alias = "liveCredentials")]
        public string LiveCredentials { get; set; }

        [YamlMember(Alias = "testCredentials")]
        public string TestCredentials { get; set; }

    }
}
