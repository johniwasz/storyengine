using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Models
{


    /// <summary>
    /// Most request types will be Intent. Audio clients also require special handling for starting, stopping or asking for help.
    /// </summary>
    public enum StoryRequestType
    {
        Unknown = 0,
        Launch = 1,
        Begin = 2,
        Resume = 3,
        Stop = 4,
        Pause = 5,
        Intent = 6,
        Help = 7,
        Reprompt = 8,
        Repeat = 9,
        CanFulfillIntent = 10

    }


    public enum Client
    {
        [EnumMember(Value = "unknown")]
        Unknown = 0,
        [EnumMember(Value = "alexa")]
        Alexa = 1,
        [EnumMember(Value = "googlehome")]
        GoogleHome = 2,
        [EnumMember(Value = "microsoftinvoke")]
        MicrosoftInvoke = 3,
        [EnumMember(Value = "facebookmessenger")]
        FacebookMessenger = 4,
        [EnumMember(Value = "sms")]
        Sms = 5,
        [EnumMember(Value = "bixby")]
        Bixby = 6
    }

    public enum UserInputType
    {

        [EnumMember(Value = "unknown")]
        Unknown = 0,
        [EnumMember(Value = "voice")]
        Voice = 1,
        [EnumMember(Value = "keyboard")]
        Keyboard = 2,
        [EnumMember(Value = "touch")]
        Touch = 3,
        [EnumMember(Value = "other")]
        Other = 99

    }

    public static class ReservedIntents
    {

        public readonly static StoryEngine.Models.Story.Intent ResumeIntent = new Story.Intent()
        {
            Name = "ResumeIntent",
            LocalizedIntents = new List<Story.LocalizedIntent>()
             {
                  new Story.LocalizedIntent() { Locale = null,

                   Utterances = new List<string>
                   {
                       "resume",
                       "resume game",
                       "resume adventure",
                       "resume investigation"
                   }
                  }
             }
        };

        public readonly static StoryEngine.Models.Story.Intent EndGameIntent = new Story.Intent()
        {
            Name = "EndGameIntent",
            LocalizedIntents = new List<Story.LocalizedIntent>()
             {
                  new Story.LocalizedIntent() { Locale = null,

                   Utterances = new List<string>
                   {
                       "end",
                       "end it",
                       "end game",
                       "end adventure",
                       "end investigation",
                       "game over"
                   }
                  }
             }
        };

        public readonly static StoryEngine.Models.Story.Intent StopIntent = new Story.Intent()
        {
            Name = "StopIntent",
            LocalizedIntents = new List<Story.LocalizedIntent>()
             {
                  new Story.LocalizedIntent() { Locale = null,

                   Utterances = new List<string>
                   {
                       "stop",
                       "stop it",
                       "stop game",
                       "stop adventure",
                       "stop i want to get off",
                       "stop investigation",
                       "exit",
                       "exit game",
                       "exit it"
                   }
                  }
             }
        };

        public readonly static StoryEngine.Models.Story.Intent RepeatIntent = new Story.Intent()
        {
            Name = "RepeatIntent",
            LocalizedIntents = new List<Story.LocalizedIntent>()
             {
                  new Story.LocalizedIntent() { Locale = null,

                   Utterances = new List<string>
                   {
                       "repeat",
                       "say again",
                       "what",
                       "what did you say",
                       "what was that",
                       "repeat that",
                       "repeat it"
                   }
                  }
             }
        };


        public readonly static StoryEngine.Models.Story.Intent HelpIntent = new Story.Intent()
        {
            Name = "HelpIntent",
            LocalizedIntents = new List<Story.LocalizedIntent>()
             {
                  new Story.LocalizedIntent() { Locale = null,

                   Utterances = new List<string>
                   {
                       "help",
                       "help me",
                       "I need help"
                   }
                  }
             }
        };

        public static readonly StoryEngine.Models.Story.Intent CancelIntent = new Story.Intent()
        {
            Name = "CancelIntent",
            LocalizedIntents = new List<Story.LocalizedIntent>()
             {
                  new Story.LocalizedIntent() { Locale = null,

                   Utterances = new List<string>
                   {
                       "cancel",
                       "cancel game",
                       "cancel it"
                   }
                  }
             }
        };


        public static readonly StoryEngine.Models.Story.Intent RestartIntent = new Story.Intent()
        {
            Name = "RestartIntent",
            LocalizedIntents = new List<Story.LocalizedIntent>()
             {
                  new Story.LocalizedIntent() { Locale = null,

                   Utterances = new List<string>
                   {
                       "restart",
                       "restart game",
                       "begin again",
                       "restart investigation",
                       "start over"
                   }
                  }
             }
        };

        public static readonly StoryEngine.Models.Story.Intent PauseIntent = new Story.Intent()
        {
            Name = "PauseIntent",
            LocalizedIntents = new List<Story.LocalizedIntent>()
             {
                  new Story.LocalizedIntent() { Locale = null,

                   Utterances = new List<string>
                   {
                       "pause",
                       "pause game",
                       "pause investigation"
                   }
                  }
             }
        };


        public static readonly StoryEngine.Models.Story.Intent BeingIntent = new Story.Intent()
        {
            Name = "BeginIntent",
            LocalizedIntents = new List<Story.LocalizedIntent>()
             {
                  new Story.LocalizedIntent() { Locale = null,

                   Utterances = new List<string>
                   {
                       "being",
                       "start game",
                       "begin game",
                       "start investigation",
                       "begin investigation",
                       "start"
                   }
                  }
             }
        };

        public static readonly StoryEngine.Models.Story.Intent YesIntent = new Story.Intent()
        {
            Name = "YesIntent",
            LocalizedIntents = new List<Story.LocalizedIntent>()
             {
                  new Story.LocalizedIntent() { Locale = null,

                   Utterances = new List<string>
                   {
                       "yes",
                       "yeah",
                       "ok",
                       "affirmative",
                       "go ahead",
                       "yes please",
                       "sure",
                       "yep"
                   }
                  }
             }
        };

        public readonly static StoryEngine.Models.Story.Intent NoIntent = new Story.Intent()
        {
            Name = "NoIntent",
            LocalizedIntents = new List<Story.LocalizedIntent>()
             {
                  new Story.LocalizedIntent() { Locale = null,

                   Utterances = new List<string>
                   {
                       "no",
                       "nah",
                       "nope",
                       "no thanks",
                       "no way",
                       "negative"
                   }
                  }
             }
        };



        public static List<StoryEngine.Models.Story.Intent> GetSystemIntents()
        {
            List<StoryEngine.Models.Story.Intent> sysIntents = new List<Story.Intent>();

            sysIntents.Add(ReservedIntents.BeingIntent);
            sysIntents.Add(ReservedIntents.CancelIntent);
            sysIntents.Add(ReservedIntents.EndGameIntent);
            sysIntents.Add(ReservedIntents.HelpIntent);
            sysIntents.Add(ReservedIntents.PauseIntent);
            sysIntents.Add(ReservedIntents.RepeatIntent);
            sysIntents.Add(ReservedIntents.RestartIntent);
            sysIntents.Add(ReservedIntents.ResumeIntent);
            sysIntents.Add(ReservedIntents.StopIntent);


            return sysIntents;

        }
    }


    /// <summary>
    /// This is the core story request. Each request coming in from Google Home, Alexa, etc. will be translated to a Story Request
    /// </summary>
    [JsonObject()]
    [DataContract]
    public class StoryRequest
    {

        /// <summary>
        /// This is the guid recorded to the audit table for the request. It is also used to associate other records, like SMS batch requests with the request
        /// that originated the batch request.
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "engineRequestId")]
        public Guid EngineRequestId { get; set; }

        /// <summary>
        /// A client-specific unique session identifier.
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "sessionId")]
        public string SessionId { get; set; }


        /// <summary>
        /// A client-provided application identifier (e.g. Alexa Skill Id)
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "applicationId", NullValueHandling = NullValueHandling.Ignore)]
        public string ApplicationId { get; set; }


        [DataMember]
        [JsonProperty(PropertyName = "alias", NullValueHandling = NullValueHandling.Ignore)]
        public string Alias { get; set; }
        /// <summary>
        /// A client-provided user identifier.
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "userId", NullValueHandling = NullValueHandling.Ignore)]
        public string UserId { get; set; }

        /// <summary>
        /// The intent requested by the user (e.g. Left, Right, etc.)
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "intent", NullValueHandling = NullValueHandling.Ignore)]
        public string Intent { get; set; }

        /// <summary>
        /// List of slot values. The key is the name and the value is the slot value.
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "slots", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> Slots { get; set; }

        /// <summary>
        /// Client-specific request id
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "requestId")]
        public string RequestId { get; set; }

        /// <summary>
        /// Determine the type of intent (e.g. Start, Stop, Intent, Help)
        /// </summary>
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "requestType")]
        public StoryRequestType RequestType { get; set; }


        /// <summary>
        /// Indicate which client is connecting.
        /// </summary>
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "client")]
        public Client Client { get; set; }


        /// <summary>
        /// The locale of the user's request.
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "locale", NullValueHandling = NullValueHandling.Ignore)]
        public string Locale { get; set; }

        /// <summary>
        /// Indicates if this is a new session. Alexa sets this value.
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "isNewSession", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsNewSession { get; set; }


        /// <summary>
        /// This applies to Google. Some users have no user identifiers. 
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "isGuest", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsGuest { get; set; }

        /// <summary>
        /// The time the request was received.
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "requestTime")]
        public DateTime RequestTime { get; set; }


        [DataMember]
        [JsonProperty(PropertyName = "securityInfo", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> SecurityInfo { get; set; }

        /// <summary>
        /// If true, then do not audit this call.
        /// </summary>
        /// <remarks>
        /// If this message is true, then this is a health check request sent from a hosting service (most likely Google Actions) to 
        /// verify that the action is responsive. This should not be audited for usage reporting.
        /// </remarks>
        [DataMember]
        [JsonProperty(PropertyName = "isPingRequest", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsPingRequest { get; set; }

        /// <summary>
        /// Contains the raw text of the request. This will be empty for Alexa requests, but populated for Google Assistant and text clients.
        /// </summary>
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
        /// Indicates the input mechanism, whether it's voice, keyboard, touch or other.
        /// </summary>
        /// <remarks>
        /// Some clients provide additional data about how the user submitted their response, like Google Assistant.
        /// </remarks>
        [DataMember]
        [JsonProperty(PropertyName = "inputType", NullValueHandling = NullValueHandling.Ignore)]
        public UserInputType? InputType { get; set; }



        /// <summary>
        /// This is stored in response to the client and returned on subsequent requests.
        /// </summary>
        [IgnoreDataMember]
        [JsonProperty(PropertyName = "sessionContext", NullValueHandling = NullValueHandling.Ignore)]
        public EngineSessionContext SessionContext { get; set; }

        public string GetUserHashKey()
        {
            var titleId = this.SessionContext?.TitleVersion?.TitleId;

            if (!titleId.HasValue)
                throw new Exception($"SessionContext does not contain the TitleId. Cannot generate user hash key from request {this.EngineRequestId} engine request id");

            if (string.IsNullOrWhiteSpace(this.UserId))
                throw new Exception(
                    $"UserId cannot be null or empty. Cannot generate user hash key from request {this.EngineRequestId} engine request id");


            string hashKey = $"{titleId.Value.ToString()}|{this.Client}|{this.UserId}";

            return hashKey;


        }


    }


    public class EngineSessionContext
    {
        [JsonProperty("userId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? EngineUserId { get; set; }

        [JsonProperty("sessionId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? EngineSessionId { get; set; }


        [JsonProperty("titleVersion")]
        public TitleVersion TitleVersion { get; set; }

        [JsonProperty("badIntentCounter")]
        public int BadIntentCounter { get; set; }

        [JsonProperty("sessionStartType", NullValueHandling = NullValueHandling.Ignore)]
        public SessionStartType? SessionStartType { get; set; }


    }

}
