using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using YamlDotNet;
using Whetstone.StoryEngine.Models.Conditions;
using MessagePack;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Serialization;
using YamlDotNet.Serialization;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Whetstone.StoryEngine.Models.Story.Ssml
{
   
    [JsonConverter(typeof(SpeechFragmentConverter))]
    [XmlInclude(typeof(AudioFile))]
    [XmlInclude(typeof(DirectAudioFile))]
    [XmlInclude(typeof(PlainTextSpeechFragment))]
    [XmlInclude(typeof(ConditionalFragment))]
    [XmlInclude(typeof(SsmlSpeechFragment))]
    [XmlInclude(typeof(SpeechBreakFragment))]
    [XmlInclude(typeof(SwitchConditionFragment))]
    [MessagePack.Union(0, typeof(AudioFile))]
    [MessagePack.Union(1, typeof(DirectAudioFile))]
    [MessagePack.Union(2, typeof(PlainTextSpeechFragment))]
    [MessagePack.Union(3, typeof(ConditionalFragment))]
    [MessagePack.Union(4, typeof(SsmlSpeechFragment)) ]
    [MessagePack.Union(5, typeof(SpeechBreakFragment))]
    [MessagePack.Union(6, typeof(SwitchConditionFragment))]
    public abstract class SpeechFragment
    {
        [DataMember]
        [MessagePack.Key(0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [System.ComponentModel.DataAnnotations.Key()]
        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }

        [IgnoreDataMember]
        public abstract SpeechFragmentType FragmentType { get; set; }

        public SpeechFragment()
        {


        }


    }


    
}
