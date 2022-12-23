using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Google.Management.Models
{
    public class DialogFlowPackage
    {
        [JsonProperty("version", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Version { get; set; }
    }
}