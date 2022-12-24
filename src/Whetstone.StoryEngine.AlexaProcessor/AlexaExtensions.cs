using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Whetstone.Alexa;
using Whetstone.Alexa.CanFulfill;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Story.Cards;
using Whetstone.StoryEngine.Repository;

namespace Whetstone.StoryEngine.AlexaProcessor
{
    public static class AlexaExtensions
    {

        private static readonly string SessionAttribKey = "engineState";

        public static StoryRequest ToStoryRequest(this AlexaRequest alexaReq)
        {

            StoryRequest result = new StoryRequest
            {
                Locale = alexaReq.Request?.Locale,

                // This is the request Id to pass around for audit tracking
                EngineRequestId = Guid.NewGuid(),
                ApplicationId = alexaReq.Session?.Application?.ApplicationId,
                RequestId = alexaReq.Request?.RequestId,
                SessionId = alexaReq.Session?.SessionId
            };
            if (string.IsNullOrWhiteSpace(result.SessionId))
            {
                string fauxSessionId = $"storyenginesessionid.{Guid.NewGuid()}";
                result.SessionId = fauxSessionId;
            }

            bool isNewSession = (alexaReq.Session?.New).GetValueOrDefault(false);

            // If the PersonId is provided, then it is the defacto user id.
            // Otherwise, the take the userId, like originally.
            string alexaUserId = alexaReq?.Context?.System?.Person?.PersonId;

            if (string.IsNullOrWhiteSpace(alexaUserId))
                alexaUserId = alexaReq.Session?.User?.UserId;


            result.UserId = alexaUserId;
            result.IsNewSession = isNewSession;
            result.Client = Client.Alexa;
            result.Intent = alexaReq.Request?.Intent?.Name;
            result.RequestTime = (alexaReq.Request?.Timestamp).GetValueOrDefault(default);

            if (alexaReq.Context?.System != null)
            {
                result.SecurityInfo = new Dictionary<string, string>
                {
                    { "apiAccessTokey", alexaReq.Context.System.ApiAccessToken },

                    { "apiEndpoint", alexaReq.Context.System.ApiEndpoint }
                };

                if (alexaReq.Context.System.Device != null)
                {
                    result.SecurityInfo.Add("deviceId", alexaReq.Context.System.Device.DeviceId);
                }
            }

            // Get the session attributes and load it into the story request
            Dictionary<string, dynamic> sessionAttribs = alexaReq.Session?.Attributes;

            // Fixed bug #140 - Session Attribute Validation Fails When Processing Test Alexa Certification Messages
            // This will check the session attributes irrespective of whether the value is null
            // or contains an empty dictionary.
            if ((sessionAttribs?.Keys?.Count).GetValueOrDefault(0) > 0)
            {

                if (!isNewSession)
                {

                    if (sessionAttribs.ContainsKey(SessionAttribKey))
                    {
                        dynamic engineDyna = sessionAttribs[SessionAttribKey];

                        if (engineDyna is JObject jcontext)
                        {
                            // Copy to a static Album instance
                            result.SessionContext = jcontext.ToObject<EngineSessionContext>();
                        }
                        else if (engineDyna is EngineSessionContext dyna)
                        {
                            result.SessionContext = dyna;
                        }

                    }

                    if (result.SessionContext == null)
                        throw new Exception($"SessionContext expected and not found in {sessionAttribs}");
                }

            }
            else
            {
                // This is a new session. Assign a new session Id.
                result.SessionContext = new EngineSessionContext { EngineSessionId = Guid.NewGuid() };
            }


            // Push any slots
            if (alexaReq.Request?.Intent?.Slots != null)
            {
                result.Slots = new Dictionary<string, string>();
                foreach (SlotAttributes slot in alexaReq.Request.Intent.Slots)
                {
                    result.Slots.Add(slot.Name, slot.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(result.Intent))
            {
                string appIntent = IntentNameMap.GetAppIntent(result.Intent);
                if (!string.IsNullOrWhiteSpace(appIntent))
                    result.Intent = appIntent;
            }

            result.IsPingRequest = alexaReq.Version.Equals("ping", StringComparison.OrdinalIgnoreCase);

            switch ((alexaReq.Request?.Type).GetValueOrDefault(RequestType.LaunchRequest))
            {

                case RequestType.LaunchRequest:
                    result.RequestType = StoryRequestType.Launch;
                    break;
                case RequestType.CanFulfillIntentRequest:
                    result.RequestType = StoryRequestType.CanFulfillIntent;
                    break;
                case RequestType.SessionEndedRequest:
                    result.RequestType = StoryRequestType.Stop;
                    break;
                case RequestType.IntentRequest:
                    if (result.Intent.Equals("AMAZON.HelpIntent", StringComparison.OrdinalIgnoreCase))
                        result.RequestType = StoryRequestType.Help;
                    else if (result.Intent.Equals("AMAZON.StopIntent", StringComparison.OrdinalIgnoreCase) ||
                        result.Intent.Equals("AMAZON.CancelIntent", StringComparison.OrdinalIgnoreCase))
                        result.RequestType = StoryRequestType.Stop;
                    else if (result.Intent.Equals("AMAZON.PauseIntent", StringComparison.OrdinalIgnoreCase))
                        result.RequestType = StoryRequestType.Pause;
                    else if (result.Intent.Equals("AMAZON.RepeatIntent", StringComparison.OrdinalIgnoreCase))
                        result.RequestType = StoryRequestType.Repeat;
                    else if (result.Intent.Equals("AMAZON.ResumeIntent", StringComparison.OrdinalIgnoreCase))
                        result.RequestType = StoryRequestType.Resume;
                    else if (result.Intent.Equals("AMAZON.StartOverIntent", StringComparison.OrdinalIgnoreCase))
                        result.RequestType = StoryRequestType.Begin;
                    else
                    {
                        result.RequestType = RequestReaderUtilities.GetRequestType(result.Intent);
                    }
                    break;

            }

            return result;
        }





        public static CanFulfillResponseAttributes ToCanFulfillResponse(this CanFulfillResponse canFulfillResponse)
        {
            CanFulfillResponseAttributes retAttribs = new CanFulfillResponseAttributes
            {
                CanFulfill = canFulfillResponse.CanFulfill.ToCanFulFillAttribEnum()
            };

            if ((canFulfillResponse.SlotFulFillment?.Any()).GetValueOrDefault(false))
            {
                retAttribs.Slots = new List<CanFulfillSlotResponse>();
                foreach (var origSlotKey in canFulfillResponse.SlotFulFillment.Keys)
                {
                    var origSlot = canFulfillResponse.SlotFulFillment[origSlotKey];

                    CanFulfillSlotResponse slotResp = new CanFulfillSlotResponse
                    {
                        Name = origSlotKey,
                        CanFulfill = origSlot.CanFulfill.ToCanFulFillAttribEnum(),
                        CanUnderstand = origSlot.CanUnderstand.ToCanFulFillAttribEnum()
                    };
                    retAttribs.Slots.Add(slotResp);
                }
            }

            return retAttribs;
        }


        public static CanFulfillEnum ToCanFulFillAttribEnum(this YesNoMaybeEnum probEnum)
        {

            CanFulfillEnum retEnum = default;

            switch (probEnum)
            {
                case YesNoMaybeEnum.Maybe:
                    retEnum = CanFulfillEnum.Maybe;
                    break;
                case YesNoMaybeEnum.No:
                    retEnum = CanFulfillEnum.No;
                    break;
                case YesNoMaybeEnum.Yes:
                    retEnum = CanFulfillEnum.Yes;
                    break;
            }

            return retEnum;
        }


        /// <summary>
        /// Converts the story response to an Alexa response.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static AlexaResponse ToAlexaResponse(this StoryResponse response, EngineSessionContext engineContext, IMediaLinker mediaLinker, ILogger responseLogger)
        {

            AlexaResponse alexaResp = new AlexaResponse();


            if (mediaLinker == null)
                throw new ArgumentNullException(nameof(mediaLinker));

            if (responseLogger == null)
                throw new ArgumentNullException(nameof(responseLogger));

            if (engineContext == null)
                throw new ArgumentNullException(nameof(engineContext));

            if (response == null)
                throw new ArgumentNullException(nameof(response));

            // -- SendStatus the text response
            alexaResp.Response = new AlexaResponseAttributes
            {
                OutputSpeech = new OutputSpeechAttributes()
            };


            if (response.LocalizedResponse == null)
                throw new ArgumentNullException(nameof(response), "StoryResponse has an empty response. This is an invalid conditionBase.");

            var localizedResponse = response.LocalizedResponse;

            // Get an alexa response and if it does not exist, then get the default client response
            var speechResponse = localizedResponse.SpeechResponses;

            string generatedText = localizedResponse.GeneratedTextResponse?.CleanText();

            if (speechResponse != null)
            {
                alexaResp.Response.OutputSpeech.Type = OutputSpeechType.Ssml;
                alexaResp.Response.OutputSpeech.Ssml = speechResponse?.ToSsml(mediaLinker, engineContext.TitleVersion);
            }
            else if (!string.IsNullOrWhiteSpace(generatedText))
            {
                alexaResp.Response.OutputSpeech.Type = OutputSpeechType.PlainText;
                alexaResp.Response.OutputSpeech.Text = generatedText;
            }

            // TODO -- pass the session id and user id
            //engineContext.EngineSessionId = response.EngineSessionId;
            //engineContext.EngineUserId = response.UserId;

            alexaResp.SessionAttributes = new Dictionary<string, dynamic>
            {
                { SessionAttribKey, engineContext }
            };

            var repromptResponse = localizedResponse?.RepromptSpeechResponses;


            alexaResp.Response.Reprompt = new RepromptAttributes
            {
                OutputSpeech = new OutputSpeechAttributes()
            };

            if (repromptResponse != null)
            {
                alexaResp.Response.Reprompt.OutputSpeech.Type = OutputSpeechType.Ssml;
                alexaResp.Response.Reprompt.OutputSpeech.Ssml =
                    repromptResponse?.ToSsml(mediaLinker, engineContext.TitleVersion);
            }
            else
            {
                if ((localizedResponse?.RepromptTextResponses?.Any()).GetValueOrDefault(false))
                {
                    alexaResp.Response.Reprompt.OutputSpeech.Type = OutputSpeechType.PlainText;
                    alexaResp.Response.Reprompt.OutputSpeech.Text = string.Join(' ', localizedResponse.RepromptTextResponses);
                }

            }

            alexaResp.Response.ShouldEndSession = !response.ForceContinueSession;


            if (localizedResponse.CardResponse != null)
            {
                CardEngineResponse storyCard = localizedResponse.CardResponse;

                var cardResp = new CardAttributes
                {
                    Title = string.IsNullOrWhiteSpace(storyCard.CardTitle) ?
                     "Default title" :
                     storyCard.CardTitle
                };

                responseLogger.LogDebug($"Sending card response title {cardResp.Title}");

                if (!string.IsNullOrWhiteSpace(storyCard.SmallImageFile) ||
                     !string.IsNullOrWhiteSpace(storyCard.LargeImageFile))
                {
                    if (storyCard.Text != null)
                        cardResp.Text = string.Join(' ', storyCard.Text);

                    cardResp.Type = CardType.Standard;

                    cardResp.ImageAttributes = new AlexaImageAttributes();
                    if (!string.IsNullOrWhiteSpace(storyCard.SmallImageFile))
                        cardResp.ImageAttributes.SmallImageUrl = mediaLinker.GetFileLink(engineContext.TitleVersion, storyCard.SmallImageFile);

                    if (!string.IsNullOrWhiteSpace(storyCard.LargeImageFile))
                        cardResp.ImageAttributes.LargeImageUrl = mediaLinker.GetFileLink(engineContext.TitleVersion, storyCard.LargeImageFile);

                }
                else
                {
                    cardResp.Type = CardType.Simple;

                    if (!string.IsNullOrWhiteSpace(generatedText))
                        cardResp.Content = generatedText;
                }

                alexaResp.Response.Card = cardResp;

            }


            if (alexaResp.Response.ShouldEndSession == true)
            {
                // remove any reprompt logic.
                alexaResp.Response.Reprompt = null;
            }

            return alexaResp;
        }


    }
}
