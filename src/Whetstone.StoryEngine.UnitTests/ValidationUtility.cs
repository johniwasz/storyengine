using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.UnitTests;
using Xunit;

namespace Whetstone.UnitTests
{
    internal static class ValidationUtility
    {

        internal static async Task<StoryResponse> ValidateNode(ExpectedNodeResult expectedResult, IStoryRequestProcessor storyRepProc, StoryRequest request,
         IStoryUserRepository userRepo, IMediaLinker linker, ISessionLogger sessLogger, bool forceContinueExpected = true)
        {
            StoryResponse resp;
            DataTitleClientUser curUser;
            resp = await storyRepProc.ProcessStoryRequestAsync(request);
            curUser = await userRepo.GetUserAsync(request);
            Assert.True(curUser.CurrentNodeName.Equals(expectedResult.NodeName),
                $"Expected node {expectedResult.NodeName}. Found {curUser.CurrentNodeName}");

            if (expectedResult.HasCardResponse || expectedResult.HasCardButtons || expectedResult.HasCardText)
                Assert.True(resp.LocalizedResponse.CardResponse != null, $"Expecting a card response. No card in node {expectedResult.NodeName}");
            else
                Assert.True(resp.LocalizedResponse.CardResponse == null, $"Expecting no card response. Card found in node {expectedResult.NodeName}");

            if (expectedResult.HasCardButtons)
            {
                Assert.True((resp.LocalizedResponse.CardResponse.Buttons?.Any()).GetValueOrDefault(false), $"Expected buttons on card response on node {expectedResult.NodeName}");
            }

            if (expectedResult.HasCardText)
            {
                Assert.True((resp.LocalizedResponse.CardResponse.Text?.Any()).GetValueOrDefault(false), $"Expecting card text. No card text in node {expectedResult.NodeName}");
            }


            if (forceContinueExpected)
            {
                Assert.True(resp.ForceContinueSession, $"Force continue session on {expectedResult.NodeName} should be true");

                if (!(resp.LocalizedResponse.RepromptSpeechResponses?.Any()).GetValueOrDefault(false))
                {
                    // if the speech responses are empty, then the text reprompts should not be
                    Assert.True((resp.LocalizedResponse.RepromptTextResponses?.Any()).GetValueOrDefault(false),
                        $"The node {expectedResult.NodeName} is missing both RepromptSpeechResponses and RepromptTextResponses");

                }
            }
            else
            {
                Assert.False(resp.ForceContinueSession, $"Force continue session on {expectedResult.NodeName} should be false");
            }

            Assert.True(string.IsNullOrWhiteSpace(resp.EngineErrorText), $"Engine error found: {resp.EngineErrorText}");
            await ResponseHelper.WriteResponseAsync(request, resp, linker, sessLogger);

            return resp;
        }
    }
}
