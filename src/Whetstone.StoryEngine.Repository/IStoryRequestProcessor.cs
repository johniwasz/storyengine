using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Repository
{
    public interface IStoryRequestProcessor
    {

        Task<StoryResponse> ProcessStoryRequestAsync(StoryRequest request);

        /// <summary>
        /// This is for Alexa-specific support to handle CanFullIntent requests for nameless invocation.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<CanFulfillResponse> CanFulfillIntentAsync(StoryRequest request);


        //  Task<string> GetTitleIdAsync(string applicationId);

        Task<StoryPhoneInfo> GetPhoneInfoAsync(TitleVersion titleVersion);

    }
}
