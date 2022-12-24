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
    [DebuggerDisplay("RoleId = {RoleId}, GroupId = {GroupId}")]
    [Table("group_role_xrefs")]
    public class DataGroupRoleXRef
    {

        [Required]
        [Column("group_id")]
        public Guid GroupId { get; set; }



        public DataGroup Group { get; set; }


        [Required]
        [Column("role_id")]
        public Guid RoleId { get; set; }



        public DataRole Role { get; set; }

    }
}
