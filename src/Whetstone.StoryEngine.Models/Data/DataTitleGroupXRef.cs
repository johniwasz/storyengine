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



    [JsonObject(Title = "UserGroupXRef")]
    [DataContract]
    [DebuggerDisplay("TitleId = {TitleId}, GroupId = {GroupId}")]
    [Table("title_group_xrefs")]
    public class DataTitleGroupXRef
    {
        [Required]
        [Column("title_id")]
        public Guid TitleId { get; set; }

        public DataTitle Title { get; set; }

        [Required]
        [Column("group_id")]
        public Guid GroupId { get; set; }

        public DataGroup Group { get; set; }

    }
}
