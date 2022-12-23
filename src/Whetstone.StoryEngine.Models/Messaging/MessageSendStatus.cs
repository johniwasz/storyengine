using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;


namespace Whetstone.StoryEngine.Models.Messaging
{

    public enum MessageSendStatus
    {
        /// <summary>
        /// The message has been received by the engine workflow process.
        /// </summary>
        Requested = 1,
        /// <summary>
        /// The message has been sent to the message dispatching service, like Twilio, Simple Notification Service, etc.
        /// </summary>
        SentToDispatcher = 2,
        /// <summary>
        /// Message is throttled. The system should try resending.
        /// </summary>
        Throttled = 3,
        /// <summary>
        /// An unrecoverable error has been thrown and the message cannot be sent.
        /// </summary>
        Error = 4,
        /// <summary>
        /// The message has been queued by the dispatcher.
        /// </summary>
        Queued = 5,
        /// <summary>
        /// The message has been delivered.
        /// </summary>
        Delivered = 6
    }

    [Table("outboundmessage_logs")]
    [DataContract]
    [JsonObject("messageLogEntry")]
    public class OutboundMessageLogEntry
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DataMember]
        [Column("outboundmessageid", Order = 0)]
        [JsonProperty(PropertyName = "outboundMessageId")]
        public Guid OutboundMessageId { get; set; }

        [System.ComponentModel.DataAnnotations.Key]
        [DataMember]
        [Column("id", Order = 1)]
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        [Column("isexception", Order = 2)]
        [JsonProperty(PropertyName = "isException", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsException { get; set; }

        [Column("httpstatus", Order = 3)]
        [JsonProperty(PropertyName = "httpStatusCode", NullValueHandling = NullValueHandling.Ignore)]
        public int? HttpStatusCode { get; set; }

        [Column("extendedstatus", Order = 4)]
        [JsonProperty(PropertyName = "extendedStatus", NullValueHandling = NullValueHandling.Ignore)]
        public string ExtendedStatus { get; set; }

        [Column("status", Order = 5)]
        [JsonProperty(PropertyName = "status", NullValueHandling = NullValueHandling.Ignore)]
        public MessageSendStatus SendStatus { get; set; }


        [Column("logtime", Order = 6 )]
        [JsonRequired]
        [JsonProperty(PropertyName = "time")]
        public DateTime LogTime { get; set; }

        /// <summary>
        /// This is used to track the message in the status callback from Twilio and other SMS service providers that
        /// have status callback updates.
        /// </summary>
        /// <value>
        /// If using Twilio, this is the MessageServiceSid. 
        /// </value>
        [NotMapped]
        [JsonProperty(PropertyName = "serviceMessageId", NullValueHandling = NullValueHandling.Ignore)]
        public string ServiceMessageId { get; set; }

        /// <summary>
        /// This is the total time to the message provided took to send the message.
        /// </summary>
        /// <remarks>This includes any prefetch authentication logic, if required.</remarks>
        [JsonProperty(PropertyName = "providerSendDuration", NullValueHandling = NullValueHandling.Ignore)]
        [Column("providersendduration", Order = 7)]
        public long? ProviderSendDuration { get; set; }

    }
}
