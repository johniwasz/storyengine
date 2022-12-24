using Newtonsoft.Json;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Configuration
{
    public class CacheConfig
    {
        [YamlMember(Alias = "primaryEndpoint", Order = 0)]
        [JsonProperty(PropertyName = "primaryEndpoint", NullValueHandling = NullValueHandling.Ignore)]
        public string PrimaryEndpoint { get; set; }

        [YamlMember(Alias = "readOnlyEndpoints", Order = 1)]
        [JsonProperty(PropertyName = "readOnlyEndpoints", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> ReadOnlyEndpoints { get; set; }


        /// <summary>
        /// Maps to the authentication token which is stored securely. Use this to connect.
        /// </summary>
        [YamlMember(Alias = "token", Order = 2)]
        [JsonProperty(PropertyName = "token", NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; set; }

        [JsonProperty(PropertyName = "dynamoDBTableName", NullValueHandling = NullValueHandling.Ignore)]
        [YamlMember(Alias = "dynamoDBTableName", Order = 3)]
        public string DynamoDBTableName { get; set; }


        [YamlMember(Alias = "defaultSlidingExpirationSeconds", Order = 4)]
        [JsonProperty(PropertyName = "defaultSlidingExpirationSeconds", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int DefaultSlidingExpirationSeconds { get; set; }



        [YamlMember(Alias = "isEnabled", Order = 5)]
        [JsonProperty(PropertyName = "isEnabled", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public bool IsEnabled { get; set; }


        /// <summary>
        /// Controls how many times the Story Engine retries the call. Defaults to 2.
        /// </summary>
        [YamlMember(Alias = "maxEngineRetries", Order = 6)]
        [JsonProperty(PropertyName = "maxEngineRetries", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int? MaxEngineRetries { get; set; }


        [YamlMember(Alias = "engineTimeout", Order = 7)]
        [JsonProperty(PropertyName = "engineTimeout", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int? EngineTimeout { get; set; }


        [YamlMember(Alias = "inMemoryCacheSizeLimit", Order = 8)]
        [JsonProperty(PropertyName = "inMemoryCacheSizeLimit", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int? InMemoryCacheSizeLimit { get; set; }


    }
}
