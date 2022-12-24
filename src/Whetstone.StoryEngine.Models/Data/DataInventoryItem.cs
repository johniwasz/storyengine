using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Data
{

    [JsonObject(IsReference = true)]
    [DataContract]
    [Table("InventoryItems")]
    public class DataInventoryItem
    {

        public DataInventoryItem()
        {

        }

        [JsonProperty(PropertyName = "sysId", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        [DatabaseGenerated((DatabaseGeneratedOption.Identity))]
        [Key]
        [Column(Order = 0)]
        public long? Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        [DataMember]
        [Required]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "isMultiItem")]
        [DataMember]
        public bool IsMultiItem { get; set; }

        [JsonIgnore]
        [DataMember]
        public long VersionId { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        [Column("VersionId")]
        public DataTitleVersion StoryVersion { get; set; }

        [JsonIgnore]
        [DataMember]
        public ICollection<DataInventoryConditionXRef> InventoryConditionXRefs { get; set; }
    }
}
