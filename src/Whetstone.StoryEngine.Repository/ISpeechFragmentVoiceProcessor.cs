using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Repository
{
    public interface ISpeechFragmentVoiceProcessor
    {

        Task<MemoryStream> GetPollyVoiceAsync(string environment, string shortName, List<DataSpeechFragment> fragments);


        Task<List<DataSpeechFragment>> GetUpdatedVoiceFragmentsAsync(StoryTitle origStoryTitle, StoryTitle newStoryTitle);


        Task UpdateVoicesAsync(string environment, string title, List<DataSpeechFragment> updatedVoiceFragments);


    }
}
