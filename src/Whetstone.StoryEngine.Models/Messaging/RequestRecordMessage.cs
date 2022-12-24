using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Messaging
{
    [DataContract]
    [JsonObject("requestrecord")]
    public class RequestRecordMessage
    {


        [JsonProperty(PropertyName = "engineRequestId")]
        [JsonRequired]
        [DataMember]
        public Guid EngineRequestId { get; set; }



        [JsonProperty(PropertyName = "engineSessionId")]
        [JsonRequired]
        [DataMember]
        public Guid EngineSessionId { get; set; }




        [JsonProperty(PropertyName = "engineUserId")]
        [JsonRequired]
        [DataMember]
        public Guid? EngineUserId { get; set; }

        /// <summary>
        /// This is the session id related to the connecting service.
        /// </summary>
        /// <remarks>If this connecting app is Alexa, then this is the Alexa session id.</remarks>

        [JsonProperty(PropertyName = "sessionId")]
        [JsonRequired]
        [DataMember]
        public string SessionId { get; set; }

        /// <summary>
        /// The user id provided by the connecting service. It is possible the user id is missing if request is a CanFulfill request.
        /// </summary>
        /// <remarks>If the connected service is Alexa, then this is the Alexa user id.</remarks>
        [JsonProperty(PropertyName = "userId")]
        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; set; }


        [DataMember]
        [JsonProperty(PropertyName = "deploymentId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid DeploymentId { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "requestId", NullValueHandling = NullValueHandling.Ignore)]
        public string RequestId { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "intentName", NullValueHandling = NullValueHandling.Ignore)]
        public string IntentName { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "slots", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> Slots { get; set; }

        [JsonRequired]
        [DataMember]
        [JsonProperty(PropertyName = "time")]
        public DateTime SelectionTime { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "isNewSession", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsNewSession { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "processDuration")]
        public long ProcessDuration { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "preNodeActionLog", NullValueHandling = NullValueHandling.Ignore)]
        public string PreNodeActionLog { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "postNodeActionLog", NullValueHandling = NullValueHandling.Ignore)]
        public string PostNodeActionLog { get; set; }

        /// <summary>
        /// This is the node the intent and slot values mapped to.
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "mappedNode", NullValueHandling = NullValueHandling.Ignore)]
        public string MappedNode { get; set; }

        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "requestType")]
        public StoryRequestType RequestType { get; set; }


        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "canFulfill", NullValueHandling = NullValueHandling.Ignore)]
        public YesNoMaybeEnum? CanFulfill { get; set; }


        [DataMember]
        [JsonProperty(PropertyName = "slotFulfillment", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, SlotCanFulFill> SlotFulFillment
        {
            get; set;

        }

        [DataMember]
        [JsonProperty(PropertyName = "rawText", NullValueHandling = NullValueHandling.Ignore)]
        public string RawText { get; set; }

        /// <summary>
        /// The likelihood that the NLP engine has confidence in the intent translation.
        /// </summary>
        /// <remarks>This is provided by some NPL engines, like DialogFlow and LUIS. This will be empty for Alexa.</remarks>
        [DataMember]
        [JsonProperty(PropertyName = "intentConfidence", NullValueHandling = NullValueHandling.Ignore)]
        public float? IntentConfidence { get; set; }

        /// <summary>
        /// Additional client-specific metadata that applies to the session.
        /// </summary>
        /// <remarks>Examples include device information about the client, environments, etc.</remarks>
        [DataMember]
        [JsonProperty(PropertyName = "sessionAttributes", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> SessionAttributes { get; set; }

        /// <summary>
        /// Additional client-specific metadata that applies to the request.
        /// </summary>
        /// <remarks>
        /// Examples include how the message was sent, like whether it was by voice or touch.
        /// </remarks>
        [DataMember]
        [JsonProperty(PropertyName = "requestAttributes", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> RequestAttributes { get; set; }

        /// <summary>
        /// Raw format of the inbound message coming from the client message. This is provided if RECORDMESSAGE is set to true.
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "requestBodyText", NullValueHandling = NullValueHandling.Ignore)]
        public string RequestBodyText { get; set; }



        /// <summary>
        /// Raw format of the outbound message being returned to the client application. This is provided if RECORDMESSAGE is set to true.
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "responseBodyText", NullValueHandling = NullValueHandling.Ignore)]
        public string ResponseBodyText { get; set; }


        [DataMember]
        [JsonProperty(PropertyName = "engineErrorText", NullValueHandling = NullValueHandling.Ignore)]
        public string EngineErrorText { get; set; }


        [DataMember]
        [JsonProperty(PropertyName = "responseConversionErrorText", NullValueHandling = NullValueHandling.Ignore)]
        public string ResponseConversionErrorText { get; set; }


        /// <summary>
        /// Indicates that the user is connecting to the title for the first time.
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "isFirstSession", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsFirstSession { get; set; }


        [DataMember]
        [JsonProperty(PropertyName = "isGuest", NullValueHandling = NullValueHandling.Ignore)]

        public bool? IsGuest { get; set; }
    }
}
