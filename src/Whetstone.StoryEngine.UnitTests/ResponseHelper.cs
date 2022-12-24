using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.StoryEngine;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Story.Cards;
using Whetstone.StoryEngine.Repository;

namespace Whetstone.UnitTests
{
    internal static class ResponseHelper
    {


        internal static async Task WriteResponseAsync(StoryRequest req, CanFulfillResponse resp, ISessionLogger sessionLogger)
        {
            if (resp == null)
                throw new ArgumentException("response is null");


            await sessionLogger.LogRequestAsync(req, resp);

            string intent = req.Intent;
            string canFulfill = resp.CanFulfill.ToString();

            Debug.WriteLine($"Can fulfill {intent}: {canFulfill}");


            Debug.WriteLine("------------------------------------------");

        }


        internal static async Task WriteResponseAsync(StoryRequest req, StoryResponse resp, IMediaLinker mediaLinker, ISessionLogger sessionLogger)
        {
            if (resp == null)
                throw new ArgumentException("response is null");

            if (resp.LocalizedResponse == null)
                throw new ArgumentException("StoryResponse has an empty response. This is an invalid conditionBase.");


            await sessionLogger.LogRequestAsync(req, resp);

            var localizedResponse = resp.LocalizedResponse;

            // Get an alexa response and if it does not exist, then get the default client response
            var speechResponse = localizedResponse.SpeechResponses;

            string generatedText = localizedResponse.GeneratedTextResponse.CleanText();

            TitleVersion titleVer = new TitleVersion(resp.TitleId, resp.TitleVersion);

            if (speechResponse != null)
            {

                string ssmlResponse = speechResponse?.ToSsml(mediaLinker, titleVer);

                Debug.WriteLine("SSML Response: ");
                Debug.WriteLine(ssmlResponse);
            }
            else if (!string.IsNullOrWhiteSpace(generatedText))
            {

                Debug.WriteLine("Text Response: ");
                Debug.WriteLine(generatedText);
            }



            var repromptResponse =
                localizedResponse.RepromptSpeechResponses;

            if (repromptResponse == null)
                repromptResponse = localizedResponse.RepromptSpeechResponses;

            if (repromptResponse != null)
            {
                string ssml =
                    repromptResponse?.ToSsml(mediaLinker, titleVer);

                Debug.WriteLine("SSML Reprompt: ");
                Debug.WriteLine(ssml);

            }
            else
            {
                Debug.WriteLine("Text Reprompt: ");
                Debug.WriteLine(localizedResponse.RepromptTextResponses);

            }

            if (localizedResponse.CardResponse != null)
            {

                CardEngineResponse cardResp = localizedResponse.CardResponse;

                string cardTitle = string.IsNullOrWhiteSpace(cardResp.CardTitle) ? "Default title" : cardResp.CardTitle;

                Debug.WriteLine($"Card Title: {cardTitle}");

                if (cardResp.Text != null)
                {
                    string cardText = string.Join(' ', cardResp.Text);

                    Debug.WriteLine($"Card Text: {cardText}");
                }

                if (!string.IsNullOrWhiteSpace(cardResp.SmallImageFile) ||
                     !string.IsNullOrWhiteSpace(cardResp.LargeImageFile))
                {


                    if (!string.IsNullOrWhiteSpace(cardResp.SmallImageFile))
                    {
                        string smallUrl = mediaLinker.GetFileLink(titleVer, cardResp.SmallImageFile);

                        Debug.WriteLine($"Card small image: {smallUrl}");
                    }


                    if (!string.IsNullOrWhiteSpace(cardResp.LargeImageFile))
                    {
                        string largeUrl = mediaLinker.GetFileLink(titleVer, cardResp.LargeImageFile);

                        Debug.WriteLine($"Card large image: {largeUrl}");
                    }
                }
            }


            if ((resp.Suggestions?.Any()).GetValueOrDefault(false))
            {
                Debug.WriteLine("Suggestions:");

                foreach (var choice in resp.Suggestions)
                {

                    Debug.WriteLine($"Suggestion: {choice}");
                }
            }



            if (resp.ForceContinueSession)
            {
                Debug.WriteLine("Session Continues");
            }
            else
                Debug.WriteLine("Session Ends");

            Debug.WriteLine("------------------------------------------");

        }

    }
}
