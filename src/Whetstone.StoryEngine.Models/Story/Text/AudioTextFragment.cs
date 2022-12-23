using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Whetstone.StoryEngine.Models.Story.Ssml;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Story.Text
{

    [DataContract]
    public class AudioTextFragment : TextFragmentBase
    {
        public AudioTextFragment()
        {
            this.FragmentType = TextFragmentType.Audio;

        }

        public AudioTextFragment(string audioFile)
        {
            FileName = audioFile;
            this.FragmentType = TextFragmentType.Simple;
        }

        [DataMember]
        [MessagePack.Key(1)]
        [YamlMember(Order = 1)]
        [JsonProperty(PropertyName = "fileName", NullValueHandling = NullValueHandling.Ignore)]
        public string FileName { get; set; }

        [NotMapped]
        [JsonProperty("fragmentType")]
        [MessagePack.Key(2)]
        [YamlIgnore]
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        public sealed override TextFragmentType FragmentType { get; set; }

        public override SpeechFragment ToSpeechFragment()
        {

            AudioFile audioSpeechFragment = new AudioFile();
            audioSpeechFragment.FileName = FileName;
            return audioSpeechFragment;
        }
    }
}
