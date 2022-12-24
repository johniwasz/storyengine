using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Integration;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Models
{

    [JsonObject]
    [DataContract]
    public class StoryResponse
    {
        [DataMember]
        [JsonProperty(PropertyName = "localizedResponse", NullValueHandling = NullValueHandling.Ignore)]
        public LocalizedEngineResponse LocalizedResponse { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "choices", NullValueHandling = NullValueHandling.Ignore)]
        public List<Choice> Choices { get; set; }

        /// <summary>
        /// If true, it overrides the choice node evaluation logic and forces the session to continue.
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "forceContinueSession")]
        public bool ForceContinueSession { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "titleId", NullValueHandling = NullValueHandling.Ignore)]
        public string TitleId { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "titleVersion", NullValueHandling = NullValueHandling.Ignore)]
        public string TitleVersion { get; set; }


        /// <summary>
        /// This is not returned to the caller, but it is logged.
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "nodeName", NullValueHandling = NullValueHandling.Ignore)]
        public string NodeName { get; set; }


        /// <summary>
        /// Gets a list of node actions to do prior to returning to the user.
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "actions", NullValueHandling = NullValueHandling.Ignore)]
        public List<NodeActionData> Actions { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "searchResponses", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, SearchResponse> SearchResponses { get; set; }


        /// <summary>
        /// In the case of single intent invocations, we need an indicator there is more than one response
        /// so that the continue session flag is properly set and overrides the default single function invocation behavior.
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "hasNextAction")]
        public bool HasNextAction { get; set; }


        /// <summary>
        /// Indicates if the request was valid or not.
        /// </summary>
        /// <remarks>For example, the user issuing a request for help or a choice on a node is valid. An invalid request is an unrecognized intent or an intent that is not valid for the current node.</remarks>
        [DataMember]
        [JsonProperty(PropertyName = "isRequestValid")]
        public bool IsRequestValid { get; set; }


        /// <summary>
        /// Amount of time it took to process the request in the story engine in milliseconds
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "processDuration", NullValueHandling = NullValueHandling.Ignore)]
        public long ProcessDuration { get; set; }

        /// <summary>
        /// This is the text log of the actions performed before the node was evaluated.
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "preNodeActionLog", NullValueHandling = NullValueHandling.Ignore)]
        public string PreNodeActionLog { get; set; }

        /// <summary>
        /// This is the text log of the actions performed after the node was evaluated.
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "postNodeActionLog", NullValueHandling = NullValueHandling.Ignore)]
        public string PostNodeActionLog { get; set; }


        [DataMember]
        [JsonProperty(PropertyName = "suggestions", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Suggestions { get; set; }


        [DataMember]
        [JsonProperty(PropertyName = "errorText", NullValueHandling = NullValueHandling.Ignore)]
        public string EngineErrorText { get; set; }


        [DataMember]
        [JsonProperty(PropertyName = "responseConversionError", NullValueHandling = NullValueHandling.Ignore)]
        public string ResponseConversionError { get; set; }


        [IgnoreDataMember]
        [JsonProperty(PropertyName = "sessionContext", NullValueHandling = NullValueHandling.Ignore)]
        public EngineSessionContext SessionContext { get; set; }


        [DataMember]
        [JsonProperty(PropertyName = "auditBehavior", NullValueHandling = NullValueHandling.Ignore)]
        public AuditBehavior? AuditBehavior { get; set; }


    }
}
