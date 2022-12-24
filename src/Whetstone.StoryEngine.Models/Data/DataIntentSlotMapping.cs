using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Data
{

    [DebuggerDisplay("Intent Alias = {Alias}")]
    [Table("IntentSlotMappings")]
    [DataContract]
    public class DataIntentSlotMapping
    {

        [DataMember]
        public long? IntentId { get; set; }

        /// <summary>
        /// Only intended to use with queries
        /// </summary>
        [ForeignKey("IntentId")]
        [DataMember]
        public DataIntent Intent { get; set; }

        [DataMember]
        [Required]
        public string Alias { get; set; }

        [DataMember]
        public long? SlotTypeId { get; set; }

        [ForeignKey("SlotTypeId")]
        [DataMember]
        public DataSlotType SlotType { get; set; }

    }
}
