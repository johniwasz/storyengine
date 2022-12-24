using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Models.Twitter
{
    public class SubscriptionResponse
    {

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }
    }
}
