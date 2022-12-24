using System.Collections.Generic;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Repository
{
    public interface ITitleValidator
    {

        Task<StoryValidationResult> ValidateTitleAsync(TitleVersion titleVersion);

        Task<StoryValidationResult> ValidateTitleAsync(StoryTitle title);


        Task<List<NodeMapItem>> GetNodeMapAsync(TitleVersion titleVersion);

        Task<List<NodeMapItem>> GetNodeRouteAsync(TitleVersion titleVersion, string sourceNode, string destNode);

    }
}
