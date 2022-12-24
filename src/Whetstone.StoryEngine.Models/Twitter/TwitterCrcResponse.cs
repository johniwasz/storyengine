using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Models.Twitter
{
    public class TwitterCrcResponse
    {

        [JsonProperty(PropertyName = "response_token")]
        public string ResponseToken { get; set; }
    }
}
