using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Repository;
using Microsoft.Extensions.Logging;
using Whetstone.StoryEngine.Data;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf.WellKnownTypes;
using Whetstone.StoryEngine.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using Intent = Google.Cloud.Dialogflow.V2.Intent;
using Context = Google.Cloud.Dialogflow.V2.Context;
using Google.Protobuf.Collections;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Google.Repository.Models;
using Whetstone.StoryEngine.Models.Story.Cards;
using Whetstone.StoryEngine.Models.Story.Text;

namespace Whetstone.StoryEngine.Google.Repository
{
    public static class DialogFlowExtensions
    {
        private static readonly string SessionContextName = "enginecontext";
        
        private static readonly string INTENT_MAIN = "actions.intent.MAIN";

        /// <summary>
        /// This is provided if the user does not respond to a Google Action prompt or reprompt.
        /// </summary>
        private static readonly string INTENT_NOINPUT = "actions.intent.NO_INPUT";

        private static readonly string AUDIO_CAPABILITY = "actions.capability.AUDIO_OUTPUT";

        public static readonly string  SCREEN_CAPABILITY = "actions.capability.SCREEN_OUTPUT";

        public static readonly string WEBBROWSER_CAPABILITY = "actions.capability.WEB_BROWSER";

        public static readonly string MEDIAPLAYER_CAPABILITY = "actions.capability.MEDIA_RESPONSE_AUDIO";


        public static WebhookResponse ToDialogFlowReprompt(this StoryResponse response, SurfaceCapabilities surfaceCaps, IMediaLinker mediaLinker, ILogger responseLogger, string userId, string clientSessionId)
        {
            if (surfaceCaps == null)
                throw new ArgumentNullException(nameof(surfaceCaps));

            if (mediaLinker == null)
                throw new ArgumentNullException(nameof(mediaLinker));

            if (responseLogger == null)
                throw new ArgumentNullException(nameof(responseLogger));

            if (response == null)
                throw new ArgumentNullException(nameof(response));

            if (response.LocalizedResponse == null)
                throw new ArgumentException($"{nameof(response.LocalizedResponse)} has an empty response. This is an invalid conditionBase.");

            WebhookResponse webResp = InitializeResponse(response.SessionContext, clientSessionId);

            webResp.Payload = InitializeGooglePayload(response.ForceContinueSession, userId);

            var message = webResp.FulfillmentMessages[0];

            var localizedResponse = response.LocalizedResponse;

            var repromptResponse = localizedResponse.RepromptSpeechResponses;


            if (repromptResponse != null)
            {
                var simpleResp = new Intent.Types.Message.Types.SimpleResponse
                {
                    Ssml = repromptResponse?.ToSsml(mediaLinker,
                        response.SessionContext?.TitleVersion)
                };


                message.SimpleResponses = new Intent.Types.Message.Types.SimpleResponses();

                message.SimpleResponses.SimpleResponses_.Add(simpleResp);
            }
            else
            {
                message.SimpleResponses = new Intent.Types.Message.Types.SimpleResponses();
                var simpleResp = new Intent.Types.Message.Types.SimpleResponse
                {
                    TextToSpeech = string.Join("  ", localizedResponse.RepromptTextResponses)
                };

                message.SimpleResponses.SimpleResponses_.Add(simpleResp);
            }




            return webResp;

        }

        private static Struct InitializeGooglePayload(bool forceContinue, string userId)
        {

            Value payloadVal = new Value { StructValue = new Struct() };

            Value expectedUserResp = new Value { BoolValue = forceContinue };

            payloadVal.StructValue.Fields.Add("expectUserResponse", expectedUserResp);

            Value userStorageValue = new Value();

            UserStorage userStorage = new UserStorage { UserId = userId };
            userStorageValue.StringValue = JsonConvert.SerializeObject(userStorage);

            payloadVal.StructValue.Fields.Add("userStorage", userStorageValue);

            Struct payloadStruct = new Struct();

            payloadStruct.Fields.Add("google", payloadVal);

            return payloadStruct;
        }


