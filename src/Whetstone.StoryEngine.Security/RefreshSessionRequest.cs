using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Security
{
    public class RefreshSessionRequest
    {

        [JsonProperty(PropertyName = "refreshToken")]
        public string RefreshToken { get; set; }

        [JsonProperty(PropertyName = "authToken")]
        public string AuthToken { get; set; }
    }
}
