using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Data
{

    [Table("Choices")]
    public class DataChoice
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [System.ComponentModel.DataAnnotations.Key]
        [Column("Id", Order = 0)]
        public long? Id { get; set; }


        [DataMember]
        public DataIntent Intent { get; set; }


        public DataChoice()
        {
        }


        public DataNode Node { get; set; }


        public ICollection<DataNodeVisitCondition> VisitConditions { get; set; }


        public ICollection<DataInventoryCondition> InventoryConditions { get; set; }


        public ICollection<ChoiceConditionVisitXRef> ChoiceConditionVisitXRefs { get; set; }

    }
}
