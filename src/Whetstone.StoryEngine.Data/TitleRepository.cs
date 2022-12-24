using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Conditions;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data
{
    public class TitleRepository : ITitleRepository
    {
        private readonly ITitleReader _titleReader;

        public TitleRepository(ITitleReader titleReader)
        {
            _titleReader = titleReader;

        }


        public async Task ClearTitleAsync(TitleVersion titleVersion)
        {
            await _titleReader.ClearTitleAsync(titleVersion);
        }

        public Task<StoryTitle> CreateOrUpdateTitleAsync(StoryTitle entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteTitleAsync(StoryTitle entity)
        {
            throw new NotImplementedException();
        }

        public Task<List<TitleRoot>> GetAllTitleDeploymentsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<StoryNode> GetBadIntentNodeAsync(TitleVersion titleVersion, int badIntentCount)
        {
            return await _titleReader.GetBadIntentNodeAsync(titleVersion, badIntentCount);
        }

        public async Task<StoryTitle> GetByIdAsync(TitleVersion titleVersion)
        {
            return await _titleReader.GetByIdAsync(titleVersion);
        }

        public async Task<StoryNode> GetErrorNodeAsync(TitleVersion titleVersion)
        {
            return await _titleReader.GetErrorNodeAsync(titleVersion);
        }

        public async Task<Intent> GetIntentByNameAsync(TitleVersion titleVersion, string intentName)
        {
            return await _titleReader.GetIntentByNameAsync(titleVersion, intentName);
        }

        public async Task<List<Intent>> GetIntentsAsync(TitleVersion titleVersion)
        {
            return await _titleReader.GetIntentsAsync(titleVersion);
        }

        public async Task<StoryNode> GetNodeByNameAsync(TitleVersion titleVersion, string storyNodeName)
        {
            return await _titleReader.GetNodeByNameAsync(titleVersion, storyNodeName);
        }

        public async Task<ICollection<StoryNode>> GetNodesByTitleAsync(TitleVersion titleVersion)
        {
            return await _titleReader.GetNodesByTitleAsync(titleVersion);
        }

        public async Task<StoryPhoneInfo> GetPhoneInfoAsync(TitleVersion titleVersion)
        {
            return await _titleReader.GetPhoneInfoAsync(titleVersion);
        }

        public async Task<List<SlotType>> GetSlotTypes(TitleVersion titleVersion)
        {
            return await _titleReader.GetSlotTypes(titleVersion);
        }

        public async Task<string> GetStartNodeNameAsync(TitleVersion titleVersion, bool isNew)
        {
            return await _titleReader.GetStartNodeNameAsync(titleVersion, isNew);
        }

        public async Task<StoryConditionBase> GetStoryConditionAsync(TitleVersion titleVersion, string conditionName)
        {
            return await _titleReader.GetStoryConditionAsync(titleVersion, conditionName);
        }

        public async Task<List<StoryConditionBase>> GetStoryConditionsAsync(TitleVersion titleVersion)
        {
            return await _titleReader.GetStoryConditionsAsync(titleVersion);
        }

        public async Task<StoryType> GetStoryTypeAsync(TitleVersion titleVersion)
        {
            return await _titleReader.GetStoryTypeAsync(titleVersion);
        }

        public async Task<bool> IsPrivacyLoggingEnabledAsync(TitleVersion titleVersion)
        {
            return await _titleReader.IsPrivacyLoggingEnabledAsync(titleVersion);
        }
    }
}
