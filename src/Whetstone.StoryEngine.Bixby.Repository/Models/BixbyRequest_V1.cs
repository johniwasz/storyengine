using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Whetstone.StoryEngine.Bixby.Repository.Models
{
    public class BixbyRequest_V1
    {

        [JsonProperty("newSession")]
        public bool NewSession { get; set; }

        [JsonProperty("applicationId")]
        public string ApplicationId { get; set; }

        [JsonProperty("intent")]
        public string Intent { get; set; }

        [JsonProperty("slots")]

        public Slot[] Slots { get; set; }

        [JsonProperty("$vivContext")]
        public VivContext Context { get; set; }

    }
}