        public static WebhookResponse ToFacebookMessengerResponse(this StoryResponse response,
            IMediaLinker mediaLinker, ILogger responseLogger, string contextPrefix)
        {

            if (mediaLinker == null)
                throw new ArgumentNullException(nameof(mediaLinker));

            if (responseLogger == null)
                throw new ArgumentNullException(nameof(responseLogger));

            if (response == null)
                throw new ArgumentNullException(nameof(response));

            if (response.LocalizedResponse == null)
                throw new ArgumentException(
                    $"{nameof(response.LocalizedResponse)} has an empty response. This is an invalid conditionBase.",
                    nameof(response));


            WebhookResponse webResp = InitializeResponse(response.SessionContext, contextPrefix);
            webResp.FulfillmentMessages.Clear();

            List<TextFragmentBase> textResponses = new List<TextFragmentBase>();
            if (response.LocalizedResponse.GeneratedTextResponse == null && (response.LocalizedResponse.CardResponse?.Text?.Any()).GetValueOrDefault(false))
            {
                // Pull the text response from the card.

                // ReSharper disable once AssignNullToNotNullAttribute
                foreach (string simpleText in response.LocalizedResponse.CardResponse.Text)
                {
                    textResponses.Add(new SimpleTextFragment(simpleText));
                }
            }
            else
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                textResponses.AddRange(response.LocalizedResponse.GeneratedTextResponse);
            }
           
            foreach (TextFragmentBase textResponse in textResponses)
            {

                if (textResponse is SimpleTextFragment simpleFrag)
                {
                    Intent.Types.Message fbTextResponse = GenerateFacebookTextResponse(simpleFrag.Text);
                    webResp.FulfillmentMessages.Add(fbTextResponse);
                }

                if (textResponse is AudioTextFragment audioFrag)
                {
                    string audioFileUrl = mediaLinker.GetFileLink(response.SessionContext?.TitleVersion, audioFrag.FileName);


                    Intent.Types.Message fbTextResponse = GenerateFacebookAudioResponse(audioFileUrl);
                    webResp.FulfillmentMessages.Add(fbTextResponse);

                }
            }

            // Append any buttons
            if ((response.Suggestions?.Any()).GetValueOrDefault(false))
            {
                // Get the last text response.
                var lastMsg = webResp.FulfillmentMessages.Last();

                var fbRespStruct = lastMsg.Payload.Fields["facebook"].StructValue;

                var quickReplies = new List<Value>();

                foreach (var suggestionButton in response.Suggestions)
                {
                    Struct quickButton = GetQuickReplyButton(suggestionButton);
                    quickReplies.Add(Value.ForStruct(quickButton));
                }

                fbRespStruct.Fields.Add("quick_replies", Value.ForList(quickReplies.ToArray()));
            }

            return webResp;
        }

        private static Intent.Types.Message GenerateFacebookTextResponse(string textResponse)
        {
            Intent.Types.Message fbTextResponse = new Intent.Types.Message
            {
                Platform = Intent.Types.Message.Types.Platform.Facebook
            };

            Struct fbMessageResp = new Struct();
            fbMessageResp.Fields.Add("text", Value.ForString(textResponse));

            Struct custPayload = new Struct();
            custPayload.Fields.Add("facebook", Value.ForStruct(fbMessageResp));

            fbTextResponse.Payload = custPayload;


            return fbTextResponse;
        }

