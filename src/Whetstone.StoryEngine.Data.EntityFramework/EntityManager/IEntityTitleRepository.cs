using System.Collections.Generic;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Conditions;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data.EntityFramework.EntityManager
{
    public interface IEntityTitleRepository
    {
        Task AddLanguageMappingsAsync(List<DataIntentSlotMapping> mappings);
        Task CreateTitleAsync(DataTitle entity);
        void DeleteTitleAsync(DataTitle entity);
        Task<StoryNode> GetBadIntentNodeAsync(string titleId, int badIntentCount);
        Task<DataTitle> GetByShortNameAsync(string titleId);

        Task<List<DataTitle>> GetTitles();


      //  Task<DataStory> GetTitleByIdAsync(long titleId);

        Task<DataTitleVersion> GetCurrentTitleVersionAsync(string shortName, string version);
        Task<StoryNode> GetNodeByNameAsync(string titleId, string storyNodeName);
        Task<ICollection<StoryNode>> GetNodesByTitleAsync(string shortName);
        Task<StoryConditionBase> GetStoryConditionAsync(string shortName, string conditionName);
        void UpdateTitle(StoryTitle entity);
        Task UpdateTitleAsync(StoryTitle entity);

        Task PurgeVersionAsync(long versionId);

        Task<DataTitleVersion> UpdateOrCreateVersionAsync(string shortName, string version, DataTitleVersion versionModel);
    }
}