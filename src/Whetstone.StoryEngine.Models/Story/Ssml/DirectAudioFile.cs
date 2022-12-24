using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Story.Ssml
{

    [DataContract]
    [JsonObject]
    [MessagePackObject]
    public class DirectAudioFile : SpeechFragment
    {


        public DirectAudioFile()
        {

            this.FragmentType = SpeechFragmentType.DirectAudioFile;
        }


        public DirectAudioFile(string audioUri)
        {
            this.AudioUrl = audioUri;
            this.FragmentType = SpeechFragmentType.DirectAudioFile;
        }

        [Key(1)]
        [DataMember]
        [JsonProperty("audioUrl", NullValueHandling = NullValueHandling.Ignore)]
        [YamlMember]
        //public Uri AudioUrl
        public string AudioUrl
        {
            get; set;
        }


        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember]
        [JsonProperty("fragmentType")]
        [YamlIgnore]
        [Key(2)]
        public sealed override SpeechFragmentType FragmentType { get; set; }
    }
}
