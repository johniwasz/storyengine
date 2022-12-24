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
    public class PlainTextSpeechFragment : SpeechFragment
    {

        private string _speechText;


        public PlainTextSpeechFragment()
        {

            this.FragmentType = SpeechFragmentType.PlainText;
        }

        public PlainTextSpeechFragment(string speechText)
        {
            _speechText = speechText;
            this.FragmentType = SpeechFragmentType.PlainText;
        }

        [DataMember]
        [JsonProperty("text", Order = 1, NullValueHandling = NullValueHandling.Ignore)]
        [Key(1)]
        [YamlMember()]
        public string Text { get { return _speechText; } set { _speechText = value; } }


        [DataMember]
        [JsonProperty("voice", Order = 2, NullValueHandling = NullValueHandling.Ignore)]
        [Key(2)]
        [YamlMember]
        public string Voice { get; set; }

        [DataMember]
        [JsonProperty("fragmentType", Order = 0)]
        [JsonConverter(typeof(StringEnumConverter))]
        [Key(3)]
        [YamlIgnore]
        public sealed override SpeechFragmentType FragmentType { get; set; }


        [DataMember]
        [JsonProperty("voiceFileId", Order = 4, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        [Key(4)]
        [YamlMember]
        public Guid VoiceFileId { get; set; }

    }
}
