using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Story
{
    [DataContract]
    public class PhoneMessage
    {

        /// <summary>
        /// This is the phone message to deliver.  Strings embedded between dollar sign characters
        /// are assumed to be macros that will be replaced by a preprocessor
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "message")]
        [YamlMember(Alias = "message", Order = 0)]
        public string Message { get; set; }

        /// <summary>
        /// This is a list of tags to help a preprocessor process the message
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "tags")]
        [YamlMember(Alias = "tags", Order = 1)]
        public Dictionary<string, string> Tags { get; set; }


        [DataMember]
        [YamlMember(Alias = "conditions", Order = 2)]
        [NotMapped]
        [JsonProperty(PropertyName = "conditions", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> ConditionNames { get; set; }



    }
}
