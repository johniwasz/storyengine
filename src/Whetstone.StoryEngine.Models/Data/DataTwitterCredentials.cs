using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Data
{

    [JsonObject(Title = "TwitterCredentials")]
    [DataContract]
    [DebuggerDisplay("Id = {Id}, Name = {Name}")]
    [Table("org_twittercredentials")]
    public class DataTwitterCredentials
    {
        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [System.ComponentModel.DataAnnotations.Key]
        [Column("id", Order = 0)]
        [JsonProperty(PropertyName = "id")]
        public Guid? Id { get; set; }

        [Required]
        [DataMember]
        [ForeignKey("DataTwitterApplication")]
        [Column("twitterapplication_id", Order = 1)]
        public Guid TwitterApplicationId { get; set; }

        public DataTwitterApplication TwitterApplication { get; set; }


        [Required]
        [DataMember]
        [Column("name", Order = 2)]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [Required]
        [DataMember]
        [Column("description", Order = 3)]
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [Required]
        [DataMember]
        [Column("consumer_key", Order = 4)]
        [JsonProperty(PropertyName = "consumerKey")]
        public string ConsumerKey { get; set; }

        [Required]
        [DataMember]
        [Column("consumer_secret", Order = 5)]
        [JsonProperty(PropertyName = "consumerSecret")]
        public string ConsumerSecret { get; set; }


        [Required]
        [DataMember]
        [Column("access_token", Order = 6)]
        [JsonProperty(PropertyName = "accessToken")]
        public string AccessToken { get; set; }


        [Required]
        [DataMember]
        [Column("access_token_secret", Order = 7)]
        [JsonProperty(PropertyName = "accessTokenSecret")]
        public string AccessTokenSecret { get; set; }


        [Required]
        [DataMember]
        [Column("bearer_token", Order = 8)]
        [JsonProperty(PropertyName = "bearerToken")]
        public string BearerToken { get; set; }


        [DataMember]
        [Column("isenabled", Order = 9)]
        public bool IsEnabled { get; set; }

        [DataMember]
        [Column("isdeleted", Order = 10)]
        public bool IsDeleted { get; set; }



    }
}
