using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Whetstone.StoryEngine.Models;

namespace Whetstone.StoryEngine.Models.Admin
{
    public class PublishVersionRequest
    {

        [JsonProperty(PropertyName = "titleName", NullValueHandling = NullValueHandling.Ignore)]
        public string TitleName { get; set; }

        [JsonProperty(PropertyName = "version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "clientType", NullValueHandling = NullValueHandling.Ignore)]
        public Client ClientType { get; set; }

        [JsonProperty(PropertyName = "clientId", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientId { get; set; }


        [JsonProperty(PropertyName = "alias", NullValueHandling = NullValueHandling.Ignore)]
        public string Alias { get; set; }

    }
}
