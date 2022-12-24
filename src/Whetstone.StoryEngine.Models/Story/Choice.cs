using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Whetstone.StoryEngine.Models.Actions;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Story
{

    [Table("Choices")]
    [DataContract]
    [JsonObject]
    public class Choice
    {


        public Choice() : base()
        {


        }

        [JsonProperty(PropertyName = "intentName", NullValueHandling = NullValueHandling.Ignore)]
        [YamlMember(Alias = "intentName", Order = 0)]
        [DataMember]
        public string IntentName { get; set; }

        [DataMember]
        [YamlMember(Alias = "conditions", Order = 1)]
        [NotMapped]
        [JsonProperty(PropertyName = "conditions", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> ConditionNames { get; set; }

        /// <summary>
        /// This corresponds to the YAML representation of the node mappings.
        /// </summary>
        [DataMember]
        [YamlMember(Alias = "nodeMapping", Order = 3)]
        [Column("NodeMappingId")]
        [JsonProperty(PropertyName = "nodeMapping", NullValueHandling = NullValueHandling.Ignore)]
        [ForeignKey("Id")]
        public NodeMappingBase NodeMapping { get; set; }

        [JsonProperty(PropertyName = "actions", NullValueHandling = NullValueHandling.Ignore)]
        [YamlMember(Alias = "actions", Order = 2)]
        [DataMember]
        public List<NodeActionData> Actions { get; set; }

    }



}
