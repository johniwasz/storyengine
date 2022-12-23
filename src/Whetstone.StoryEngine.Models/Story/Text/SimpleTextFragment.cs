using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;
using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Whetstone.StoryEngine.Models.Story.Ssml;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Story.Text
{
    [JsonObject]
    [DataContract]
    public class SimpleTextFragment : TextFragmentBase
    {
        public SimpleTextFragment()
        {
            this.FragmentType = TextFragmentType.Simple;

        }

        public SimpleTextFragment(string text)
        {
            Text = text;
            this.FragmentType = TextFragmentType.Simple;
        }

        [DataMember]
        [MessagePack.Key(1)]
        [YamlMember(Order = 1)]
        [JsonProperty(PropertyName ="text", NullValueHandling =  NullValueHandling.Ignore)]
        public string Text { get; set; }

        [NotMapped]
        [JsonProperty("fragmentType")]
        [MessagePack.Key(2)]
        [YamlIgnore]
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        public sealed override TextFragmentType FragmentType { get; set; }

        public override SpeechFragment ToSpeechFragment()
        {

            PlainTextSpeechFragment plainTextSpeechFragment = new PlainTextSpeechFragment
            {
                Text = this.Text
            };
            return plainTextSpeechFragment;
        }
    }
}
