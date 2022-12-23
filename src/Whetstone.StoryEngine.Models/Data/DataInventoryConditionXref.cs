using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Whetstone.StoryEngine.Models.Story;

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
