using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Data
{

    [JsonObject(Title = "TwitterSubscription")]
    [DataContract]
    [DebuggerDisplay("Id = {Id}, Name = {Name}")]
    [Table("twittersubscriptions")]
    public class DataTwitterSubscription
    {

        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [System.ComponentModel.DataAnnotations.Key]
        [Column("id", Order = 0)]
        [JsonProperty(PropertyName = "id")]
        public Guid? Id { get; set; }

        [Required]
        [DataMember]
        [Column("application_id", Order = 1)]
        public Guid TwitterApplicationId { get; set; }


        [Required]
        [DataMember]
        [Column("twitter_user_id", Order = 2)]
        public long TwitterUserId { get; set; }


        [Required]
        [DataMember]
        [Column("enable_autofollowback", Order = 3)]
        public bool IsAutoFollowbackEnabled { get; set; }

    }
}
