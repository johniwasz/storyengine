using System.ComponentModel.DataAnnotations.Schema;

namespace Whetstone.StoryEngine.Models.Data
{

    [Table("InventoryConditionItemXRefs")]
    public class DataInventoryConditionXRef
    {

        [Column("ConditionId")]
        public long ConditionId { get; set; }


        [ForeignKey("ConditionId")]
        public DataInventoryCondition Condition { get; set; }

        [Column("ItemId")]
        public long InventoryItemId { get; set; }


        [ForeignKey("InventoryItemId")]
        public DataInventoryItem InventoryItem { get; set; }

    }
}
