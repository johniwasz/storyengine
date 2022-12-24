using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Security
{
    public class SignOutRequest
    {
        [JsonProperty(PropertyName = "authToken")]
        public string AuthToken { get; set; }
    }
}
