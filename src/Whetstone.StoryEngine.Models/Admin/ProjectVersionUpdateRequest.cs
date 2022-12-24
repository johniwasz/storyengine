using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Models.Admin
{
    public class ProjectVersionUpdateRequest
    {


        [JsonProperty(PropertyName = "description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "logFullClientMessages", NullValueHandling = NullValueHandling.Ignore)]
        public bool? LogFullClientMessages { get; set; }


    }
}
