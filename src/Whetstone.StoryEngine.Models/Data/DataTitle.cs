using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Data
{

    [JsonObject(Title = "Title", Description = "Root title definition")]
    [Table("titles")]
    public class DataTitle
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [System.ComponentModel.DataAnnotations.Key]
        [Column("id")]
        [DataMember]
        public Guid? Id { get; set; }



        [Required]
        [DataMember]
        [Column("shortname")]
        public string ShortName { get; set; }

        /// <summary>
        /// Public title of the audio skill.
        /// </summary>
        [Required]
        [DataMember]
        [Column("title")]
        public string Title { get; set; }



        /// <summary>
        /// Summary of the skill.
        /// </summary>   
        [DataMember]
        [Column("description")]
        public string Description { get; set; }

        [ForeignKey("TitleId")]
        [DataMember]
        [InverseProperty("Title")]
        public IEnumerable<DataTitleVersion> Versions { get; set; }


        [ForeignKey("TitleId")]
        [DataMember]
        [InverseProperty("Title")]
        public IEnumerable<DataTitleClientUser> TitleUsers { get; set; }


        public IEnumerable<DataTitleGroupXRef> TitleGroupXRef { get; set; }


    }
}
