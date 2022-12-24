using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Google.Management.Models
{
    public class AffectedContext
    {
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("parameters", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public object Parameters { get; set; }

        [JsonProperty("lifespan", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Lifespan { get; set; }
    }
}
