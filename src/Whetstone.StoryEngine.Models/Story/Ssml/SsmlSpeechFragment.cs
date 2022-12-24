using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Story.Ssml
{

    [JsonObject]
    [DataContract]
    [MessagePackObject]
    public class SsmlSpeechFragment : SpeechFragment
    {
        public SsmlSpeechFragment()
        {
            FragmentType = SpeechFragmentType.Ssml;

        }

        public SsmlSpeechFragment(string ssml)
        {
            this.Ssml = ssml;
            FragmentType = SpeechFragmentType.Ssml;

        }

        [Key(2)]
        [DataMember]
        [JsonProperty("ssml", Order = 1, NullValueHandling = NullValueHandling.Ignore)]
        public string Ssml { get; set; }


        [Key(1)]
        [DataMember]
        [YamlIgnore]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("fragmentType", Order = 0)]
        public override SpeechFragmentType FragmentType { get; set; }

        [DataMember]
        [JsonProperty("voice", Order = 3, NullValueHandling = NullValueHandling.Ignore)]
        [Key(3)]
        [YamlMember]
        public string Voice { get; set; }


        [DataMember]
        [JsonProperty("voiceFileId", Order = 4, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Key(4)]
        [YamlMember]
        public Guid? VoiceFileId { get; set; }
    }
}
