using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.CoreApi.Models
{
    public class CloneRequest
    {
        [JsonProperty(PropertyName = "titleId")]
        public string TitleId { get; set; }

        [JsonProperty(PropertyName = "sourceVersion")]
        public string SourceVersion { get; set; }

        [JsonProperty(PropertyName = "destinationVersion")]
        public string DestinationVersion { get; set; }
    }
}
