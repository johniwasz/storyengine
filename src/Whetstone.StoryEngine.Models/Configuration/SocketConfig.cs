using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Configuration
{
    public class SocketConfig
    {
        [JsonProperty(PropertyName = "socketConnectionTableName")]
        [YamlMember(Alias = "socketConnectionTableName")]
        public string SocketConnectionTableName { get; set; }

        [JsonProperty(PropertyName = "socketWriteEndpoint")]
        [YamlMember(Alias = "socketWriteEndpoint")]
        public string SocketWriteEndpoint { get; set; }

        [JsonProperty(PropertyName = "pendingNotificationsTableName")]
        [YamlMember(Alias = "pendingNotificationsTableName")]
        public string PendingNotificationsTableName { get; set; }

        [JsonProperty(PropertyName = "notificationsLambdaArn")]
        [YamlMember(Alias = "notificationsLambdaArn")]
        public string NotificationsLambdaArn { get; set; }

        [JsonProperty(PropertyName = "notificationsLambdaName")]
        [YamlMember(Alias = "notificationsLambdaName")]
        public string NotificationsLambdaName { get; set; }

    }
}
