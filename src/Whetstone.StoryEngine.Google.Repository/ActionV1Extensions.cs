using Amazon.Lambda.APIGatewayEvents;
using Google.Protobuf.WellKnownTypes;
using System;
using Whetstone.StoryEngine.DependencyInjection;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.Google.Actions.V1;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models;
using Microsoft.Extensions.Logging;
using Whetstone.StoryEngine.Repository;
using Newtonsoft.Json.Linq;
using Whetstone.StoryEngine.Google.Repository.Models;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Story.Cards;
using System.Linq;

namespace Whetstone.StoryEngine.Google.Repository
{
    internal static class ActionV1Extensions
    {
        internal const string APPID = "appid";

        internal const string LAUNCHREQ = "actions.intent.MAIN";

        internal const string PHONENUMBER_INTENT = "PhoneNumberIntent";

        internal const string ENGINE_CONTEXT = "enginecontext";

        internal const string USER_STORAGE = "userStorage";

        internal const string REPROMPT_INTENT = "NoInput";


        /// <summary>
        /// Converts the WebhookRequest to a StoryRequest.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="appReader">Required to resolve the request to a title</param>
        /// <returns></returns>
        public static async Task<StoryRequest> ToStoryRequestAsync(this APIGatewayProxyRequest req, IAppMappingReader appReader,
            ILogger logger)
        {
            if (appReader == null)
                throw new ArgumentNullException(nameof(appReader));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));



            HandlerRequest handlerReq = HandlerRequest.FromJson(req.Body);

            StoryRequest storyReq = await req.ToStoryRequestAsync(appReader, logger, handlerReq);
         

