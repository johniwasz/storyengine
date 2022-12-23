using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Models.Admin
{
    public class VersionDeployment
    {

        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "projectId")]
        public Guid ProjectId { get; set; }


        [JsonProperty(PropertyName = "versionId")]
        public Guid VersionId { get; set; }


        [JsonProperty(PropertyName = "clientType", NullValueHandling = NullValueHandling.Ignore)]
        public Client ClientType { get; set; }

        [JsonProperty(PropertyName = "clientId", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "publishDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime PublishDate { get; set; }

        [JsonProperty(PropertyName = "alias", NullValueHandling = NullValueHandling.Ignore)]
        public string Alias { get; set; }
    }
}