        private static Intent.Types.Message GenerateFacebookAudioResponse(string audioFileUrl)
        {
            Intent.Types.Message fbTextResponse = new Intent.Types.Message
            {
                Platform = Intent.Types.Message.Types.Platform.Facebook
            };

            Struct fbMessageResp = new Struct();

            Struct attachment = new Struct();
            attachment.Fields.Add("type", Value.ForString("audio"));

            Struct payload = new Struct();
            payload.Fields.Add("url", Value.ForString(audioFileUrl));
            payload.Fields.Add("is_reusable", Value.ForBool(true));

            attachment.Fields.Add("payload", Value.ForStruct(payload));

            fbMessageResp.Fields.Add("attachment", Value.ForStruct(attachment));

            Struct custPayload = new Struct();
            custPayload.Fields.Add("facebook", Value.ForStruct(fbMessageResp));

            fbTextResponse.Payload = custPayload;


            return fbTextResponse;


        }

        private static Struct GetQuickReplyButton(string label)
        {

            Struct repStruct = new Struct();


            repStruct.Fields.Add("content_type", Value.ForString("text"));
            repStruct.Fields.Add("title", Value.ForString(label));
            repStruct.Fields.Add("payload", Value.ForString(label));


            return repStruct;
        }

        /// <summary>
            /// Converts the story response to an Alexa response.
            /// </summary>
            /// <param name="response"></param>
            /// <param name="environment"></param>
            /// <returns></returns>
            public static WebhookResponse ToDialogFlowResponse(this StoryResponse response,  SurfaceCapabilities surfaceCaps, IMediaLinker mediaLinker, ILogger responseLogger, string userId, string contextPrefix)
        {
            if (surfaceCaps == null)
                throw new ArgumentNullException(nameof(surfaceCaps));

            if (mediaLinker == null)
                throw new ArgumentNullException(nameof(mediaLinker));

            if (responseLogger == null)
                throw new ArgumentNullException(nameof(responseLogger));

            if (response == null)
                throw new ArgumentNullException(nameof(response));

            if (response.LocalizedResponse == null)
                throw new ArgumentException(nameof(response), 
                    $"{nameof(response.LocalizedResponse)} has an empty response. This is an invalid conditionBase.");


            WebhookResponse webResp = InitializeResponse(response.SessionContext, contextPrefix);

            webResp.Payload = InitializeGooglePayload(response.ForceContinueSession, userId);

            var message = webResp.FulfillmentMessages[0];

            var localizedResponse = response.LocalizedResponse;

            // Get an alexa response and if it does not exist, then get the default client response
            var speechResponse = localizedResponse.SpeechResponses;

            

            // speechResponse = null;
            if (speechResponse != null)
            {
                var simpleResp = new Intent.Types.Message.Types.SimpleResponse
                {
                    Ssml =  speechResponse?.ToSsml(mediaLinker,
                        response?.SessionContext?.TitleVersion)
                };

                message.SimpleResponses = new Intent.Types.Message.Types.SimpleResponses();

                message.SimpleResponses.SimpleResponses_.Add(simpleResp);
            }
            else 
            {
                string generatedText = localizedResponse.GeneratedTextResponse.GetCleanText();
                if (!string.IsNullOrWhiteSpace(generatedText))
                {
                    message.SimpleResponses = new Intent.Types.Message.Types.SimpleResponses();
                    var simpleResp = new Intent.Types.Message.Types.SimpleResponse
                    {
                        TextToSpeech = generatedText
                    };
                    message.SimpleResponses.SimpleResponses_.Add(simpleResp);
                }

            }

            // TODO do not consider the send card response boolean.

            if (localizedResponse.CardResponse!=null  &&
                 (surfaceCaps.HasScreen || surfaceCaps.HasWebBrowser))
            {

                var basicCardResponse = new Intent.Types.Message
                {
                    Platform = Intent.Types.Message.Types.Platform.ActionsOnGoogle,
                    BasicCard = new Intent.Types.Message.Types.BasicCard(),
                };

                CardEngineResponse storyCard = localizedResponse.CardResponse;


                string cardText = null;
                
                if((storyCard.Text?.Any()).GetValueOrDefault(false))
                    cardText = string.Join(' ', storyCard.Text);

                basicCardResponse.BasicCard.FormattedText = string.IsNullOrWhiteSpace(cardText) ? string.Empty : cardText;

                string cardTitle = string.IsNullOrWhiteSpace(storyCard.CardTitle) ? "Default title" : storyCard.CardTitle;
 
                basicCardResponse.BasicCard.Title = cardTitle;

                bool hasImage = false;
                if (!string.IsNullOrWhiteSpace(storyCard.SmallImageFile) ||
                     !string.IsNullOrWhiteSpace(storyCard.LargeImageFile))
                {
                    if (!string.IsNullOrWhiteSpace(storyCard.SmallImageFile))
                    {

                        basicCardResponse.BasicCard.Image = new Intent.Types.Message.Types.Image
                        {
                            ImageUri = mediaLinker.GetFileLink(response.SessionContext?.TitleVersion,
                                storyCard.SmallImageFile),
                            AccessibilityText = "Card Image"
                        };

                        hasImage = true;
                    }
                }

                // Add buttons if available.
                if((storyCard.Buttons?.Any()).GetValueOrDefault(false))
                {
                    foreach(CardButton storyButton in storyCard.Buttons)
                    {

                        if (storyButton is LinkButton linkButton)
                        {
                            Intent.Types.Message.Types.BasicCard.Types.Button btn =
                                new Intent.Types.Message.Types.BasicCard.Types.Button {Title = linkButton.LinkText};

                            Intent.Types.Message.Types.BasicCard.Types.Button.Types.OpenUriAction uriAction =
                                new Intent.Types.Message.Types.BasicCard.Types.Button.Types.OpenUriAction
                                {
                                    Uri = linkButton.Url
                                };

                            btn.OpenUriAction = uriAction;
                            basicCardResponse.BasicCard.Buttons.Add(btn);
                        }
                    }
                }

                if (hasImage)
                    webResp.FulfillmentMessages.Add(basicCardResponse);

            }

            if((response.Suggestions?.Any()).GetValueOrDefault(false))
            {
                var suggestionMessage = new Intent.Types.Message
                {
                    Platform = Intent.Types.Message.Types.Platform.ActionsOnGoogle,
                    Suggestions = new Intent.Types.Message.Types.Suggestions()
                };


                if (response.Suggestions != null)
                    foreach (string suggestion in response.Suggestions)
                    {
                        var textSuggestion = suggestion.Trim();

                        if (textSuggestion.Length > 25)
                            Debug.WriteLine("Suggestion is too long");

                        suggestionMessage.Suggestions.Suggestions_.Add(new Intent.Types.Message.Types.Suggestion()
                            {Title = textSuggestion});
                    }

                webResp.FulfillmentMessages.Add(suggestionMessage);
            }


            return webResp;
        }

