using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Twitter
{
    public class TwitterCredentials
    {

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
