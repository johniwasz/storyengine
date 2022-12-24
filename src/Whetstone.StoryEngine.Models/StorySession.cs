using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models
{

    /// <summary>
    /// The connecting application could be any one of a number of services including Alexa or Cortana. Each
    /// service provides the same basic information, the user id and a session id. This is used to resume
    /// the game where the user left off.
    /// </summary>
    [Table("engine_session")]
    [DataContract]
    public class EngineSession
    {

        [System.ComponentModel.DataAnnotations.Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id", Order = 0)]
        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        public Guid? Id { get; set; }


        [Column("titleuserid", Order = 1)]
        [DataMember]
        [JsonProperty(PropertyName = "titleUserId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid TitleUserId { get; set; }


        /// <summary>
        /// This is the session id related to the connecting service.
        /// </summary>
        /// <remarks>If this connecting app is Alexa, then this is the Alexa session id.</remarks>
        [Column("sessionid", Order = 2)]
        [JsonProperty(PropertyName = "sessionId")]
        [JsonRequired]
        [DataMember]
        public string SessionId { get; set; }

        /// <summary>
        /// The user id provided by the connecting service. It is possible the user id is missing if request is a CanFulfill request.
        /// </summary>
        /// <remarks>If the connected service is Alexa, then this is the Alexa user id.</remarks>
        [Column("userid", Order = 3)]
        [JsonProperty(PropertyName = "userId")]
        [DataMember]
        public string UserId { get; set; }

        [Column("userlocale", Order = 4, TypeName = "varchar(10)")]
        [DataMember]
        [StringLength(10)]
        [JsonProperty(PropertyName = "locale", NullValueHandling = NullValueHandling.Ignore)]
        public string Locale { get; set; }


        [Column("deploymentid", Order = 5)]
        [DataMember]
        [JsonProperty(PropertyName = "deploymentId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid DeploymentId { get; set; }


        [Column("startdate", Order = 6)]
        [DataMember]
        [JsonProperty(PropertyName = "startDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// The last time the session connected. It is a UTC date time.
        /// </summary>
        [Column("lastaccesseddate", Order = 7)]
        [JsonProperty(PropertyName = "lastAccessedDate", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        public DateTime? LastAccessedDate { get; set; }


        /// <summary>
        /// Additional client-specific metadata that applies to the request.
        /// </summary>
        /// <remarks>
        /// Examples include how the message was sent, like whether it was by voice or touch.
        /// </remarks>
        [DataMember]
        [Column("sessionattributes")]
        [JsonProperty(PropertyName = "sessionAttributes", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> SessionAttributes { get; set; }

        [ForeignKey("SessionId")]
        [JsonProperty(PropertyName = "selections")]
        public List<EngineRequestAudit> Selections { get; set; }


        [DataMember]
        [Column("isfirstsession")]
        [JsonProperty(PropertyName = "isFirstSession", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsFirstSession { get; set; }


    }

    [Table("engine_requestaudit")]
    [DebuggerDisplay("RequestId={RequestId},IntentName={IntentName}")]
    // [JsonObject("Intent and node selection")]
    public class EngineRequestAudit
    {


        [System.ComponentModel.DataAnnotations.Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id", Order = 0)]
        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        public Guid? Id { get; set; }

        [Required]
        [Column("requestid", Order = 1)]
        [JsonProperty(PropertyName = "requestId", NullValueHandling = NullValueHandling.Ignore)]
        public string RequestId { get; set; }

        [Required]
        [Column("sessionid", Order = 2)]
        [JsonProperty(PropertyName = "sessionid", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? SessionId { get; set; }

        [Column("intentname", Order = 3)]
        [JsonProperty(PropertyName = "intentName", NullValueHandling = NullValueHandling.Ignore)]
        public string IntentName { get; set; }

        [Column("slots", Order = 4)]
        [JsonProperty(PropertyName = "slots", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> Slots { get; set; }

        [JsonRequired]
        [Column("selectiontime", Order = 5)]
        [JsonProperty(PropertyName = "time")]
        public DateTime SelectionTime { get; set; }

        [Column("processduration", Order = 6)]
        [Required]
        [JsonProperty(PropertyName = "processDuration")]
        public long ProcessDuration { get; set; }


        [Column("prenodeactionlog", Order = 7)]
        [JsonProperty(PropertyName = "preNodeActionLog", NullValueHandling = NullValueHandling.Ignore)]
        public string PreNodeActionLog { get; set; }


        [Column("postnodeactionlog", Order = 8)]
        [JsonProperty(PropertyName = "postNodeActionLog", NullValueHandling = NullValueHandling.Ignore)]
        public string PostNodeActionLog { get; set; }

        /// <summary>
        /// This is the node the intent and slot values mapped to.
        /// </summary>
        [Column("mappednode", Order = 9)]
        [JsonProperty(PropertyName = "mappedNode", NullValueHandling = NullValueHandling.Ignore)]
        public string MappedNode { get; set; }

        [Required]
        [Column("requesttype", Order = 10)]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "requestType")]
        public StoryRequestType RequestType { get; set; }


        [DataMember]
        [Column("canfulfill", Order = 11)]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "canFulfill", NullValueHandling = NullValueHandling.Ignore)]
        public YesNoMaybeEnum? CanFulfill { get; set; }


        [DataMember]
        [NotMapped]
        [JsonProperty(PropertyName = "slotFulfillment", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, SlotCanFulFill> SlotFulFillment
        {
            get; set;

        }


        private string SlotFulFillmentJson
        {

            get
            {
                if (SlotFulFillment == null)
                    return null;
                else
                    return JsonConvert.SerializeObject(SlotFulFillment);

            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    SlotFulFillment = null;
                else
                    SlotFulFillment = JsonConvert.DeserializeObject<Dictionary<string, SlotCanFulFill>>(value);
            }
        }


        /// <summary>
        /// Additional client-specific metadata that applies to the request.
        /// </summary>
        /// <remarks>
        /// Examples include how the message was sent, like whether it was by voice or touch.
        /// </remarks>
        [DataMember]
        [Column("requestattributes")]
        [JsonProperty(PropertyName = "requestAttributes", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> RequestAttributes { get; set; }

        [Column("rawtext")]
        [JsonProperty(PropertyName = "rawText")]
        public string RawText { get; set; }


        /// <summary>
        /// The likelihood that the NLP engine has confidence in the intent translation.
        /// </summary>
        /// <remarks>This is provided by some NPL engines, like DialogFlow and LUIS. This will be empty for Alexa.</remarks>
        [DataMember]
        [Column("intentconfidence")]
        [JsonProperty(PropertyName = "intentConfidence", NullValueHandling = NullValueHandling.Ignore)]
        public float? IntentConfidence { get; set; }


        [DataMember]
        [Column("responsebody")]
        [JsonProperty(PropertyName = "requestBodyText", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientRequestText { get; set; }


        [DataMember]
        [Column("requestbody")]
        [JsonProperty(PropertyName = "responseBodyText", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientResponseText { get; set; }


        [DataMember]
        [Column("engineerror")]
        [JsonProperty(PropertyName = "engineError", NullValueHandling = NullValueHandling.Ignore)]
        public string EngineError { get; set; }


        [DataMember]
        [Column("responseconversionerror")]
        [JsonProperty(PropertyName = "responseConversionError", NullValueHandling = NullValueHandling.Ignore)]
        public string ResponseConversionError { get; set; }
    }
}
