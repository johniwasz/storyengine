using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Google.Management.Models
{

    public class IntentResponse
    {

        [JsonProperty("resetContexts", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool ResetContexts { get; set; }

        [JsonProperty("action", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Action { get; set; }

        [JsonProperty("affectedContexts", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<AffectedContext> AffectedContexts { get; set; }

        [JsonProperty("parameters", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<IntentParameter> Parameters { get; set; }

        [JsonProperty("messages", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<IntentMessage> Messages { get; set; }

        [JsonProperty("defaultResponsePlatforms", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IntentResponsePlatforms DefaultResponsePlatforms { get; set; }

        [JsonProperty("speech", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<object> Speech { get; set; }
    }
}
