using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.CoreApi.Models
{
    public class WebHookRegistrationRequest
    {
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
}
