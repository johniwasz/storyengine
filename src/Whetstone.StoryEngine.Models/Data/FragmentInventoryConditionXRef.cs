using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Whetstone.StoryEngine.Models.Data
{
    [Table("FragmentInventoryConditionXRefs")]
    public class FragmentInventoryConditionXRef : IJoinEntity<DataConditionalFragment>, IJoinEntity<DataInventoryCondition>
    {
        [Column("ConditionFragmentId")]
        public long ConditionFragmentId { get; set; }

        [ForeignKey("ConditionFragmentId")]
        public DataConditionalFragment ConditionFragment { get; set; }

        [Column("ConditionId")]
        public long ConditionId { get; set; }


        [ForeignKey("ConditionId")]
        public DataInventoryCondition Condition { get; set; }

        DataConditionalFragment IJoinEntity<DataConditionalFragment>.Navigation
        {
            get => ConditionFragment;
            set => ConditionFragment = value;
        }

        DataInventoryCondition IJoinEntity<DataInventoryCondition>.Navigation
        {
            get => Condition;
            set => Condition = value;
        }

    }
}
