using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Configuration
{

    /// <summary>
    /// Data in this class is used to launch the bootstrapping process before
    /// all other configuration data is loaded.
    /// </summary>
    /// <remarks>This tells the configuration logic where to go for the full configuration information. For example,
    /// it indicates which cache to read and which bucket to access for configuration.
    /// </remarks>
    public class BootstrapConfig
    {

        [JsonProperty(PropertyName = "bucket")]
        [YamlMember(Alias = "bucket", Order = 0)]
        public string Bucket { get; set; }

        [JsonProperty(PropertyName = "cacheConfig")]
        [YamlMember(Alias = "cacheConfig", Order = 1)]
        public CacheConfig CacheConfig { get; set; }

        [JsonProperty(PropertyName = "logLevel")]
        [YamlMember(Alias = "logLevel", Order = 2)]
        public LogLevel? LogLevel { get; set; }

        [JsonProperty(PropertyName = "enforceAlexaPolicy")]
        [YamlMember(Alias = "enforceAlexaPolicy", Order = 3)]
        public bool EnforceAlexaPolicy { get; set; }


        [JsonProperty(PropertyName = "sessionLoggerType", NullValueHandling = NullValueHandling.Ignore)]
        [YamlMember(Alias = "sessionLoggerType", Order = 4)]
        public SessionLoggerType? SessionLoggerType { get; set; }

        [JsonProperty(PropertyName = "sessionAuditQueue", NullValueHandling = NullValueHandling.Ignore)]
        [YamlMember(Alias = "sessionAuditQueue", Order = 5)]
        public string SessionAuditQueue { get; set; }


        /// <summary>
        /// Record client messages.
        /// </summary>
        [JsonProperty(PropertyName = "recordMessages")]
        [YamlMember(Alias = "recordMessages", Order = 6)]
        public bool RecordMessages { get; set; }

        [JsonProperty(PropertyName = "databaseSettings")]
        [YamlMember(Alias = "databaseSettings", Order = 8)]
        public DatabaseConfig DatabaseSettings { get; set; }

        [JsonProperty(PropertyName = "smsConfig")]
        [YamlMember(Alias = "smsConfig", Order = 9)]
        public SmsConfig SmsConfig { get; set; }

        [JsonProperty(PropertyName = "dynamoDBTables")]
        [YamlMember(Alias = "dynamoDBTables", Order = 10)]
        public DynamoDBTablesConfig DynamoDBTables { get; set; }

        [JsonProperty(PropertyName = "reportBucket")]
        [YamlMember(Alias = "reportBucket", Order = 11)]
        public string ReportBucket { get; set; }

        [JsonProperty(PropertyName = "reportStepFunction")]
        [YamlMember(Alias = "reportStepFunction", Order = 12)]
        public string ReportStepFunction { get; set; }

        [JsonProperty(PropertyName = "openIdSecurity")]
        [YamlMember(Alias = "openIdSecurity", Order = 13)]
        public OpenIdSecurity OpenIdSecurity { get; set; }


        [JsonProperty(PropertyName = "security")]
        [YamlMember(Alias = "security", Order = 14)]
        public SecurityConfig Security { get; set; }

        [JsonProperty(PropertyName = "socketConfig")]
        [YamlMember(Alias = "socketConfig", Order = 14)]
        public SocketConfig SocketConfig { get; set; }


        /// <summary>
        /// Used to set local debug configuration options. This should not appear in any deployed
        /// bootstrap config file.
        /// </summary>
        [JsonIgnore]
        [YamlIgnore]
        public DebugConfig Debug { get; set; }

    }
}