        private static WebhookResponse InitializeResponse(EngineSessionContext engineContext, string contextPrefix)
        {
            WebhookResponse webResp = new WebhookResponse();

            var message = new Intent.Types.Message();
            webResp.FulfillmentMessages.Add(message);
            message.Platform = Intent.Types.Message.Types.Platform.ActionsOnGoogle;

            // var payload = Struct.Parser.ParseJson("{\"google\": { \"expectUserResponse\": true}} ");


            // Store the engine session context (allow it to expire after 20 min)
            Context itemContext = new Context
            {
                LifespanCount  = 99,
                Name = GetContextName(contextPrefix),
                Parameters = new Struct()
            };

            Value sessionState = new Value
            {
                StringValue = JsonConvert.SerializeObject(engineContext)
            };
            itemContext.Parameters.Fields.Add(SessionContextName, sessionState);
            webResp.OutputContexts.Add(itemContext);

            return webResp;
        }


        private static void ApplyCapability(string capText, SurfaceCapabilities sufCaps)
        {
            if (!string.IsNullOrWhiteSpace(capText))
            {
                if (capText.Equals(AUDIO_CAPABILITY, StringComparison.OrdinalIgnoreCase))
                {
                    sufCaps.HasAudio = true;
                }
                else if (capText.Equals(SCREEN_CAPABILITY, StringComparison.OrdinalIgnoreCase))
                {
                    sufCaps.HasScreen = true;
                }
                else if (capText.Equals(MEDIAPLAYER_CAPABILITY, StringComparison.OrdinalIgnoreCase))
                {
                    sufCaps.HasMediaAudio = true;
                }
                else if (capText.Equals(WEBBROWSER_CAPABILITY, StringComparison.OrdinalIgnoreCase))
                {
                    sufCaps.HasWebBrowser = true;
                }
            }
        }

