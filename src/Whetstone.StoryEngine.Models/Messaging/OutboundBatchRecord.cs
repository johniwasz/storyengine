using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Models.Messaging
{


    [JsonObject("outboundMessagesBatch")]
    [DataContract]
    [Table("outbound_batches")]
    public class OutboundBatchRecord : IOutboundNotificationRecord
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [System.ComponentModel.DataAnnotations.Key]
        [DataMember]
        [JsonRequired]
        [Column("id", Order =0)]
        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

        [DataMember]
        [JsonRequired]
        [Required]
        [Column("titleclientid", Order = 1)]
        [JsonProperty(PropertyName = "titleclientid")]
        public Guid? TitleUserId { get; set; }


        [DataMember]
        [Column("enginerequestid", Order = 2)]
        [JsonProperty(PropertyName = "engineRequestId")]
        public Guid? EngineRequestId { get; set; }

        [DataMember]
        [Column("smstonumberid", Order = 3)]
        [JsonProperty(PropertyName = "smsToNumberid")]
        public Guid? SmsToNumberId { get; set; }

      
        [DataMember]
        [Column("smsfromnumberid", Order = 4)]
        [JsonProperty(PropertyName = "smsFromNumberId")]
        public Guid? SmsFromNumberId { get; set; }


        [DataMember]
        [Column("consentid", Order = 5)]
        [JsonProperty(PropertyName = "consentId")]
        public Guid? ConsentId { get; set; }

        [DataMember]       
        [Column("smsprovider", Order = 6)]
        [JsonProperty(PropertyName = "smsprovider")]
        [JsonConverter(typeof(StringEnumConverter))]
        public SmsSenderType? SmsProvider { get; set; }

        [JsonRequired]
        [DataMember]
        [Column("allsent", Order = 7)]
        [JsonProperty(PropertyName = "allSent", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool AllSent { get; set; }


        [DataMember]
        [JsonRequired]
        [NotMapped]
        [JsonProperty(PropertyName = "sendAttemptsCount", DefaultValueHandling = DefaultValueHandling.Include)]
        public int SendAttemptsCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is saved to the database.
        /// </summary>
        /// <remarks>If it is not saved to the relational database, then it is saved to the DynamoDB database.</remarks>
        /// <value>
        ///   <c>true</c> if this instance is saved to the database; otherwise, <c>false</c>.
        /// </value>
        [JsonRequired]
        [NotMapped]
        [DataMember]
        [JsonProperty(PropertyName ="isSaved", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool IsSaved { get; set; }



        [ForeignKey("OutboundBatchRecordId")]
        [JsonRequired]
        [Required]
        [JsonProperty(PropertyName = "messages")]
        public List<OutboundMessagePayload> Messages
        {
            get; set;
        }



        [JsonProperty(PropertyName = "sentToPhone", NullValueHandling = NullValueHandling.Ignore)]
        public DataPhone SentToPhone { get; set; }

        [JsonProperty(PropertyName = "sentFromPhone", NullValueHandling = NullValueHandling.Ignore)]
        public DataPhone SentFromPhone { get; set; }

        [JsonProperty(PropertyName = "consent", NullValueHandling = NullValueHandling.Ignore)]
        public UserPhoneConsent Consent { get; set; }

        [JsonRequired]
        [NotMapped]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "notificationType")]
        public NotificationTypeEnum NotificationType { get { return NotificationTypeEnum.Sms; } set { /* do nothing */ }  }
    }
}