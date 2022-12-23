using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Configuration
{
    public class DynamoDBTablesConfig
    {


        /// <summary>
        /// Name of the DynamoDB user table that mirrors the users stored in the PostgreSQL database.
        /// </summary>
        [JsonProperty(PropertyName = "userTable")]
        [YamlMember(Alias = "userTable", Order = 0)]
        public string UserTable { get; set; }



        /// <summary>
        /// This controls how long the client call waits for the endpoint to time out, irrespective of the cache provider. If the call is to a database, then it is a db connection timeout.
        /// If it is an HTTP endpoint, then it is an HTTP client timeout. Values is in milliseconds. Defaults to 2000 milliseconds.
        /// </summary>
        [YamlMember(Alias = "endpointTimeout", Order = 1)]
        [JsonProperty(PropertyName = "endpointTimeout", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int? Timeout { get; set; }


        /// <summary>
        /// Controls how many times the endpoint client retries the call. Defaults to 3.
        /// </summary>
        [YamlMember(Alias = "errorRetries", Order = 2)]
        [JsonProperty(PropertyName = "errorRetries", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public int? ErrorRetries { get; set; }

    }
}
