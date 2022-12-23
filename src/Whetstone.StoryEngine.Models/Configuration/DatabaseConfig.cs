using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Configuration
{
    public enum DBConnectionRetreiverType
    {
        /// <summary>
        /// Use IAM role based authentication
        /// </summary>
        IamRole = 1,

        /// <summary>
        /// If direct is specified, then the password will be retrieved from
        /// the Database connection attributes.
        /// </summary>
        Direct = 2,

        /// <summary>
        /// The secrets manager property must be set. The password is retrieved from the secret manager
        /// </summary>
        SecretsManager = 3
    }


    public class DatabaseConfig
    {
        /// <summary>
        /// Name value pairs are used to construct the connection string. A account authentication token will be used for the password.
        /// </summary>
        [JsonProperty(PropertyName = "settings")]
        [YamlMember(Alias = "settings")]
        public Dictionary<string, string> Settings { get; set; }


        /// <summary>
        /// Used by the story engine lambdas
        /// </summary>
        /// <remarks>
        /// Has access to read from the user table, title, version and deployment tables. Also can access phone and consent records.
        /// </remarks>
        [JsonProperty(PropertyName = "engineUser")]
        [YamlMember(Alias = "engineUser")]
        public string EngineUser { get; set; }

        /// <summary>
        /// Used by the core api
        /// </summary>
        [JsonProperty(PropertyName = "adminUser")]
        [YamlMember(Alias = "adminUser")]
        public string AdminUser { get; set; }

        /// <summary>
        /// Has access rights to invoke the function to log sessions and that's all.
        /// </summary>
        [JsonProperty(PropertyName = "sessionLoggingUser")]
        [YamlMember(Alias = "sessionLoggingUser")]
        public string SessionLoggingUser { get; set; }


        /// <summary>
        /// Can update SMS logs and read SMS messages to dispatch.
        /// </summary>
        [JsonProperty(PropertyName = "smsUser")]
        [YamlMember(Alias = "smsUser")]
        public string SmsUser { get; set; }

        /// <summary>
        /// Defaults to 5432 if not specified
        /// </summary>
        [JsonProperty(PropertyName = "port")]
        [YamlMember(Alias = "port")]
        public int? Port { get; set; }


        /// <summary>
        /// Number of seconds the RDS token is kept in memory. Defaults to 840 (14 min.) if not set. Amazon issued RDS
        /// authentication tokens are times for 15 min.
        /// </summary>
        [JsonProperty(PropertyName = "tokenExpirationSeconds", NullValueHandling =  NullValueHandling.Ignore)]
        [YamlMember(Alias = "tokenExpirationSeconds")]
        public int? TokenExpirationSeconds { get; set; }

        /// <summary>
        /// This logs SQL Server messages. Only enable this when actively debugging. This will record PII data in production. Delete any
        /// logs this generates after usage.
        /// </summary>
        [JsonProperty(PropertyName = "enableSensitiveLogging", NullValueHandling = NullValueHandling.Ignore)]
        [YamlMember(Alias = "enableSensitiveLogging")]
        public bool? EnableSensitiveLogging { get; set; }


        /// <summary>
        /// Drives which connection retriever. Default to IAM
        /// </summary>
        [JsonProperty(PropertyName = "connectionRetrieverType", NullValueHandling = NullValueHandling.Ignore)]
        [YamlMember(Alias = "connectionRetrieverType")]
        public DBConnectionRetreiverType? ConnectionRetrieverType { get; set; }


        /// <summary>
        /// Use the values here to drive the direct connect logic.
        /// </summary>
        [JsonProperty(PropertyName = "directConnect", NullValueHandling = NullValueHandling.Ignore)]
        [YamlMember(Alias = "directConnect")]
        public DBDirectConnectConfig DirectConnect { get; set; }

    }



    public class DBDirectConnectConfig
    {
        /// <summary>
        /// Pass token for connecting to the database.
        /// </summary>
        [JsonProperty(PropertyName = "clientSecret")]
        public string ClientSecret { get; set; }

        /// <summary>
        /// Override role-based security access user name with this value.
        /// </summary>
        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }

    }


}