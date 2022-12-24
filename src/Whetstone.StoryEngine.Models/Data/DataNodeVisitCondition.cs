using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Data
{

    [JsonObject()]
    [DataContract]
    [DebuggerDisplay("Name = {Name}; Id = {Id}")]
    [Table("ConditionsNodeVisit")]
    public class DataNodeVisitCondition
    {

        public DataNodeVisitCondition()
        {

            //FragmentNodeVisitConditionXRefs = new List<FragmentNodeVisitConditionXRef>();
            //ConditionalFragments = new JoinCollectionFacade<DataConditionalFragment, DataNodeVisitCondition, FragmentNodeVisitConditionXRef>(this,
            // FragmentNodeVisitConditionXRefs);

        }

        [DataMember]
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

        [JsonIgnore()]
        [Column("VersionId")]
        public DataTitleVersion StoryVersion { get; set; }


        public ICollection<DataNodeVisitConditionXRef> NodeVisitConditionXRefs { get; set; }



        private List<FragmentNodeVisitConditionXRef> FragmentNodeVisitConditionXRefs { get; } = new List<FragmentNodeVisitConditionXRef>();


        public ICollection<DataConditionalFragment> ConditionalFragments { get; set; }



        public ICollection<ChoiceConditionVisitXRef> ChoiceConditionVisitXRefs { get; set; }


    }
}
