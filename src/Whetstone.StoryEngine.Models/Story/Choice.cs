using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using YamlDotNet.Serialization;
using MessagePack;
using Whetstone.StoryEngine.Models.Serialization;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Conditions;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Actions;

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
