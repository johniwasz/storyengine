using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Whetstone.StoryEngine.Models.Story
{
    /// <summary>
    /// Returns the title and version mapping given the client type and client application id.
    /// </summary>
    [DebuggerDisplay("Title = {ShortName}, Version = {Version}")]
    public class TitleVersion
    {

        public TitleVersion()
        {

        }

        public TitleVersion(string shortName, string version)
        {
            ShortName = shortName;
            Version = version;
        }

        [JsonProperty(PropertyName = "shortName", NullValueHandling = NullValueHandling.Ignore)]
        public string ShortName { get; set; }

        [JsonProperty(PropertyName = "version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "titleId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? TitleId { get; set; }

        /// <summary>
        /// This value is populated from the database
        /// </summary>
        [JsonProperty(PropertyName = "versionId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? VersionId { get; set; }


        [JsonProperty(PropertyName = "deploymentId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? DeploymentId { get; set; }

        /// <summary>
        /// Alias provided by the client application.
        /// </summary>
        [JsonProperty(PropertyName = "alias", NullValueHandling = NullValueHandling.Ignore)]
        public string Alias { get; set; }


        /// <summary>
        /// Forces the system to log the full inbound and outbound client messages.
        /// </summary>
        /// <remarks>
        /// This overrides the default environment wide settings. For example, if the lambda environment settings are configured not to log all client messages and
        /// this value is set to true, then all client message are logged anyway.
        /// </remarks>
        [JsonProperty(PropertyName = "logFullClientMessages", NullValueHandling = NullValueHandling.Ignore)]
        public bool LogFullClientMessages { get; set; }
    }
}
