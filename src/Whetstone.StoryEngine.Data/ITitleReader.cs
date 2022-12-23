using Whetstone.StoryEngine.Models.Story;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Conditions;
using Whetstone.StoryEngine.Models;

namespace Whetstone.StoryEngine.Data
{
    public interface ITitleReader
    {
        // TODO - replace title version parameters with title name and version text

        Task<StoryType> GetStoryTypeAsync(TitleVersion titleVersion);

        Task<StoryTitle> GetByIdAsync(TitleVersion titleVersion);

        Task<ICollection<StoryNode>> GetNodesByTitleAsync(TitleVersion titleVersion);

        Task<StoryNode> GetNodeByNameAsync(TitleVersion titleVersion, string storyNodeName);

        Task<bool> IsPrivacyLoggingEnabledAsync(TitleVersion titleVersion);

        Task<Intent> GetIntentByNameAsync(TitleVersion titleVersion, string intentName);

        Task<List<Intent>> GetIntentsAsync(TitleVersion titleVersion);

        Task<List<SlotType>> GetSlotTypes(TitleVersion titleVersion);

        Task<StoryPhoneInfo> GetPhoneInfoAsync(TitleVersion titleVersion);

        Task<StoryConditionBase> GetStoryConditionAsync(TitleVersion titleVersion, string conditionName);

        Task<List<StoryConditionBase>> GetStoryConditionsAsync(TitleVersion titleVersion);

        Task<StoryNode> GetBadIntentNodeAsync(TitleVersion titleVersion, int badIntentCount);

        Task<StoryNode> GetErrorNodeAsync(TitleVersion titleVersion);

        Task<string> GetStartNodeNameAsync(TitleVersion titleVersion, bool isNew);

        Task ClearTitleAsync(TitleVersion titleVersion);

    }
}
