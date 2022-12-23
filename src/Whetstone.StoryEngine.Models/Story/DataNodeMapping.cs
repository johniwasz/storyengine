using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Whetstone.StoryEngine.Models.Conditions;

namespace Whetstone.StoryEngine.Models.Story
{


    /// <summary>
    /// This is the relational represenation of the node mapping relationship between choices and node direction.
    /// </summary>
    [Table("NodeMappings")]
    public class DataNodeMapping
    {


         [Column(Order = 0)]
         [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long? Id { get; set; }


        [Column(Order =1)]
        [ForeignKey("TrueResultId")]
        public DataNodeMapping TrueResult { get; set; }

        [Column(Order = 2)]
        [ForeignKey("FalseResultId")]
        public DataNodeMapping FalseResult { get; set; }


        [Column(Order = 3)]
        [ForeignKey("Id")]
        public List<StoryConditionBase> Conditions { get; set; }
    }
}
