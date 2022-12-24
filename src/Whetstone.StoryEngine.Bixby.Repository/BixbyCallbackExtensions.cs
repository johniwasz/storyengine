using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Bixby.Repository.Models;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Story.Cards;
using Whetstone.StoryEngine.Repository;

namespace Whetstone.StoryEngine.Bixby.Repository
{
    public static class BixbyCallbackExtensions
    {
        //private static readonly string SessionContextName = "enginecontext";

        // private static readonly ILogger _logger = StoryEngineLogFactory.CreateLogger();

        //    private static readonly string AUDIO_CAPABILITY = "actions.capability.AUDIO_OUTPUT";

        public static readonly string SCREEN_CAPABILITY = "actions.capability.SCREEN_OUTPUT";

        public static readonly string WEBBROWSER_CAPABILITY = "actions.capability.WEB_BROWSER";

        public static readonly string MEDIAPLAYER_CAPABILITY = "actions.capability.MEDIA_RESPONSE_AUDIO";

        /// <summary>
        /// Converts the BixbyRequest_V1 to a StoryRequest.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="appReader">Required to resolve the request to a title</param>
        /// <returns></returns>
        public static async Task<StoryRequest> ToStoryRequestAsync(this BixbyRequest_V1 req, IAppMappingReader appReader,
            string alias)
        {

            StoryRequest storyReq = new StoryRequest();

            if (String.IsNullOrEmpty(req.Context.SessionId))
                throw new InvalidOperationException("Bixby request sessionId cannot be empty.");

            // Grab the session id and application id from Bixby - we do not actually know if this is a new session
            storyReq.ApplicationId = req.ApplicationId;
            storyReq.SessionId = req.Context.SessionId;
            storyReq.IsNewSession = req.NewSession;

            // There is no concept of a request Id from Bixby so just reuse the Engine Request Id
            storyReq.EngineRequestId = Guid.NewGuid();
            storyReq.RequestId = storyReq.EngineRequestId.ToString();

            // If we have a bixbyUserId use that, if not use the session id. Getting a bixbyUserId requires requesting permissions from the user, which
            // just doesn't make a whole lot of sense. If we have a true UserId, we will assume the user is not a guest, otherwise they are.
            if (String.IsNullOrEmpty(req.Context.BixbyUserId))
            {
                storyReq.UserId = req.Context.SessionId;
                storyReq.IsGuest = true;
            }
            else
            {
                storyReq.UserId = req.Context.BixbyUserId;
                storyReq.IsGuest = false;
            }

            // Client type, request time
            storyReq.Client = Client.Bixby;
            storyReq.RequestTime = DateTime.UtcNow;
            storyReq.Locale = req.Context.Locale;

            // Grab the intent and slots from the request. There is no concept of a new conversation from Bixby, we just know that they sent us an intent.
            storyReq.RequestType = StoryRequestType.Intent;
            storyReq.Intent = req.Intent;

            if (req.Slots != null && req.Slots.Length > 0)
            {
                storyReq.Slots = new Dictionary<string, string>();
                foreach (Slot slot in req.Slots)
                {
                    storyReq.Slots.Add(slot.Name, slot.SlotValue);
                }

            }

            // I *think* I need to set this up each time?
            storyReq.IsPingRequest = false;
            storyReq.SessionContext = new EngineSessionContext
            {
                EngineSessionId = Guid.NewGuid(),
                TitleVersion = await appReader.GetTitleAsync(Client.Bixby, storyReq.ApplicationId, alias)
            };

            return storyReq;

        }

