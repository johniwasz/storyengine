using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Google.Management.Models
{
    public class DialogFlowPackage
    {
        [JsonProperty("version", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Version { get; set; }
    }
}