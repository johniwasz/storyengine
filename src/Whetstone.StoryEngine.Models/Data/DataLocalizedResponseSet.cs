using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Models.Data
{
    [DataContract]
    [Table("LocResponseSet")]
    public class DataLocalizedResponseSet
    {
        [DataMember]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long? Id { get; set; }


        [DataMember]
        public List<DataLocalizedResponse> LocalizedResponses { get; set; }
    }
}
