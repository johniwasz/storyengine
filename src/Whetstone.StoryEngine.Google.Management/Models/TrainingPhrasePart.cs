using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Google.Management.Models
{
    public class TrainingPhrasePart
    {

        [JsonProperty("text", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Text { get; set; }

        [JsonProperty("userDefined", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool UserDefined { get; set; }



        [JsonProperty("alias", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Alias { get; set; }



        [JsonProperty("meta", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Meta { get; set; }
    }
}
