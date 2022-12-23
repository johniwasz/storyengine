using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Reporting.Models
{
    public class ReportDestinationSftp : ReportDestinationBase
    {

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "destinationType")]
        [YamlIgnore]
        public override ReportDestinationType DestinationType
        {
            get => ReportDestinationType.SftpEndpoint;
            set
            { 
                // do nothing
            }

        }


        /// <summary>
        /// Name of the secret store parameter that contains the
        /// credentials to connect to the SFTP endpoint and
        /// host name.
        /// </summary>
        [JsonProperty(PropertyName = "secretStore")]
        [YamlMember(Alias = "secretStore")]
        public string SecretStore { get; set; }

        [JsonProperty(PropertyName = "directory")]
        [YamlMember(Alias =  "directory")]
        public string Directory { get; set; }


    }
}
