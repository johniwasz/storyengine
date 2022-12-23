using Newtonsoft.Json;
using System;

namespace Whetstone.StoryEngine.Models.Admin
{
    public class UniqueId
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
    }
}
