using Newtonsoft.Json;


namespace Whetstone.StoryEngine.Google.Management.Models
{
    public class DialogFlowEntity
    {
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("isOverridable", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool IsOverridable { get; set; }

        [JsonProperty("isEnum", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool IsEnum { get; set; }

        [JsonProperty("isRegexp", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool IsRegExp { get; set; }

        [JsonProperty("automatedExpansion", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool AutomatedExpansion { get; set; }

        [JsonProperty("allowFuzzyExtraction", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool AllowFuzzyExtraction { get; set; }

    }
}
