using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;


namespace Whetstone.StoryEngine.Models.Messaging
{
    [JsonObject("message")]
    [DataContract]
    [Table("outbound_messages")]
    public class OutboundMessagePayload
    {


        /// <summary>
        /// System identifier of the individual text message.
        /// </summary>
        /// <remarks>
        /// System identifier assigned to the message.
        /// </remarks>
        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [System.ComponentModel.DataAnnotations.Key]
        [Column("id", Order = 0)]
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        [DataMember]
        [Column("outboundbatchrecordid", Order = 1)]
        [JsonProperty(PropertyName = "outboundBatchRecordId")]
        public Guid OutboundBatchRecordId { get; set; }

        /// <summary>
        /// Body of the SMS text message
        /// </summary>
        /// <value>
        /// Contents of the SMS message.
        /// </value>
        [JsonRequired]
        [DataMember]
        [Column("message", Order = 2)]
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }



        [DataMember]
        [Column("tags", Order = 3)]
        [JsonProperty(PropertyName = "tags", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> Tags { get; set; }


        /// <summary>
        /// Reports the current status of the message.
        /// </summary>
        /// <value>
        /// Current status of the individual text message.
        /// </value>
        [DataMember]
        [Column("status", Order = 4)]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "status", NullValueHandling = NullValueHandling.Ignore)]
        public MessageSendStatus? Status { get; set; }

        /// <summary>
        /// Store the identifier of the message provided by the message sender (e.g. Twilio)
        /// </summary>
        /// <value>
        /// The provider message identifier.
        /// </value>
        [DataMember]
        [Column("providermessageid", Order = 5)]
        [JsonProperty(PropertyName = "providerMessageId", NullValueHandling = NullValueHandling.Ignore)]
        public string ProviderMessageId { get; set; }



        /// <summary>
        /// History of the message.
        /// </summary>
        /// <value>
        /// Message processing history.
        /// </value>
        [ForeignKey("OutboundMessageId")]
        [JsonProperty(PropertyName = "results", NullValueHandling = NullValueHandling.Ignore)]
        public List<OutboundMessageLogEntry> Results { get; set; }




    }
}