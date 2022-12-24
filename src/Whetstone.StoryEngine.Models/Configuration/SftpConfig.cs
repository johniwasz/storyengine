using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Configuration
{

    /// <summary>
    /// Defines a secure FTP configuration
    /// </summary>
    public class SftpConfig
    {

        /// <summary>
        /// User name to use to connect to the secure FTP site.
        /// </summary>
        [JsonProperty(PropertyName = "userName")]
        [YamlMember(Alias = "userName")]
        public string UserName { get; set; }

        /// <summary>
        /// Secret to pass to the SFTP server
        /// </summary>
        [JsonProperty(PropertyName = "userSecret")]
        [YamlMember(Alias = "userSecret")]
        public string UserSecret { get; set; }

        [JsonProperty(PropertyName = "host")]
        [YamlMember(Alias = "host")]
        public string Host { get; set; }

        [JsonProperty(PropertyName = "port")]
        [YamlMember(Alias = "port")]
        public int? Port { get; set; }

    }
}
