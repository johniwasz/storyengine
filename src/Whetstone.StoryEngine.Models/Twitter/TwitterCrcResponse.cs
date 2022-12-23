using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Twitter
{
    public class TwitterCrcResponse
    {

        [JsonProperty(PropertyName = "response_token")]
        public string ResponseToken { get; set; }
    }
}
