using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Google.Management.Models
{
    public class NamedEvent
    {
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Name { get; set; }
    }
}
