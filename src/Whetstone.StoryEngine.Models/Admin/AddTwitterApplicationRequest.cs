using Newtonsoft.Json;
using System;

namespace Whetstone.StoryEngine.Models.Admin
{
    public class AddTwitterApplicationRequest
    {


        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "credentials")]
        public AddTwitterApplicationCredentialsRequest Credentials { get; set; }

        [JsonProperty(PropertyName = "titleVersionId")]
        public Guid? TitleVersionId { get; set; }


        [JsonProperty(PropertyName = "isEnabled")]
        public bool IsEnabled { get; set; }
    }


    public class AddTwitterApplicationCredentialsRequest
    {

        [JsonProperty(PropertyName = "consumerKey")]
        public string ConsumerKey { get; set; }

        [JsonProperty(PropertyName = "consumerSecret")]
        public string ConsumerSecret { get; set; }

        [JsonProperty(PropertyName = "accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "accessTokenSecret")]
        public string AccessTokenSecret { get; set; }

        [JsonProperty(PropertyName = "bearerToken")]
        public string BearerToken { get; set; }

    }
}
