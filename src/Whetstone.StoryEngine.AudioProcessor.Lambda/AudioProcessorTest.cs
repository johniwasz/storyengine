using Newtonsoft.Json;
using System;

namespace Whetstone.StoryEngine.AudioProcessor.Lambda
{
    public class AudioProcessorTest
    {
        [JsonProperty(PropertyName = "fileName")]
        public string FileName { get; set; }

        [JsonProperty(PropertyName = "filePath")]
        public string FilePath { get; set; }

        [JsonProperty(PropertyName = "destFileName")]
        public string DestFileName { get; set; }

        [JsonProperty(PropertyName = "destFilePath")]
        public string DestFilePath { get; set; }
    }
}
