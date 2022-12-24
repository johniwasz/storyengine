using Newtonsoft.Json;
using System.Collections.Generic;

namespace Whetstone.StoryEngine.Google.Management.Models
{
    public class DialogFlowEntityValues
    {
        [JsonProperty("value", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Value { get; set; }

        [JsonProperty("synonyms", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<string> Synonyms { get; set; }

    }
}
