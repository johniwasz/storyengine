using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Story.Ssml
{
    [JsonObject]
    [DataContract]
    public class SpeechBreakFragment : SpeechFragment
    {

        public SpeechBreakFragment()
        {

            FragmentType = SpeechFragmentType.Break;
        }

        [JsonProperty("duration", Order =1)]
        [DataMember]
        public int Duration { get; set; }

        [YamlIgnore]
        [JsonProperty("fragmentType", Order =0)]
        [DataMember]
        public override SpeechFragmentType FragmentType { get; set; }
    }
}
