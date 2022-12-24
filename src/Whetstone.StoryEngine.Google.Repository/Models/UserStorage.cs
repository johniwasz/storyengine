using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Google.Repository.Models
{
    public class UserStorage
    {

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }
    }
}
