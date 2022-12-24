using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data
{
    public interface IFileReader
    {

        Task<StoryTitle> GetTitleContentsAsync(TitleVersion titleVersion);


    }
}
