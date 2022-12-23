using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Admin
{

    /// <summary>
    /// Returned by the client REST API
    /// </summary>
    public class TitleRoot
    {

        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "shortName", NullValueHandling = NullValueHandling.Ignore)]
        public string ShortName { get; set; }

        [JsonProperty(PropertyName = "description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "versions", NullValueHandling = NullValueHandling.Ignore)]
        public List<TitleVersionAdmin> Versions { get; set; }

    }

    /// <summary>
    /// Returned by the client REST API
    /// </summary>
    public class TitleVersionAdmin
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "logFullClientMessages", NullValueHandling = NullValueHandling.Ignore)]
        public bool LogFullClientMessages { get; set; }

        [JsonProperty(PropertyName = "deployments", NullValueHandling = NullValueHandling.Ignore)]
        public List<TitleVersionDeploymentBasic> Deployments { get; set; }

    }

    /// <summary>
    /// Returned by the client REST API
    /// </summary>
    public class TitleVersionDeploymentBasic
    {
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "clientType", NullValueHandling = NullValueHandling.Ignore)]
        public Client ClientType { get; set; }

        [JsonProperty(PropertyName = "clientId", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "publishDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? PublishDate { get; set; }

        [JsonProperty(PropertyName = "alias", NullValueHandling = NullValueHandling.Ignore)]
        public string Alias { get; set; }

    }
}
