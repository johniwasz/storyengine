using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Google.Management.Models
{

  
    public class DialogFlowIntent
    {
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Id { get; set; }


        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("auto", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Auto { get; set; }

        [JsonProperty("contexts", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<object> Contexts { get; set; }

        [JsonProperty("responses", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<IntentResponse> Responses { get; set; }

        [JsonProperty("priority", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Priority { get; set; }

        [JsonProperty("webhookUsed", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool WebhookUsed { get; set; }

        [JsonProperty("webhookForSlotFilling", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool WebhookForSlotFilling { get; set; }

        [JsonProperty("fallbackIntent", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool FallbackIntent { get; set; }

        [JsonProperty("events", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<NamedEvent> Events { get; set; }

        [JsonProperty("conditionalResponses", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<object> ConditionalResponses { get; set; }

        [JsonProperty("condition", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Condition { get; set; }

        [JsonProperty("conditionalFollowupEvents", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<object> ConditionalFollowupEvents { get; set; }
    }
}
