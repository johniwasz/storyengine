using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Models.Messaging
{

    /// <summary>
    /// Represents the deserialized values retrieved from the secret store
    /// </summary>
    public class TwilioCredentials
    {

        [JsonProperty(PropertyName = "accountSid")]
        public string AccountSid { get; set; }

        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; }

    }

}
