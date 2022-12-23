using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Models.Admin
{
    public class ProjectVersion
    {

        [JsonProperty(PropertyName = "id")]
        public Guid? Id { get; set; }

        [JsonProperty(PropertyName = "projectId")]
        public Guid? ProjectId { get; set; }

        [JsonProperty(PropertyName = "version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "logFullClientMessages", NullValueHandling = NullValueHandling.Ignore)]
        public bool? LogFullClientMessages { get; set; }
    }
}
