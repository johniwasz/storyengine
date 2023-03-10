using Newtonsoft.Json;
using System;

namespace Whetstone.StoryEngine.Models.Admin
{
    public class Project
    {

        [JsonProperty(PropertyName = "id")]
        public Guid? Id { get; set; }

        [JsonProperty(PropertyName = "shortName", NullValueHandling = NullValueHandling.Ignore)]
        public string ShortName { get; set; }

        [JsonProperty(PropertyName = "description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

    }
}
