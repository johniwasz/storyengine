using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Google.Management.Models
{



    public class IntentTrainingPhrase
    {
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("data", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<TrainingPhrasePart> Data { get; set; }

        [JsonProperty("isTemplate", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool IsTemplate { get; set; }

        [JsonProperty("count", DefaultValueHandling = DefaultValueHandling.Include)]
        public int Count { get; set; }

        [JsonProperty("updated", DefaultValueHandling = DefaultValueHandling.Include)]
        public int Updated { get; set; }
    }
}
