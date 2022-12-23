using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data
{
    public interface IStoryNodeRepository
    {
        Task CreateAsync(List<StoryNode> nodes);

        Task CreateAsync(StoryNode nodes);


        void SaveNode(StoryNode foundNode);


        Task<StoryNode> GetByIdAsync(string id);


        Task<List<StoryNode>> GetNodesByTitleAsync(string titleId);

        Task<StoryNode> GetNodeByNameAsync(string titleId, string storyNodeName);
    }
}
