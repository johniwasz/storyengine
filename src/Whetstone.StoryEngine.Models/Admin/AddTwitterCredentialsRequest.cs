using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Models.Admin
{
    public class AddTwitterCredentialsRequest
    {

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "consumerKey")]
        public string ConsumerKey { get; set; }

        [JsonProperty(PropertyName = "consumerSecret")]
        public string ComsumerSecret { get; set; }

        [JsonProperty(PropertyName = "accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "accessTokenSecret")]
        public string AccessTokenSecret { get; set; }

        [JsonProperty(PropertyName = "bearerToken")]
        public string BearerToken { get; set; }
    }
}
