using MessagePack;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Story.Ssml
{
    [JsonObject]
    [DataContract]
    [MessagePackObject]
    public class SpeechBreakFragment : SpeechFragment
    {

        public SpeechBreakFragment()
        {

            FragmentType = SpeechFragmentType.Break;
        }

        [Key(1)]
        [JsonProperty("duration", Order = 1)]
        [DataMember]
        public int Duration { get; set; }

        [YamlIgnore]
        [JsonProperty("fragmentType", Order = 0)]
        [DataMember]
        public override SpeechFragmentType FragmentType { get; set; }
    }
}
