using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using Whetstone.StoryEngine.Models.Story.Ssml;

namespace Whetstone.StoryEngine.Models.Story
{


    public class ClientSpeechFragments
    {

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("speechClient", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public Client? SpeechClient { get; set; }


        [JsonProperty("speechFragments", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public List<SpeechFragment> SpeechFragments { get; set; }


    }

}
