using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Models.Messaging
{
    public enum ClientMessageType
    {
        Request = 1,
        Response = 2
    }


    public class ClientMessageAudit
    {

        /// <summary>
        /// The type of client response
        /// </summary>
        [JsonProperty(PropertyName = "clientType")]
        public Client ClientType { get; set; }

        /// <summary>
        /// Session id provided by the client.
        /// </summary>
        [JsonProperty(PropertyName = "sessionId")]
        public string SessionId { get; set; }

        /// <summary>
        /// Request id provided by the client application
        /// </summary>
        [JsonProperty(PropertyName = "requestId")]
        public string RequestId { get; set; }



        /// <summary>
        /// This is the raw format of the client response or request message.
        /// </summary>
        [JsonProperty(PropertyName = "messagePayload")]
        public string MessagePayload { get; set; }


        /// <summary>
        /// Indicates if this is a request or response audit message.
        /// </summary>
        [JsonProperty(PropertyName = "auditType")]
        public ClientMessageType AuditType { get; set; }


        /// <summary>
        /// This is the raw format of the client response or request message.
        /// </summary>
        [JsonProperty(PropertyName = "requestBody")]
        public string RequestBody { get; set; }


        [JsonProperty(PropertyName = "responseBody")]
        public string ResponseBody { get; set; }

    }
}
