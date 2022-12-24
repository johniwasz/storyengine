using Newtonsoft.Json;

namespace Whetstone.StoryEngine.CoreApi.Models
{
    public class WebHookRegistrationRequest
    {
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
}
