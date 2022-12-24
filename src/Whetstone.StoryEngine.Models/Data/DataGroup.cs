using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Data
{
    [JsonObject(Title = "Group")]
    [DataContract]
    [DebuggerDisplay("UserId = {Id}, Name = {Name}")]
    [Table("groups")]
    public class DataGroup
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

        [Required]
        [DataMember]
        [Column("organization_id", Order = 3)]
        public Guid OrganizationId { get; set; }

        public IEnumerable<DataUserGroupXRef> UserGroupXRefs { get; set; }

        public IEnumerable<DataGroupRoleXRef> GroupRoleXRefs { get; set; }

        public IEnumerable<DataTitleGroupXRef> TitleGroupXRef { get; set; }
    }
}
