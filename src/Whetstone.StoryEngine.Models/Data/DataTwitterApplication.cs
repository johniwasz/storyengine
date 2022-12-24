using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Data
{

    [JsonObject(Title = "TwitterApplication")]
    [DataContract]
    [DebuggerDisplay("Id = {Id}, Name = {Name}")]
    [Table("org_twitterapplications")]
    public class DataTwitterApplication
    {
        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [System.ComponentModel.DataAnnotations.Key]
        [Column("id", Order = 0)]
        [JsonProperty(PropertyName = "id")]
        public Guid? Id { get; set; }

        [Required]
        [DataMember]
        [Column("organization_id", Order = 1)]
        [JsonProperty(PropertyName = "organizationId")]
        public Guid OrganizationId { get; set; }


        [Required]
        [DataMember]
        [Column("name", Order = 2)]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [DataMember]
        [Column("description", Order = 3)]
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [Required]
        [DataMember]
        [Column("twitter_app_id", Order = 4)]
        [JsonProperty(PropertyName = "twitterappId")]
        public long TwitterAppId { get; set; }


        [DataMember]
        [Column("isenabled", Order = 5)]
        [JsonProperty(PropertyName = "isEnabled")]
        public bool IsEnabled { get; set; }

        [DataMember]
        [Column("isdeleted", Order = 6)]
        [JsonProperty(PropertyName = "isDeleted")]
        public bool IsDeleted { get; set; }

        [DataMember]
        [Column("title_version_id", Order = 7)]
        [JsonProperty(PropertyName = "titleVersionId")]
        public Guid? TitleVersionId { get; set; }

        [DataMember]
        [Column("isverified", Order = 8)]
        [JsonProperty(PropertyName = "isVerified")]
        public bool IsVerified { get; set; }

        public DataTitleVersion TitleVersion { get; set; }

        public DataTwitterCredentials TwitterCredentials { get; set; }


        [ForeignKey("TwitterApplicationId")]
        public List<DataTwitterSubscription> TwitterSubscriptions { get; set; }
    }
}