        public static SurfaceCapabilities GetSurfaceCapabilities(this WebhookRequest webReq)
        {
            if (webReq == null)
                throw new ArgumentNullException($"{nameof(webReq)}");


            SurfaceCapabilities surfaceCaps = new SurfaceCapabilities();

            Struct structPayLoad = webReq.OriginalDetectIntentRequest?.Payload;

            if (structPayLoad != null)
            {
                if (structPayLoad.Fields.Keys.Contains("surface"))
                {

                    Struct surface = structPayLoad.Fields["surface"].StructValue;

                    if(surface!=null)
                    {
                        if((surface.Fields?.Keys?.Contains("capabilities")).GetValueOrDefault(false))
                        {
                            ListValue capList = surface.Fields?["capabilities"]?.ListValue;

                            if(capList!=null)
                            {
                                foreach (var capVal in capList.Values)
                                {
                                    if (capVal.StructValue != null)
                                    {
                                        Struct capStruct = capVal.StructValue;

                                        if ((capStruct.Fields?.Keys?.Contains("name")).GetValueOrDefault(false))
                                        {
                                            string nameVal = capStruct.Fields?["name"].StringValue;

                                            ApplyCapability(nameVal, surfaceCaps);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }


            return surfaceCaps;

        }


        public static bool IsHealthRequest(this GoogleLegacyRequest healthCheck)
        {

            bool isPingRequest = false;

            var firstInput = healthCheck.Inputs?.FirstOrDefault(x => (x.Intent?.Equals(INTENT_MAIN)).GetValueOrDefault(false));

            var arg = firstInput?.Arguments?.FirstOrDefault(x => (x.Name?.Equals("is_health_check")).GetValueOrDefault(false));

            if (arg != null)
            {
                isPingRequest = arg.BoolValue;

            }

            return isPingRequest;

        }


        internal static bool IsRepromptRequest(this WebhookRequest request)
        {
            bool isRepromptRequest = false;

            if ((request.OriginalDetectIntentRequest.Payload?.Fields?.Keys?.Contains("inputs")).GetValueOrDefault(false))
            {

                ListValue inputs = request.OriginalDetectIntentRequest.Payload?.Fields?["inputs"]?.ListValue;

                if (inputs != null)
                {

                    bool isIntentFound = false;

                    int valCount = 0;
                    while (!isIntentFound && valCount < inputs.Values.Count)
                    {

                        Value val = inputs.Values[valCount];

                        if (val.StructValue?.Fields["intent"] != null)
                        {
                            string intentName = val.StructValue.Fields["intent"].StringValue;
                            isIntentFound = true;

                            if (intentName.Equals(INTENT_NOINPUT, StringComparison.OrdinalIgnoreCase))
                            {
                                isRepromptRequest = true;

                            }
                        }

                        valCount++;

                    }

                }

            }

            return isRepromptRequest;
        }


        /// <summary>
        /// Converts the WebhookRequest to a StoryRequest.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="appReader">Required to resolve the request to a title</param>
        /// <returns></returns>
        public static async Task<StoryRequest> ToStoryRequestAsync(this WebhookRequest req, IAppMappingReader appReader,
            string alias, ILogger logger)
        {
            if (appReader == null)
                throw new ArgumentNullException(nameof(appReader));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));


            StoryRequest storyReq;
            if (req.OriginalDetectIntentRequest.Source.Equals("facebook"))
            {
               
                storyReq = await GetFacebookMessengerRequestAsync(alias, req, appReader, logger);

            }
            else
            {
                storyReq = await GetGoogleAssistantRequestAsync(alias, req, appReader, logger);
            }

            return storyReq;
        }

        private static async Task<StoryRequest> GetFacebookMessengerRequestAsync(string alias, WebhookRequest req, IAppMappingReader appReader, ILogger logger)
        {
            StoryRequest storyReq = InitializeRequest(alias, req);

            storyReq.Client = Client.FacebookMessenger;

            string[] rawSession = req.Session.Split("/");
            string session = rawSession[4];
            storyReq.SessionId = session;

            storyReq.IsNewSession = req.QueryResult.Action.Equals("input.welcome", StringComparison.OrdinalIgnoreCase);
            storyReq.IsGuest = false;
            storyReq.IsPingRequest = false;

            if (storyReq.IsNewSession.GetValueOrDefault(false))
            {

                // If the query text is FACEBOOK_WELCOME, then disregard the intent text.
                // The user has not communicated an intent despite the inbound message.
                if(req.QueryResult.QueryText.Equals("FACEBOOK_WELCOME"))
                {
                    storyReq.Intent = null;
                }

                storyReq.RequestType = StoryRequestType.Launch;

                storyReq.SessionContext = new EngineSessionContext
                {
                    EngineSessionId = Guid.NewGuid(),
                    TitleVersion = await appReader.GetTitleAsync(Client.GoogleHome, storyReq.ApplicationId, alias)
                };

                logger.LogInformation("Starting new Facebook Messenger session");
            }
            else
            {
                storyReq.RequestType = RequestReaderUtilities.GetRequestType(storyReq.Intent);
                storyReq.SessionContext = GetStoredEngineContext(req);
            }


            storyReq.Slots = GetSlots(req);

            var payload = req.OriginalDetectIntentRequest.Payload;

            var dataStruct = payload?.Fields?["data"];

            var senderStruct = dataStruct?.StructValue?.Fields?["sender"];

            storyReq.UserId = senderStruct?.StructValue?.Fields?["id"]?.StringValue;


            return storyReq;
        }



        private static async Task<StoryRequest> GetGoogleAssistantRequestAsync(string alias, WebhookRequest req, IAppMappingReader appReader, ILogger logger)
        {

            StoryRequest storyReq = InitializeRequest(alias, req);

            storyReq.Client = Client.GoogleHome;

            Struct intentRequestPayload = req.OriginalDetectIntentRequest?.Payload;

            var conversationStruct = intentRequestPayload?.Fields?["conversation"];

            string convType = conversationStruct?.StructValue?.Fields?["type"]?.StringValue;

            storyReq.IsNewSession = convType != null && convType.Equals("NEW", StringComparison.OrdinalIgnoreCase);

            // Use the conversationid
            storyReq.SessionId = conversationStruct?.StructValue?.Fields?["conversationId"]?.StringValue;


            storyReq.RequestAttributes = new Dictionary<string, string> { { "ConversationType", convType } };

            var userStruct = intentRequestPayload?.Fields?["user"];

            if (storyReq.IsNewSession.GetValueOrDefault(false))
            {
                storyReq.RequestType = StoryRequestType.Launch;

                storyReq.SessionContext = new EngineSessionContext
                {
                    EngineSessionId = Guid.NewGuid(),
                    TitleVersion = await appReader.GetTitleAsync(Client.GoogleHome, storyReq.ApplicationId, alias)
                };


                if ((intentRequestPayload?.Fields?.Keys?.Contains("surface")).GetValueOrDefault(false))
                {

                    Struct surface = intentRequestPayload.Fields?["surface"].StructValue;
                    List<string> capabilityNames = new List<string>();
                    if (surface != null)
                    {
                        if ((surface.Fields?.Keys?.Contains("capabilities")).GetValueOrDefault(false))
                        {
                            ListValue capList = surface.Fields?["capabilities"]?.ListValue;

                            if (capList != null)
                            {
                                foreach (var capVal in capList.Values)
                                {
                                    if (capVal.StructValue != null)
                                    {
                                        Struct capStruct = capVal.StructValue;
                                        if ((capStruct.Fields?.Keys?.Contains("name")).GetValueOrDefault(false))
                                        {
                                            string nameVal = capStruct.Fields?["name"].StringValue;
                                            capabilityNames.Add(nameVal);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    string capString = string.Join(",", capabilityNames);

                    if (!string.IsNullOrWhiteSpace(capString))
                    {
                        if (storyReq.SessionAttributes == null)
                            storyReq.SessionAttributes = new Dictionary<string, string>();

                        storyReq.SessionAttributes.Add("DeviceCapabilities", capString);
                    }
                }

            }
            else
            {

                // Get the session context from request.
                if (req.QueryResult != null)
                {
                    storyReq.SessionContext = GetStoredEngineContext(req);
                }


                if (req.IsRepromptRequest())
                {
                    storyReq.RequestType = StoryRequestType.Reprompt;
                }
                else
                {
                    storyReq.Intent = req.QueryResult.Intent.DisplayName;
                    storyReq.RequestType = RequestReaderUtilities.GetRequestType(storyReq.Intent);
                }

                if (storyReq.SessionContext == null)
                {
                    logger.LogInformation("Not a new session, but the session context is not in request.");
                    storyReq.SessionContext = new EngineSessionContext
                    {
                        EngineSessionId = Guid.NewGuid(),
                        TitleVersion =
                            await appReader.GetTitleAsync(Client.GoogleHome, storyReq.ApplicationId, alias)
                    };


                }

            }

            storyReq.Slots = GetSlots(req);

            storyReq.IsPingRequest = false;

            // Determine if this is a health check. If it is, then set IsPingRequest to true
            // so that the message is not logged to the database. 
            if (req.OriginalDetectIntentRequest != null)
            {
                string origIntentText = req.OriginalDetectIntentRequest.Payload.ToString();
                GoogleLegacyRequest healthCheck = JsonConvert.DeserializeObject<GoogleLegacyRequest>(origIntentText);

                storyReq.IsPingRequest = healthCheck?.IsHealthRequest();

                string requestType = healthCheck?.RequestType;

                if (!string.IsNullOrWhiteSpace(requestType))
                    storyReq.RequestAttributes.Add("RequestType", requestType);


                RawInput rawInput = healthCheck?.Inputs?.FirstOrDefault()?.RawInputs?.FirstOrDefault();

                if (rawInput != null)
                {
                    storyReq.RawText = rawInput.Query;

                    if (!string.IsNullOrWhiteSpace(rawInput.InputType))
                    {
                        string inputTypeText = rawInput.InputType;
                        storyReq.RequestAttributes.Add("InputType", inputTypeText);


                        if (inputTypeText.Equals("VOICE"))
                            storyReq.InputType = UserInputType.Voice;
                        else if (inputTypeText.Equals("KEYBOARD"))
                            storyReq.InputType = UserInputType.Keyboard;
                        else if (inputTypeText.Equals("TOUCH"))
                            storyReq.InputType = UserInputType.Touch;
                        else
                            storyReq.InputType = UserInputType.Other;
                    }
                    else
                        storyReq.InputType = UserInputType.Unknown;
                }
            }

            storyReq.IsGuest = true;
            if ((userStruct?.StructValue?.Fields?.ContainsKey("userVerificationStatus")).GetValueOrDefault(false))
            {

                string userVerificationStatus =
                    userStruct?.StructValue?.Fields?["userVerificationStatus"].StringValue;

                if (!string.IsNullOrWhiteSpace(userVerificationStatus)
                    && userVerificationStatus.Equals("VERIFIED", StringComparison.OrdinalIgnoreCase))
                {
                    // If the user is verified and this is not a new session, then 
                    // the user storage should be present.
                    if ((userStruct?.StructValue?.Fields?.Keys.Contains("userStorage")).GetValueOrDefault(false))
                    {
                        string userStorageText = userStruct?.StructValue?.Fields?["userStorage"]?.StringValue;
                        if (!string.IsNullOrWhiteSpace(userStorageText))
                        {
                            UserStorage userStore = JsonConvert.DeserializeObject<UserStorage>(userStorageText);
                            storyReq.UserId = userStore.UserId;
                        }
                    }

                    storyReq.IsGuest = false;
                }
            }

            // Do not pass intent if this is a new session.
            if (storyReq.IsNewSession.GetValueOrDefault(false))
            {
                storyReq.Intent = null;
            }

            if (storyReq.IsGuest.GetValueOrDefault(false))
                storyReq.UserId = storyReq.SessionId;
            else if (string.IsNullOrWhiteSpace(storyReq.UserId) &&
                     (storyReq.SessionContext?.EngineUserId.HasValue).GetValueOrDefault(false))
            {
                storyReq.UserId = storyReq.SessionContext.EngineUserId.Value.ToString();
            }

            if (storyReq.IsPingRequest.GetValueOrDefault(false))
                logger.LogInformation("Request is a HealthCheck ping request");

            return storyReq;
        }

        private static Dictionary<string, string> GetSlots(WebhookRequest req)
        {
            Dictionary<string, string> slotResult = new Dictionary<string, string>();

            var slotParams = req.QueryResult.Parameters;

            if (slotParams?.Fields?.Keys?.Count > 0)
            {
                foreach (string slotKey in slotParams.Fields.Keys)
                {
                    string slotVal = slotParams.Fields[slotKey].StringValue;
                    slotResult.Add(slotKey, slotVal);
                }

            }

            return slotResult;
        }

        private static EngineSessionContext GetStoredEngineContext(WebhookRequest dialogFlowReq)
        {
            EngineSessionContext retContext = null;

            RepeatedField<Context> contexts = dialogFlowReq.QueryResult.OutputContexts;

            string contextName = GetContextName(dialogFlowReq.Session);

            Context foundContext = contexts.FirstOrDefault(x => x.Name.Equals(contextName));

            string engineContextText = foundContext?.Parameters?.Fields?["enginecontext"]?.StringValue;

            if (engineContextText != null)
            {
                EngineSessionContext engineSession =
                    JsonConvert.DeserializeObject<EngineSessionContext>(engineContextText);
                retContext = engineSession;
            }


            return retContext;
        }

        private static StoryRequest InitializeRequest(string alias, WebhookRequest req)
        {

            string session = req.Session;
            // The DialogFlow agent is embedded in the session token. 
            // Parse the session token to obtain the dialog flow agent identifier which
            // is then used to look up the story in the appmappings.yaml file.

            // An alternative is to pull this from a preconfigured title identifier in the 
            // query string of the Webhook URL entered in the Fulfillment Webhook URL 
            // in the DialogFlow Agent configuration.
            string[] sessionParts = session.Split('/');

            StoryRequest storyReq = new StoryRequest
            {
                Alias = alias,
                EngineRequestId = Guid.NewGuid(),
                ApplicationId = sessionParts[1],
                RequestId = req.ResponseId,
                RequestTime = DateTime.UtcNow,
                Locale = req.QueryResult.LanguageCode,
                Intent = req.QueryResult.Intent?.DisplayName,
                IntentConfidence = req.QueryResult.IntentDetectionConfidence
            };

            return storyReq;
        }


        /// <summary>
        /// Returns the name of the context to retrieve from the DialogFlow Webhook request.
        /// </summary>
        /// <param name="clientSessionId">Full client session id string from the webhook request</param>
        /// <returns></returns>
        private static string GetContextName(string clientSessionId)
        {
            return string.Concat(clientSessionId, "/contexts/", SessionContextName);
        }




    }
}