        /// <summary>
        /// Converts the story response to a Bixby response.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static BixbyCallbackResponse_V1 ToBixbyCallbackResponse(this StoryResponse response, IMediaLinker mediaLinker, ILogger responseLogger)
        {

            if (mediaLinker == null)
                throw new ArgumentException($"{nameof(mediaLinker)} is null");

            if (responseLogger == null)
                throw new ArgumentException($"{nameof(responseLogger)} is null");

            if (response == null)
                throw new ArgumentException($"{nameof(response)} is null");

            if (response.LocalizedResponse == null)
                throw new ArgumentException($"{nameof(response.LocalizedResponse)} has an empty response. This is an invalid conditionBase.");


            BixbyCallbackResponse_V1 webResp = new BixbyCallbackResponse_V1
            {
                NodeName = response.NodeName
            };

            var localizedResponse = response.LocalizedResponse;

            // Get a bixby response and if it does not exist, then get the default client response
            var speechResponse = localizedResponse.SpeechResponses;

            string generatedText = localizedResponse.GeneratedTextResponse?.CleanText();

            // speechResponse = null;
            if (speechResponse != null)
            {
                webResp.Dlg = speechResponse?.ToSsml(mediaLinker,
                        response?.SessionContext?.TitleVersion);
            }
            else if (!string.IsNullOrWhiteSpace(generatedText))
            {
                webResp.Dlg = generatedText;
            }

            //
            // We need to fix audio tags returned from the story engine to make them compatible with Bixby.
            //
            webResp.Dlg = FixAudioTags(webResp.Dlg, 0);

            var repromptResponse = localizedResponse.RepromptSpeechResponses;

            if (repromptResponse != null)
            {
                webResp.FollowUpPrompt = repromptResponse?.ToSsml(mediaLinker,
                        response.SessionContext?.TitleVersion);
            }
            else
            {
                webResp.FollowUpPrompt = string.Join(' ', localizedResponse.RepromptTextResponses);
            }

            if (!string.IsNullOrWhiteSpace(generatedText) && localizedResponse.CardResponse != null)//&&
                                                                                                    //(surfaceCaps.HasScreen || surfaceCaps.HasWebBrowser))
            {
                // We have a card
                webResp.HasCard = true;

                CardEngineResponse cardResp = localizedResponse.CardResponse;

                // If we have display dialog return it

                _ = generatedText.Replace("’", @"'")
                                                         .Replace("“", "\"")
                                                         .Replace("”", "\"")
                                                         .Replace("–", "-");

                if (cardResp.Text != null)
                    webResp.DisplayDlg = string.Join(' ', cardResp.Text);

                // If we have images, pick one and set the url
                if (!string.IsNullOrWhiteSpace(cardResp.SmallImageFile) ||
                     !string.IsNullOrWhiteSpace(cardResp.LargeImageFile))
                {
                    if (!string.IsNullOrWhiteSpace(cardResp.SmallImageFile))
                    {
                        webResp.Url = mediaLinker.GetFileLink(response.SessionContext?.TitleVersion,
                                cardResp.SmallImageFile);
                    }
                    else
                    {
                        webResp.Url = mediaLinker.GetFileLink(response.SessionContext?.TitleVersion,
                                cardResp.LargeImageFile);
                    }
                }

                webResp.CardTitle = cardResp.CardTitle;

            }

            return webResp;


        }

        private static readonly string AUDIO_TAG_PARTIAL_BEGIN = "<audio";
        private static readonly string XML_TAG_END = "/>";
        private static readonly string AUDIO_TAG_SRC_SINGLEQUOTE = "src='";
        private static readonly string AUDIO_TAG_SRC_ENDQUOTE = "'";
        private static readonly string AUDIO_TAG_BIXBY_FORMAT = "<audio src=\"{0}\"></audio>";

        //   public static string AUDIO_CAPABILITY1 => AUDIO_CAPABILITY;

        //
        // The StoryEngine returns an audio tag that is incompatible with Bixby's interpreter so we need to do some surgery
        // The current tag uses the following format: <audio src='filename' />
        // We need to change it to: <audio src="filename"></audio>
        //
        private static string FixAudioTags(string bixbyDlg, int nStartIndex)
        {
            StringBuilder sb = new StringBuilder();

            while (bixbyDlg.IndexOf(AUDIO_TAG_PARTIAL_BEGIN, nStartIndex) != -1)
            {
                // Extract the entire audio tag
                int nAudioTagStart = bixbyDlg.IndexOf(AUDIO_TAG_PARTIAL_BEGIN, nStartIndex);
                int nAudioTagEndIndex = bixbyDlg.IndexOf(XML_TAG_END, nStartIndex) + XML_TAG_END.Length;

                // Append all characters before the audio tag start
                sb.Append(bixbyDlg.Substring(nStartIndex, nAudioTagStart - nStartIndex));

                // Extract the source file
                int nSrcFileBeginIndex = bixbyDlg.IndexOf(AUDIO_TAG_SRC_SINGLEQUOTE, nAudioTagStart) + AUDIO_TAG_SRC_SINGLEQUOTE.Length;
                int nSrcFileEndQuoteIndex = bixbyDlg.IndexOf(AUDIO_TAG_SRC_ENDQUOTE, nSrcFileBeginIndex);

                string audioTagSourceFile = bixbyDlg.Substring(nSrcFileBeginIndex, nSrcFileEndQuoteIndex - nSrcFileBeginIndex);

                sb.AppendFormat(AUDIO_TAG_BIXBY_FORMAT, audioTagSourceFile);

                // Next loop iteration starts after *this* audio tag.
                nStartIndex = nAudioTagEndIndex;

            }

            sb.Append(bixbyDlg.Substring(nStartIndex));

            return sb.ToString();
        }


    }
}
