using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Conditions
{
    public enum ConditionType
    {
        [EnumMember(Value = "inventory")]
        Inventory = 0,
        [EnumMember(Value = "nodevisit")]
        NodeVisit = 1,
        [EnumMember(Value = "userclientcondition")]
        UserClientCondition = 2,
        [EnumMember(Value = "slotvalue")]
        SlotValue = 3
    }

    [JsonConverter(typeof(NodeConditionConverter))]
    [DataContract]
    [XmlInclude(typeof(InventoryCondition))]
    [XmlInclude(typeof(NodeVisitCondition))]
    [XmlInclude(typeof(UserClientCondition))]
    [XmlInclude(typeof(SlotValueCondition))]
    [MessagePack.Union(0, typeof(InventoryCondition))]
    [MessagePack.Union(1, typeof(NodeVisitCondition))]
    [MessagePack.Union(2, typeof(UserClientCondition))]
    [MessagePack.Union(3, typeof(SlotValueCondition))]
    public abstract class StoryConditionBase
    {
        [DataMember]
        [JsonProperty("sysId")]
        [DatabaseGenerated((DatabaseGeneratedOption.Identity))]
        [Key]
        [Column(Order = 0)]
        public virtual long? Id { get; set; }


        [DataMember]
        [YamlMember]
        [Required]
        [JsonRequired]
        [Column(Order = 1)]
        public virtual string Name { get; set; }

        [DataMember]
        [YamlMember]
        [Column(Order = 2)]
        public virtual bool? RequiredOutcome { get; set; }

        [JsonIgnore]
        [YamlIgnore]
        [DataMember]
        public Choice ParentChoice { get; set; }

        [DataMember]
        [NotMapped]
        public abstract ConditionType ConditionType { get; set; }

        public abstract bool IsStoryCondition(ConditionInfo condInfo);

    }
}
