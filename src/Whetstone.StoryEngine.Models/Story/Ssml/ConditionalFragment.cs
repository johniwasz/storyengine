using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Story.Ssml
{
    [JsonObject]
    [DataContract]
    public class ConditionalFragment : SpeechFragment
    {

        public ConditionalFragment()
        {

            this.FragmentType = SpeechFragmentType.ConditionalFragment;
        }

        /// <summary>
        /// All conditions must be true.
        /// </summary>
        [DataMember]
        [YamlMember]
        [JsonProperty(PropertyName = "conditions", NullValueHandling = NullValueHandling.Ignore)]
        [Key(1)]
        public List<string> Conditions { get; set; }


        /// <summary>
        /// If all conditions are true, then return these speech fragments.
        /// </summary>
        [YamlMember]
        [DataMember]
        [ForeignKey("TrueFragementResponseId")]
        [JsonProperty(PropertyName = "trueResultFragments", NullValueHandling = NullValueHandling.Ignore)]
        [Key(2)]
        public List<SpeechFragment> TrueResultFragments { get; set; }


        /// <summary>
        /// If any one of the conditions is false, then use these speech fragments. 
        /// </summary>
        [YamlMember]
        [DataMember]
        [ForeignKey("FalseFragementResponseId")]
        [JsonProperty(PropertyName = "falseResultFragments", NullValueHandling = NullValueHandling.Ignore)]
        [Key(3)]
        public List<SpeechFragment> FalseResultFragments { get; set; }

        [JsonProperty("fragmentType")]
        [JsonConverter(typeof(StringEnumConverter))]
        [YamlIgnore]
        [DataMember]
        [Key(4)]
        public override SpeechFragmentType FragmentType { get; set; }
    }
}
