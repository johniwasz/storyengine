using System.ComponentModel.DataAnnotations.Schema;

namespace Whetstone.StoryEngine.Models.Data
{

    public enum StoryNodeType
    {
        HelpNode = 1,
        StartNode = 2,
        NewUserNode = 3,
        ReturningUserNode = 4,
        ResumeNode = 5,
        StopNode = 6,
        EndOfGameNode = 7



    }


    [Table("StandardNodes")]
    public class DataStandardNode
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [System.ComponentModel.DataAnnotations.Key]
        public long? Id { get; set; }


        public StoryNodeType NodeType { get; set; }


        public DataNode Node { get; set; }

    }
}
