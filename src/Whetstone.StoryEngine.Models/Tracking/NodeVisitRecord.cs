using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;

namespace Whetstone.StoryEngine.Models.Tracking
{
    [Table("node_visit_record")]
    [DataContract]
    public class NodeVisitRecord : IStoryCrumb
    {


        [Column("id")]
        [System.ComponentModel.DataAnnotations.Key()]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DataMember]
        public Guid? Id { get; set; }

        /// <summary>
        /// Name of the node visited
        /// </summary>

        [Column("name")]
        [DataMember]
        public string Name { get; set; }



        /// <summary>
        /// Number of times the user has visited the node.
        /// </summary>
        [DataMember]
        [Column("nodevisitcount")]
        public int VisitCount { get; set; }

        [JsonIgnore]
        [DataMember()]
        public long? UserId { get; set; }


        public override string ToString()
        {
            return string.Concat("NodeVisitRecord(", Name, ",Count ", VisitCount, ")");
        }

    }
}
