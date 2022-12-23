using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Conditions;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Tracking;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Data
{
    [JsonArray()]
    [JsonObject()]
    [DataContract]
    [DebuggerDisplay("Name = {Name}; Id = {Id}")]
    [Table("ConditionsInventory")]
    public class DataInventoryCondition
    {

        public DataInventoryCondition()
        {

            ConditionalFragments = new JoinCollectionFacade<DataConditionalFragment, DataInventoryCondition, FragmentInventoryConditionXRef>(this,
                FragmentInventoryConditionXRefs);

        }

        [DatabaseGenerated((DatabaseGeneratedOption.Identity))]
        [Key]
        [Column(Order = 0)]
        public long? Id { get; set; }

        [DataMember]
        [Required]
        [Column(Order = 1)]
        public string Name { get; set; }

        [DataMember]
        [Required]      
        [Column(Order = 2)]
        public bool RequiredOutcome { get; set; }
       
        [DataMember]
        public long? VersionId { get; set; }

        [JsonIgnore]
        [Column("VersionId")]
        public DataTitleVersion StoryVersion { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        [NotMapped]
        public ICollection<DataChoice> Choices { get; set; }

        private List<FragmentInventoryConditionXRef> FragmentInventoryConditionXRefs { get; } = new List<FragmentInventoryConditionXRef>();

        [JsonIgnore]
        [IgnoreDataMember]
        [NotMapped]
        public ICollection<DataConditionalFragment> ConditionalFragments { get; }

        [DataMember]
        public ICollection<DataInventoryConditionXRef> InventoryConditionXRefs { get; set; }


        public InventoryCondition ToInventoryCondition(List<DataInventoryItem> dataItems)
        {
            InventoryCondition invCondition = new InventoryCondition();

            invCondition.Id = this.Id;
            invCondition.Name = this.Name;
            invCondition.RequiredOutcome = this.RequiredOutcome;
            invCondition.InventoryItems = dataItems;
            return invCondition;
        }
    }
}
