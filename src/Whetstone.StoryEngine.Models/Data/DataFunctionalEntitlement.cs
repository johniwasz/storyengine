using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Models.Data
{


    [JsonObject(Title = "FunctionalEntitlements")]
    [DataContract]
    [DebuggerDisplay("Id = {Id}, Name = {Name}, Claim = {Claim}")]
    [Table("funcentitlements")]
    public class DataFunctionalEntitlement
    {

        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [System.ComponentModel.DataAnnotations.Key]
        [Column("id", Order = 0)]
        public Guid Id { get; set; }

        [Required]
        [DataMember]
        [Column("name", Order = 1)]
        public string Name { get; set; }

        [Required]
        [DataMember]
        [Column("description", Order = 2)]
        public string Description { get; set; }

        [Required]
        [DataMember]
        [Column("claim", Order = 3)]
        public string Claim { get; set; }

        public IEnumerable<DataFunctionalEntitlementRoleXRef> FunctionalEntitlementRoleXRefs { get; set; }


    }
}
