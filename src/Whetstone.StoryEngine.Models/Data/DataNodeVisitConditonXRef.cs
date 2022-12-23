using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Models.Data
{
    [Table("NodeVisitConditionXRefs")]
    public class DataNodeVisitConditionXRef
    {

        [Column("ConditionId")]
        public long ConditionId { get; set; }


        [ForeignKey("ConditionId")]
        public DataNodeVisitCondition Condition { get; set; }

        [Column("NodeId")]
        public long NodeId { get; set; }


        [ForeignKey("NodeId")]
        public DataNode Node { get; set; }

    }
}
