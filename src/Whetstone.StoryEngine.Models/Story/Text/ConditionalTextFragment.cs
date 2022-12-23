using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Whetstone.StoryEngine.Models.Story.Ssml;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Story.Text
{

    [DataContract]
    public class ConditionalTextFragment : TextFragmentBase
    {

        public ConditionalTextFragment()
        {
            this.FragmentType = TextFragmentType.Conditional;
        }

        /// <summary>
        /// All conditions must be true.
        /// </summary>
        [YamlMember]
        [DataMember]
        [Key(1)]
        [JsonProperty(PropertyName = "conditions", NullValueHandling =  NullValueHandling.Ignore)]
        public List<string> Conditions { get; set; }


        /// <summary>
        /// If all conditions are true, then return these speech fragments.
        /// </summary>
        [YamlMember]
        [DataMember]
        [Key(2)]
        [JsonProperty(PropertyName = "trueResultFragments", NullValueHandling = NullValueHandling.Ignore)]
        public List<TextFragmentBase> TrueResultFragments { get; set; }


        /// <summary>
        /// If any one of the conditions is false, then use these speech fragments. 
        /// </summary>
        [YamlMember]
        [DataMember]
        [Key(3)]
        [JsonProperty(PropertyName = "falseResultFragments", NullValueHandling = NullValueHandling.Ignore)]
        public List<TextFragmentBase> FalseResultFragments { get; set; }

        [YamlIgnore]
        [Key(4)]
        [DataMember]
        [NotMapped]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("fragmentType")]
        public sealed override TextFragmentType FragmentType { get; set; }

        public override SpeechFragment ToSpeechFragment()
        {


            ConditionalFragment speechFrag = new ConditionalFragment
            {
                Conditions = new List<string>()
            };

            if (Conditions!=null)
                speechFrag.Conditions.AddRange(Conditions);

            if (TrueResultFragments != null)
            {
                speechFrag.TrueResultFragments = new List<SpeechFragment>();

                foreach (TextFragmentBase textBase in TrueResultFragments)
                {

                    speechFrag.TrueResultFragments.Add(textBase.ToSpeechFragment());

                }
            }


            if (FalseResultFragments != null)
            {
                speechFrag.FalseResultFragments = new List<SpeechFragment>();

                foreach (TextFragmentBase textBase in FalseResultFragments)
                {

                    speechFrag.FalseResultFragments.Add(textBase.ToSpeechFragment());

                }
            }

            return speechFrag;

        }
    }
}
