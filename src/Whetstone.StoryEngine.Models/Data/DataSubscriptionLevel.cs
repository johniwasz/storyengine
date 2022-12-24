using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Data
{

    [JsonObject(Title = "SubscriptionLevel")]
    [DataContract]
    [DebuggerDisplay("Id = {Id}, Name = {Name}")]
    [Table("subscriptionlevels")]
    public class DataSubscriptionLevel
    {

        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [System.ComponentModel.DataAnnotations.Key]
        [Column("id", Order = 0)]
        public Guid? Id { get; set; }

        [Required]
        [DataMember]
        [Column("name", Order = 1)]
        public string Name { get; set; }

        [Required]
        [DataMember]
        [Column("description", Order = 2)]
        public string Description { get; set; }


        [ForeignKey("SubscriptionLevelId")]
        public List<DataOrganization> Organizations { get; set; }
    }
}
