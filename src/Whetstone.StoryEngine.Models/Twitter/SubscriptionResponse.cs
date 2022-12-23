using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Twitter
{
    public class SubscriptionResponse
    {

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }
    }
}