            return storyReq;
        }

        public static async Task<StoryRequest> ToStoryRequestAsync(this APIGatewayProxyRequest req,  IAppMappingReader appReader,
                    ILogger logger, HandlerRequest handlerReq)
        {
            if (appReader == null)
                throw new ArgumentNullException(nameof(appReader));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            if(handlerReq == null)
                throw new ArgumentNullException(nameof(handlerReq));

            StoryRequest storyReq = await GetActionV1RequestAsync(req, appReader, logger, handlerReq);


            return storyReq;
        }


        public static APIGatewayProxyResponse ToAPIGatewayProxyResponse(this StoryResponse resp, StoryRequest storyReq, HandlerRequest origRequest,
           IMediaLinker linker, ILogger logger)
        {
            if (resp == null)
                throw new ArgumentNullException(nameof(resp));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            if (origRequest == null)
                throw new ArgumentNullException(nameof(origRequest));

            if (storyReq == null)
                throw new ArgumentNullException(nameof(storyReq));

            if (linker == null)
                throw new ArgumentNullException(nameof(linker));

            HandlerResponse actionResponse = InitializeResponse(resp, storyReq, origRequest);



            var locResp = resp.LocalizedResponse;
            actionResponse.Prompt = new Prompt
            {
                Override = true,
                FirstSimple = new Simple()
            };
            actionResponse.Prompt.FirstSimple.Speech = locResp.SpeechResponses.ToSsml(linker, resp.SessionContext.TitleVersion);
            actionResponse.Prompt.FirstSimple.Text = locResp.GeneratedTextResponse.GetCleanText();

            actionResponse.Scene = new Scene();
            actionResponse.Scene.Name = storyReq.RequestAttributes["scene"];
        
            

            if (!resp.ForceContinueSession)
            {

                //actionResponse.Scene.Slots = new Dictionary<string, dynamic>();
                //actionResponse.Scene.Slots.Add("status", "FINAL");
                actionResponse.Scene.Next = new NextScene
                {
                    Name = "Exit"
                };
                //actionResponse.Scene.SlotFillingStatus = SlotFillingStatus.Final;

            }


            if (resp.Suggestions != null && resp.Suggestions.Count>0)
            {
                actionResponse.Prompt.Suggestions = new List<Suggestion>();
                foreach(string storySuggestion in resp.Suggestions)
                {
                    Suggestion sug = new Suggestion();
                    sug.Title = storySuggestion;
                    actionResponse.Prompt.Suggestions.Add(sug);

                }
            }

            if (locResp.CardResponse != null)
            {

                if(actionResponse.Prompt.Content == null)
                {
                    actionResponse.Prompt.Content = new Content();
                }

                actionResponse.Prompt.Content.Card = GetCardContent(locResp.CardResponse, linker, storyReq.SessionContext?.TitleVersion);

            

            }


            //actionResponse.User = new User();
            //actionResponse.User.Params.Add("userid", "some userid");

            // For more information about building prompt responses:
            // https://developers.google.com/assistant/conversational/prompts

            //  string jsonResponse = await ProcessFlowRequestAsync(storyReq, surfaceCaps, isRepromptRequest, bodyText, request.Session);

            string jsonResponse = actionResponse.ToJson();

            APIGatewayProxyResponse apiResp = APIGatewayProxyExtensions.BuilGatewayResponse();

            if (string.IsNullOrWhiteSpace(jsonResponse))
            {
                apiResp.StatusCode = 500;
            }
            else
            {
                apiResp.StatusCode = 200;
                apiResp.Body = jsonResponse;

            }


            return apiResp;
        }

        private static Card GetCardContent(CardEngineResponse storyCard, IMediaLinker linker, Whetstone.StoryEngine.Models.Story.TitleVersion titleVer)
        {
            Card retCard = new Card();

            retCard.Title = string.IsNullOrWhiteSpace(storyCard.CardTitle) ? "Default title" : storyCard.CardTitle;

            if ((storyCard.Text?.Any()).GetValueOrDefault(false))
            {
                retCard.Text = string.Join(' ', storyCard.Text);
            }

            if (!string.IsNullOrWhiteSpace(storyCard.SmallImageFile))
            {

                retCard.Image = new Image();

                retCard.Image.Alt = "Card Image";

                retCard.Image.Url = linker.GetFileLink(titleVer, storyCard.SmallImageFile);
            }


            //// Add buttons if available.
            if ((storyCard.Buttons?.Any()).GetValueOrDefault(false))
            {
                // Card return only supports one link button. Take the first
                LinkButton storyLinkButton = storyCard.Buttons.FirstOrDefault(x => x.ButtonType == CardButtonType.Link) as LinkButton;

                if(storyLinkButton!=null)
                {
                    retCard.Button = new Link
                    {
                        Name = storyLinkButton.LinkText,
                        Open = new OpenUrl
                        {
                            Url = storyLinkButton.Url
                        }
                    };
                }               
            }

            return retCard;
        }

        private static async Task<StoryRequest> GetActionV1RequestAsync(APIGatewayProxyRequest req, IAppMappingReader appReader, ILogger logger, HandlerRequest handlerReq)
        {
            string body = req.Body;


            StoryRequest storyReq = InitializeRequest(req, handlerReq);
            //Struct intentRequestPayload = req.OriginalDetectIntentRequest?.Payload;

            //var conversationStruct = intentRequestPayload?.Fields?["conversation"];

            //string convType = conversationStruct?.StructValue?.Fields?["type"]?.StringValue;

            //storyReq.IsNewSession = convType != null && convType.Equals("NEW", StringComparison.OrdinalIgnoreCase);



            //storyReq.RequestAttributes = new Dictionary<string, string> { { "ConversationType", convType } };

            //var userStruct = intentRequestPayload?.Fields?["user"];

            //if (storyReq.IsNewSession.GetValueOrDefault(false))
            //{
            //    storyReq.RequestType = StoryRequestType.Launch;

            //    storyReq.SessionContext = new EngineSessionContext
            //    {
            //        EngineSessionId = Guid.NewGuid(),
            //        TitleVersion = await appReader.GetTitleAsync(Client.GoogleHome, storyReq.ApplicationId, alias)
            //    };


            //    if ((intentRequestPayload?.Fields?.Keys?.Contains("surface")).GetValueOrDefault(false))
            //    {

            //        Struct surface = intentRequestPayload.Fields?["surface"].StructValue;
            //        List<string> capabilityNames = new List<string>();
            //        if (surface != null)
            //        {
            //            if ((surface.Fields?.Keys?.Contains("capabilities")).GetValueOrDefault(false))
            //            {
            //                ListValue capList = surface.Fields?["capabilities"]?.ListValue;

            //                if (capList != null)
            //                {
            //                    foreach (var capVal in capList.Values)
            //                    {
            //                        if (capVal.StructValue != null)
            //                        {
            //                            Struct capStruct = capVal.StructValue;
            //                            if ((capStruct.Fields?.Keys?.Contains("name")).GetValueOrDefault(false))
            //                            {
            //                                string nameVal = capStruct.Fields?["name"].StringValue;
            //                                capabilityNames.Add(nameVal);
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }

            //        string capString = string.Join(",", capabilityNames);

            //        if (!string.IsNullOrWhiteSpace(capString))
            //        {
            //            if (storyReq.SessionAttributes == null)
            //                storyReq.SessionAttributes = new Dictionary<string, string>();

            //            storyReq.SessionAttributes.Add("DeviceCapabilities", capString);
            //        }
            //    }

            //}
            //else
            //{

            //    // Get the session context from request.
            //    if (req.QueryResult != null)
            //    {
            //        storyReq.SessionContext = GetStoredEngineContext(req);
            //    }


            //    if (req.IsRepromptRequest())
            //    {
            //        storyReq.RequestType = StoryRequestType.Reprompt;
            //    }
            //    else
            //    {
            //        storyReq.Intent = req.QueryResult.Intent.DisplayName;
            //        storyReq.RequestType = RequestReaderUtilities.GetRequestType(storyReq.Intent);
            //    }

            // Get the session context

            // If the intent type is launch, then treat this like a new session, even if session data is supplied.
            // The test console continued to supply the session context data even when starting new test sessions.
            EngineSessionContext sessionContext = storyReq.RequestType != StoryRequestType.Launch ? GetSessionContext(handlerReq) : null;


            // If the context is not found in the request, then initialize the session context.
            if (sessionContext == null)
            {
                logger.LogInformation("Session context is not in request.");
                sessionContext = new EngineSessionContext
                {
                    EngineSessionId = Guid.NewGuid(),
                    TitleVersion =
                        await appReader.GetTitleAsync(Client.GoogleHome, storyReq.ApplicationId, storyReq.Alias)
                };
               
            }
            else
            {
                storyReq.IsNewSession = false;            
            }

            storyReq.SessionContext = sessionContext;

            //}

            //storyReq.Slots = GetSlots(req);

            //storyReq.IsPingRequest = false;

            //// Determine if this is a health check. If it is, then set IsPingRequest to true
            //// so that the message is not logged to the database. 
            //if (req.OriginalDetectIntentRequest != null)
            //{
            //    string origIntentText = req.OriginalDetectIntentRequest.Payload.ToString();
            //    GoogleLegacyRequest healthCheck = JsonConvert.DeserializeObject<GoogleLegacyRequest>(origIntentText);

            //    storyReq.IsPingRequest = healthCheck?.IsHealthRequest();

            //    string requestType = healthCheck?.RequestType;

            //    if (!string.IsNullOrWhiteSpace(requestType))
            //        storyReq.RequestAttributes.Add("RequestType", requestType);


            //    RawInput rawInput = healthCheck?.Inputs?.FirstOrDefault()?.RawInputs?.FirstOrDefault();

            //    if (rawInput != null)
            //    {
            //        storyReq.RawText = rawInput.Query;

            //        if (!string.IsNullOrWhiteSpace(rawInput.InputType))
            //        {
            //            string inputTypeText = rawInput.InputType;
            //            storyReq.RequestAttributes.Add("InputType", inputTypeText);


            //            if (inputTypeText.Equals("VOICE"))
            //                storyReq.InputType = UserInputType.Voice;
            //            else if (inputTypeText.Equals("KEYBOARD"))
            //                storyReq.InputType = UserInputType.Keyboard;
            //            else if (inputTypeText.Equals("TOUCH"))
            //                storyReq.InputType = UserInputType.Touch;
            //            else
            //                storyReq.InputType = UserInputType.Other;
            //        }
            //        else
            //            storyReq.InputType = UserInputType.Unknown;
            //    }
            //}

            storyReq.IsGuest = true;


            if (handlerReq.User.VerificationStatus.GetValueOrDefault(VerificationStatus.UserVerificationStatusUnspecified) == VerificationStatus.Verified)
            {
                // If the user is verified and this is not a new session, then
                // the user storage should be present.
                if (handlerReq.User.Params!=null)
                {
                    if(handlerReq.User.Params.ContainsKey(USER_STORAGE))
                    {
                        
                        JObject userStorageJObject = handlerReq.User.Params[USER_STORAGE] as JObject;

                        if (userStorageJObject!=null)
                        {
                            UserStorage userStore = userStorageJObject.ToObject<UserStorage>();
                            storyReq.UserId = userStore.UserId;
                        }

                    }    
                }

                storyReq.IsGuest = false;
            }
            

            // Do not pass intent if this is a new session.
            if (storyReq.IsNewSession.GetValueOrDefault(false))
            {
                storyReq.Intent = null;
            }

            if (storyReq.IsGuest.GetValueOrDefault(false))
            {
                storyReq.UserId = storyReq.SessionId;
            }
            else if (string.IsNullOrWhiteSpace(storyReq.UserId) &&
                     (storyReq.SessionContext?.EngineUserId.HasValue).GetValueOrDefault(false))
            {
                storyReq.UserId = storyReq.SessionContext.EngineUserId.ToString();
            }

            if (storyReq.IsPingRequest.GetValueOrDefault(false))
                logger.LogInformation("Request is a HealthCheck ping request");

            return storyReq;
        }

        private static EngineSessionContext GetSessionContext(HandlerRequest handlerReq)
        {
            EngineSessionContext retContext = null;

            if (handlerReq.Session?.Params != null)
            {
                if (handlerReq.Session.Params.ContainsKey(ENGINE_CONTEXT))
                {
                    dynamic dynaContext = handlerReq.Session.Params[ENGINE_CONTEXT];

                    if (dynaContext != null)
                    {
                        JObject dynaJObject = dynaContext as JObject;
                        if (dynaJObject != null)
                        {
                            retContext = dynaJObject.ToObject<EngineSessionContext>();
                           
                        }
                       

                    }
                }
            }

            return retContext;
        }

        private static StoryRequest InitializeRequest(APIGatewayProxyRequest apiReq, HandlerRequest handlerReq)
        {


            string alias = apiReq.GetAliasRequest();
            string requestId = apiReq.RequestContext?.RequestId;
            string appId = apiReq.GetQueryStringValue(APPID);

            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new Exception($"No {APPID} found in query string");
            }

            string requestTimeText = apiReq.RequestContext?.RequestTime;
            DateTime? requestTime = null;

            if(DateTime.TryParse(requestTimeText, out DateTime messageTime))
            {
                requestTime = messageTime;
            }

            Guid engineRequestId = Guid.NewGuid();

            StoryRequest storyReq = new StoryRequest
            {
                Client = Client.GoogleHome,
                SessionId = handlerReq.Session.Id,
                Alias = alias,
                EngineRequestId = engineRequestId,
                ApplicationId = appId,
                RequestId = string.IsNullOrWhiteSpace(requestId) ? engineRequestId.ToString() : requestId,
                RequestTime = requestTime.HasValue ? requestTime.Value : DateTime.UtcNow,
                Locale = string.IsNullOrWhiteSpace(handlerReq.Session.LanguageCode) ? handlerReq.User.Locale : handlerReq.Session.LanguageCode,             
                RawText = handlerReq.Intent.Query,              
            };



            storyReq.RequestAttributes = new Dictionary<string, string>();
            storyReq.RequestAttributes.Add("scene", handlerReq.Scene.Name);

            if (handlerReq.Intent.Name.Equals(LAUNCHREQ, StringComparison.OrdinalIgnoreCase))
            {
                storyReq.RequestType = StoryRequestType.Launch;
                storyReq.IsNewSession = true;
            }
            else if (handlerReq.Intent.Name.Equals(PHONENUMBER_INTENT, StringComparison.OrdinalIgnoreCase))
            {
                // the inbound phone number needs to be massaged into a phone number slot as the 
                // Google Action handling is unexpected.
                storyReq.RequestType = StoryRequestType.Intent;
                storyReq.Intent = PHONENUMBER_INTENT;
                storyReq.Slots = GetPhoneNumberSlot(handlerReq.Intent);
            }
            else if(handlerReq.Intent.Name.Equals(REPROMPT_INTENT, StringComparison.OrdinalIgnoreCase))
            {
                storyReq.RequestType = StoryRequestType.Reprompt;
            }
            else if(handlerReq.Intent.Name.Equals(ReservedIntents.HelpIntent.Name))
            {
                storyReq.RequestType = StoryRequestType.Help;
            }
            else if (handlerReq.Intent.Name.Equals(ReservedIntents.BeingIntent.Name) ||
                handlerReq.Intent.Name.Equals(ReservedIntents.RestartIntent.Name)
                 )
            {
                storyReq.RequestType = StoryRequestType.Begin;
            }
            else if (handlerReq.Intent.Name.Equals(ReservedIntents.EndGameIntent.Name) ||
                handlerReq.Intent.Name.Equals(ReservedIntents.CancelIntent.Name) ||
                handlerReq.Intent.Name.Equals(ReservedIntents.StopIntent.Name))
            {
                storyReq.RequestType = StoryRequestType.Stop;
            }
            else if( handlerReq.Intent.Name.Equals(ReservedIntents.ResumeIntent.Name))
            {
                storyReq.RequestType = StoryRequestType.Resume;
            }
            else if (handlerReq.Intent.Name.Equals(ReservedIntents.RepeatIntent.Name))
            {
                storyReq.RequestType = StoryRequestType.Repeat;
            }
            else
            {
                storyReq.RequestType = StoryRequestType.Intent;
                storyReq.Intent = handlerReq.Intent.Name;
                storyReq.Slots = GetSlots(handlerReq.Intent);
            }

            UserStorage userStore = GetUserStorage(handlerReq);
            storyReq.UserId = userStore?.UserId;

            return storyReq;
        }

        private static Dictionary<string, string> GetPhoneNumberSlot(Intent intent)
        {
            Dictionary<string, string> retSlots = null;

            if (intent.Params != null)
            {
                retSlots = new Dictionary<string, string>();

                Dictionary<string, dynamic> slotParams = intent.Params;

                foreach (string slotName in slotParams.Keys)
                {
                    if (slotParams[slotName] is JObject slotValsJObject)
                    {

                        JToken phoneParts = slotValsJObject["resolved"] as JToken;
                        List<string> phonePartList = new List<string>();
                        if (phoneParts is JArray)
                        {
                            JArray phonaArray = phoneParts as JArray;
                           

                            for (int i = 0; i < phonaArray.Count; i++)
                            {
                                string phonePart = phonaArray[i].ToString();
                                StringBuilder phonePartBuilder = new StringBuilder();

                                for (int charIndex = 0; charIndex < phonePart.Length; charIndex++)
                                {
                                    char phoneChar = phonePart[charIndex];
                                    if (char.IsDigit(phoneChar))
                                    {
                                        phonePartBuilder.Append(phoneChar);
                                    }

                                }
                                string builtPhonePart = phonePartBuilder.ToString();
                                if (!string.IsNullOrWhiteSpace(builtPhonePart))
                                {
                                    phonePartList.Add(builtPhonePart);
                                }
                            }
                        }



                        string phoneNumber = string.Concat(phonePartList);

                        retSlots.Add("phonenumber", phoneNumber);

                    }

                }
            }

            return retSlots;
        }

        private static Dictionary<string, string> GetSlots(Intent intent)
        {
            Dictionary<string, string> retSlots = null;

            if (intent.Params != null)
            {
                retSlots = new Dictionary<string, string>();

                Dictionary<string, dynamic> slotParams = intent.Params;

                foreach (string slotName in slotParams.Keys)
                {
                    JObject slotValsJObject = slotParams[slotName] as JObject;
                    if (slotValsJObject != null)
                    {
                        retSlots.Add(slotName, slotValsJObject["resolved"].ToString());

                    }

                }
            }

            return retSlots;
        }

        private static UserStorage GetUserStorage(HandlerRequest handlerReq)
        {
            UserStorage userStore = null;

            if (handlerReq.User.Params != null)
            {
                Dictionary<string, dynamic> userParams = handlerReq.User.Params;

                if (userParams.ContainsKey(USER_STORAGE))
                {
                    JObject userStorageJObject = userParams[USER_STORAGE] as JObject;

                    if (userStorageJObject != null)
                    {
                        userStore = userStorageJObject.ToObject<UserStorage>();
                        
                    }
                }
            }

            return userStore;
        }

        private static HandlerResponse InitializeResponse(StoryResponse resp, StoryRequest req, HandlerRequest origRequest)
        {

            EngineSessionContext engineContext = resp.SessionContext;

            HandlerResponse handlerResp = new HandlerResponse();

            handlerResp.Session = new Session();
            handlerResp.Session.Params = new Dictionary<string, dynamic>();
            handlerResp.Session.Params.Add(ENGINE_CONTEXT, engineContext);

            handlerResp.Session.Id = origRequest.Session.Id;

            handlerResp.User = new Whetstone.Google.Actions.V1.User();
            handlerResp.User.Params = new Dictionary<string, dynamic>();

            UserStorage userStore = new UserStorage();
            userStore.UserId = req.UserId;
            handlerResp.User.Params.Add(USER_STORAGE, userStore);


            //var message = new Intent.Types.Message();
            //webResp.FulfillmentMessages.Add(message);
            //message.Platform = Intent.Types.Message.Types.Platform.ActionsOnGoogle;

            //// var payload = Struct.Parser.ParseJson("{\"google\": { \"expectUserResponse\": true}} ");



            //Value sessionState = new Value
            //{
            //    StringValue = JsonConvert.SerializeObject(engineContext)
            //};
            //itemContext.Parameters.Fields.Add(SessionContextName, sessionState);
            //webResp.OutputContexts.Add(itemContext);

            return handlerResp;
        }
    }
}
