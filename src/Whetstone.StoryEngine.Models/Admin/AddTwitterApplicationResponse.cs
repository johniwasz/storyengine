using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Models.Admin
{
    public class AddTwitterApplicationResponse
    {

        [JsonProperty(PropertyName = "applicationId")]
        public long ApplicationId { get; set; }

        [JsonProperty(PropertyName = "twitterApplicationId")]
        public long TwitterApplicationId { get; set; }
    }
}
