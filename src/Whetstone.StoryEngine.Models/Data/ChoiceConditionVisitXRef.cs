using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Models.Data
{

    [Table("ChoiceConditionVisitXRefs")]
    public class ChoiceConditionVisitXRef 
    {
        [Column("ChoiceId")]
        public long ChoiceId { get; set; }


        [ForeignKey("ChoiceId")]
        public DataChoice Choice { get; set; }

        [Column("ConditionId")]
        public long NodeVisitConditionId { get; set; }


        [ForeignKey("NodeVisitConditionId")]
        public DataNodeVisitCondition Condition { get; set; }

    }
}
