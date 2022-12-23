using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Google.Management.Models
{
    public class IntentMessage
    {
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Type { get; set; }

        [JsonProperty("lang", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Lang { get; set; }

        [JsonProperty("condition", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Condition { get; set; }

        [JsonProperty("speech", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<object> Speech { get; set; }
    }
}
