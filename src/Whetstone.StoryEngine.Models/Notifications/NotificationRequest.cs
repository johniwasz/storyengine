using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Models.Notifications

{
    public class NotificationRequest
    {
        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "clientId")]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "requestType")]
        public NotificationRequestType RequestType { get; set; }

        [JsonProperty(PropertyName = "connectionId")]
        public string ConnectionId { get; set; }

        [JsonProperty(PropertyName = "notificationId")]
        public string NotificationId { get; set; }

        [JsonProperty(PropertyName = "notificationType")]
        public NotificationDataType NotificationType { get; set; }

        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }

    }
}
