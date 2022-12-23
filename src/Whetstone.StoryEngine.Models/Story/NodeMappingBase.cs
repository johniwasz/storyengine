using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Whetstone.StoryEngine.Models.Serialization;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Story
{

    
    [Table("NodeMappings")]
    [XmlInclude(typeof(SingleNodeMapping))]
    [XmlInclude(typeof(MultiNodeMapping))]
    [XmlInclude(typeof(ConditionalNodeMapping))]
    [XmlInclude(typeof(SlotNodeMapping))]
    [XmlInclude(typeof(SlotMap))]
    [JsonConverter(typeof(NodeMapConverter))]
    [MessagePack.Union(0, typeof(SingleNodeMapping))]
    [MessagePack.Union(1, typeof(MultiNodeMapping))]
    [MessagePack.Union(2, typeof(ConditionalNodeMapping))]
    [MessagePack.Union(3, typeof(SlotNodeMapping))]
    [MessagePack.Union(4, typeof(SlotMap))]
    public abstract class NodeMappingBase
    {
        [Key(0)]
        [Column(Order = 1)]
        [YamlMember(Alias = "conditions", Order = 0)]
        [JsonProperty(PropertyName = "conditions", NullValueHandling = NullValueHandling.Ignore)]
        public virtual List<string> Conditions { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("nodeMappingType", Order = 0, NullValueHandling = NullValueHandling.Ignore)]
        public abstract NodeMappingType NodeMapping { get; set; }

    }

    [JsonObject]
    [DataContract]
    [MessagePackObject()]
    public class SlotMap : NodeMappingBase
    {

        public SlotMap()
        {
            NodeMapping = NodeMappingType.SlotMap;
        }

        [Key(1)]
        [DataMember]
        [ForeignKey(("Id"))]
        [JsonProperty("mappings", Order = 1, NullValueHandling = NullValueHandling.Ignore)]
        public List<SlotNodeMapping> Mappings { get; set; }

        [NotMapped]
        [YamlIgnore]
        [JsonProperty("nodeMappingType", Order =  0, NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember]
        [Key(2)]
        public sealed override NodeMappingType NodeMapping { get; set; }
    }

    [JsonObject]
    [DataContract]
    [MessagePackObject()]
    public class SlotNodeMapping : NodeMappingBase
    {

        public SlotNodeMapping()
        {
            this.NodeMapping = NodeMappingType.SlotNodeMapping;
        }

        [JsonProperty("nodeMap", Order = 1, NullValueHandling = NullValueHandling.Ignore)]
        [Key(1)]
        [DataMember]
        public NodeMappingBase NodeMap { get; set; }

        /// <summary>
        /// All slot values must be met to map to the assoicated story node.
        /// </summary>
        [JsonProperty("requiredSlotValues", Order = 2, NullValueHandling = NullValueHandling.Ignore)]
        [Key(2)]
        [NotMapped]
        [DataMember] 
        public Dictionary<string, List<string>> RequiredSlotValues
        {
            get;
            set;
        }

        [NotMapped]
        [YamlIgnore]
        [JsonProperty("nodeMappingType", Order =  0)]
        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember]
        [Key(3)]
        public sealed override NodeMappingType NodeMapping { get; set; }


        }

    [DataContract]
    [JsonObject()]
    [MessagePackObject()]
    public class SingleNodeMapping : NodeMappingBase
    {
        public SingleNodeMapping()
        {
            this.NodeMapping = NodeMappingType.SingleNodeMapping;
        }
        

        public SingleNodeMapping(string nodeName)
        {
            this.NodeMapping = NodeMappingType.SingleNodeMapping;
            NodeName = nodeName;
        }

        [JsonProperty("nodeName", Order = 1, NullValueHandling = NullValueHandling.Ignore)]
        [Key(1)]
        [DataMember]
        [NotMapped]
        public string NodeName { get; set; }

        [NotMapped]
        [JsonProperty("nodeMappingType", Order = 0)]
        [YamlIgnore]
        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember]
        [Key(2)]
        public sealed override NodeMappingType NodeMapping { get; set; }

        [DataMember]
        [YamlMember(Alias = "localizedSuggestionText")]
        [NotMapped]
        [Key(3)]
        [JsonProperty(PropertyName = "localizedSuggestionText", Order = 2, NullValueHandling = NullValueHandling.Ignore)]
        public List<LocalizedPlainText> LocalizedSuggestionText { get; set; }

    }

    [JsonObject]
    [DataContract]
    [MessagePackObject()]
    public class MultiNodeMapping : NodeMappingBase
    {

        public MultiNodeMapping()
        {
            this.NodeMapping = NodeMappingType.MultiNodeMapping;

        }

        [JsonProperty("nodeNames", Order = 1, NullValueHandling = NullValueHandling.Ignore)]
        [Key(1)]
        [DataMember]
        [NotMapped]
        public List<string> NodeNames { get; set; }

        [NotMapped]
        [JsonProperty("nodeMappingType", Order = 0)]
         [YamlIgnore]
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [Key(2)]
        public sealed override NodeMappingType NodeMapping { get; set; }


        [DataMember]
        [YamlMember(Alias = "localizedSuggestionText")]
        [NotMapped]
        [Key(3)]
        [JsonProperty(PropertyName = "localizedSuggestionText", Order = 2, NullValueHandling = NullValueHandling.Ignore)]
        public List<LocalizedPlainText> LocalizedSuggestionText { get; set; }

    }

    [JsonObject]
    [DataContract]
    [MessagePackObject()]
    public class ConditionalNodeMapping : NodeMappingBase
    {

        public ConditionalNodeMapping()
        {
            this.NodeMapping = NodeMappingType.ConditionalNodeMapping;
        }

        [YamlMember(Alias = "trueConditionResult", Order = 2)]
        [JsonProperty("trueConditionResult", Order = 1, NullValueHandling = NullValueHandling.Ignore)]
        [Key(1)]
        [DataMember]
        public NodeMappingBase TrueConditionResult { get; set; }

        [JsonProperty("falseConditionResult", Order = 2, NullValueHandling = NullValueHandling.Ignore)]
        [Key(2)]
        [YamlMember(Alias = "falseConditionResult", Order =2)]
        [DataMember]
        public NodeMappingBase FalseConditionResult { get; set; }

        [NotMapped]
        [JsonProperty("nodeMappingType", Order = 0)]
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [YamlIgnore]
        [Key(3)]
        public sealed override NodeMappingType NodeMapping { get; set; }

    }

}
