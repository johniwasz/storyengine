using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models
{
    public class AppMapping
    {
        [JsonProperty(PropertyName ="appId")]
        public string AppId { get; set; }

        [JsonProperty(PropertyName = "titleId")]
        public string TitleId { get; set; }

        [JsonProperty(PropertyName = "client", NullValueHandling = NullValueHandling.Ignore)]
        public Client? Client { get; set; }
    }
}
