using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Security
{
    public class ConfirmationCodeResendRequest
    {

        [JsonProperty(PropertyName = "name")]
        public string UserName { get; set; }
    }
}
