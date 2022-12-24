using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Security
{
    public class ConfirmNewUserRequest
    {

        [JsonProperty(PropertyName = "name")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "confirmationCode")]
        public string ConfirmationCode { get; set; }



    }
}
