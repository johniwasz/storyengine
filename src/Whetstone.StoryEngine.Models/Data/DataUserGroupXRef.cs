using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Data
{


    [JsonObject(Title = "UserGroupXRef")]
    [DataContract]
    [DebuggerDisplay("UserId = {UserId}, GroupId = {GroupId}")]
    [Table("user_group_xrefs")]
    public class DataUserGroupXRef
    {
        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }


        public DataUser User { get; set; }

        [Required]
        [Column("group_id")]
        public Guid GroupId { get; set; }


        public DataGroup Group { get; set; }

    }

}
