using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Whetstone.StoryEngine.LambdaUtilities.ConfigUpdate
{

    public enum ConfigEntryType
    {
        ReportBucket = 1,
        TitleBucket = 2,
        ReportStepFunction = 3,
        SessionAuditQueue = 4,
        SessionLoggerType = 5,
        DynamoDbUserTable = 6,
        DynamoDbCacheTable = 7,
        CacheSlidingExpiration = 8,
        IsCacheEnabled = 9,
        TwilioLiveKey = 10,
        TwilioTestKey = 11,
        TwilioSourceNumber = 12,
        DefaultSmsSenderType = 13,
        MessageSendRetryLimit = 15,
        AdminApiClientId = 16,
        AdminApiMetadataAddress = 17,
        AdminApiResponseType = 18,
        DynamoDBMaxErrorRetries = 19,
        DynamoDBTimeout = 20,
        AuthenticatorType = 21,
        CognitoUserPoolId = 22,
        CognitoUserClientId = 23,
        CognitoUserClientSecret = 24,
        CognitoUserPoolRegion = 25,
        SocketConnectionTableName = 26,
        SocketWriteEndpoint = 27,
        PendingNotificationsTableName = 28,
        NotificationsLambdaArn = 29,
        NotificationsLambdaName = 30
    }

    public class ConfigUpdateRequest
    {

        [JsonProperty(PropertyName = "ServiceToken")]
        public string ServiceToken { get; set; }

        /// <summary>
        /// Name of the parameter to update.
        /// </summary>
        [JsonProperty(PropertyName = "Parameter")]
        public string Parameter { get; set; }



        /// <summary>
        /// Optional. Alias or ARN of the key used to encrypt the parameter.
        /// </summary>
        [JsonProperty(PropertyName = "KeyId")]
        public string KeyId { get; set; }

        [JsonProperty(PropertyName = "ConfigEntries")]
        public List<ConfigEntry> ConfigEntries { get; set; }

    }


    public class ConfigEntry
    {


        /// <summary>
        /// Value to apply to the bootstrap configuration setting.
        /// </summary>
        [JsonProperty(PropertyName = "Value")]
        public dynamic Value { get; set; }

        /// <summary>
        /// Indicate which configuration entry to update
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "ConfigType")]
        public ConfigEntryType ConfigType { get; set; }


    }
}
