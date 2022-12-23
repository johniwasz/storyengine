using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Tracking;
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using MessagePack;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Serialization;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;
using System.Linq;
using Whetstone.StoryEngine.Models;
using System.Xml.Serialization;

namespace Whetstone.StoryEngine.Models.Actions
{

    /// <summary>
    /// This marks what action is performed when a user visits a story node.
    /// </summary>
    [JsonConverter(typeof(NodeActionConverter))]   
    [Table("NodeActions")]
    [DataContract]
    [XmlInclude(typeof(InventoryActionData))]
    [XmlInclude(typeof(NodeVisitRecordActionData))]
    [XmlInclude(typeof(RecordSelectedItemActionData))]
    [XmlInclude(typeof(PhoneMessageActionData))]
    [XmlInclude(typeof(ResetStateActionData))]
    [XmlInclude(typeof(AssignSlotValueActionData))]
    [XmlInclude(typeof(GetPersonalInfoActionData))]
    [XmlInclude(typeof(ValidatePhoneNumberActionData))]
    [XmlInclude(typeof(SmsConfirmationActionData))]
    [XmlInclude(typeof(GetSmsConfirmationActionData))]
    [MessagePack.Union(0, typeof(InventoryActionData))]
    [MessagePack.Union(1, typeof(NodeVisitRecordActionData))]
    [MessagePack.Union(2, typeof(RecordSelectedItemActionData))]
    [MessagePack.Union(3, typeof(PhoneMessageActionData))]
    [MessagePack.Union(4, typeof(ResetStateActionData))]
    [MessagePack.Union(5, typeof(AssignSlotValueActionData))]
    [MessagePack.Union(6, typeof(GetPersonalInfoActionData))]
    [MessagePack.Union(7, typeof(ValidatePhoneNumberActionData))]
    [MessagePack.Union(8, typeof(SmsConfirmationActionData))]
    [MessagePack.Union(9, typeof(GetSmsConfirmationActionData))]
    public abstract  class NodeActionData
    {
        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [System.ComponentModel.DataAnnotations.Key]
        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }

      
        [IgnoreDataMember]
        [JsonIgnore]
        [NotMapped]
        public string ParentNodeName { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "nodeAction")]
        public abstract NodeActionEnum NodeAction { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "isPermanent", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsPermanent { get; set; }


       
    }
}
