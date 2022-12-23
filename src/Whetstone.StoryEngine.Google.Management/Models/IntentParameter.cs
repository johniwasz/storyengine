using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Google.Management.Models
{
    public class IntentParameter
    {

        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("required", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Required { get; set; }

        [JsonProperty("dataType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string DataType { get; set; }


        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Name { get; set; }


        [JsonProperty("value", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Value { get; set; }

        [JsonProperty("promptMessages", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<object> PromptMessages { get; set; }

        [JsonProperty("noMatchPromptMessages", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<object> NoMatchPromptMessages { get; set; }

        [JsonProperty("noInputPromptMessages", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<object> NoInputPromptMessages { get; set; }

        [JsonProperty("outputDialogContexts", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public List<object> OutputDialogContexts { get; set; }

        [JsonProperty("isList", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool IsList { get; set; }
    }
}
