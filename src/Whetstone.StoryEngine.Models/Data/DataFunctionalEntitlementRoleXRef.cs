using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Data
{


    [JsonObject(Title = "FunctionalEntitlementRoleXRef")]
    [DataContract]
    [DebuggerDisplay("RoleId = {Id}, Name = {Name}")]
    [Table("funcent_role_xrefs")]
    public class DataFunctionalEntitlementRoleXRef
    {
        [Required]
        [Column("func_entitlement_id")]
        public Guid FunctionalEntitlementId { get; set; }

        public DataFunctionalEntitlement FunctionalEntitlement { get; set; }

        [Required]
        [Column("role_id")]
        public Guid RoleId { get; set; }



        public DataRole Role { get; set; }


    }
}
