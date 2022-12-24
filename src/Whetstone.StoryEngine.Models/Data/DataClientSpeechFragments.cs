using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Data
{

    [JsonObject(Title = "DataClientSpeechFragments")]
    [DataContract]
    [Table("clientspeechfrags")]
    public class DataClientSpeechFragments
    {

        [DataMember]
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }

        [DataMember]
        [Column("speechclient")]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("speechClient", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public Client? SpeechClient { get; set; }

        [DataMember]
        [ForeignKey("ClientSpeechFragId")]
        [JsonProperty("speechFragments", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public List<DataSpeechFragment> SpeechFragments { get; set; }

    }
}
