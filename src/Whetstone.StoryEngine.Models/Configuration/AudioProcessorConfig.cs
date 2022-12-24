using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Configuration
{
    public class AudioProcessorConfig
    {
        [JsonProperty(PropertyName = "tempFolder")]
        [YamlMember(Alias = "tempFolder")]
        public string TempFolder { get; set; }

        [JsonProperty(PropertyName = "ffMpegPath")]
        [YamlMember(Alias = "ffMpegPath")]
        public string FFMpegPath { get; set; }
    }
}
