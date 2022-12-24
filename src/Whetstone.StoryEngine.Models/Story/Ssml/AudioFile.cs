using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Story.Ssml
{
    [JsonObject]
    [MessagePackObject]
    public class AudioFile : SpeechFragment
    {

        public AudioFile()
        {

            this.FragmentType = SpeechFragmentType.AudioFile;
        }


        public AudioFile(string fileName)
        {
            this.FileName = fileName;
            this.FragmentType = SpeechFragmentType.AudioFile;
        }

        [JsonProperty("fileName", NullValueHandling = NullValueHandling.Ignore)]
        [YamlMember(Order = 0)]
        [Key(1)]
        //public Uri AudioUrl
        public string FileName
        {
            get; set;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("fragmentType")]
        [Key(2)]
        [YamlIgnore]
        public sealed override SpeechFragmentType FragmentType { get; set; }


        /// <summary>
        /// This appears inside the desc tag of an audio tag. This is supported by GoogleHome and ignored by other clients.
        /// </summary>
        /// <value>
        /// The description SSML.
        /// </value>
        [JsonProperty("descriptionSsml")]
        [Key(3)]
        public SsmlSpeechFragment DescriptionSsml { get; set; }



    }
}
