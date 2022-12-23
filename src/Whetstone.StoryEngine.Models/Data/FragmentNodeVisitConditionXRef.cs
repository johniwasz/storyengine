using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Whetstone.StoryEngine.Models.Data
{

    [Table("FragmentNodeVisitConditionXRefs")]
    public class FragmentNodeVisitConditionXRef 
    {
        [Column("ConditionFragmentId")]
        public long ConditionFragmentId { get; set; }

        [ForeignKey("ConditionFragmentId")]
        public DataConditionalFragment ConditionFragment { get; set; }

        [Column("ConditionId")]
        public long ConditionId { get; set; }


        [ForeignKey("ConditionId")]
        public DataNodeVisitCondition Condition { get; set; }


    }
}
