using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Story.Ssml
{

    [JsonObject]
    [MessagePackObject]
    public class SwitchConditionFragment : SpeechFragment
    {

        public SwitchConditionFragment()
        {

            this.FragmentType = SpeechFragmentType.SwitchFragment;
        }


        public override SpeechFragmentType FragmentType
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the condition value
        /// </summary>
        [DataMember]
        [YamlMember]
        [JsonProperty(PropertyName = "condition", NullValueHandling = NullValueHandling.Ignore)]
        [Key(1)]
        public string Conditions { get; set; }


        [DataMember]
        [YamlMember]
        [JsonProperty(PropertyName = "isCaseSensitive", NullValueHandling = NullValueHandling.Ignore)]
        [Key(2)]
        public bool? IsCaseSensitive { get; set; }


        [DataMember]
        [YamlMember]
        [JsonProperty(PropertyName = "switchConditions", NullValueHandling = NullValueHandling.Ignore)]
        [Key(3)]
        public SwitchConditonBlock SwitchConditions { get; set; }

    }

    [JsonObject]
    [MessagePackObject]
    public class SwitchConditonBlock
    {
        [DataMember]
        [YamlMember(Alias = "switchConditions")]
        [JsonProperty(PropertyName = "switchConditions", NullValueHandling = NullValueHandling.Ignore)]
        [Key(0)]
        public List<SwitchCondition> SwitchConditions { get; set; }


        [DataMember]
        [YamlMember(Alias = "default")]
        [JsonProperty(PropertyName = "default", NullValueHandling = NullValueHandling.Ignore)]
        [Key(1)]
        public List<SpeechFragment> DefaultFragments { get; set; }

    }


    [JsonObject]
    [MessagePackObject]
    public class SwitchCondition
    {


        [DataMember]
        [YamlMember(Alias = "value")]
        [JsonProperty(PropertyName = "value", NullValueHandling = NullValueHandling.Ignore)]
        [Key(0)]
        public string ConditionValue { get; set; }

        [DataMember]
        [YamlMember(Alias = "default")]
        [JsonProperty(PropertyName = "default", NullValueHandling = NullValueHandling.Ignore)]
        [Key(1)]
        public List<SpeechFragment> ConditionFragments { get; set; }

    }
}
