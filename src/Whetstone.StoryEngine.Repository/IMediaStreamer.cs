using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Repository
{
    public interface IMediaStreamer
    {

        Task<SimpleMediaResponse> GetFileStreamAsync(string environment, TitleVersion titleVer, string fileName, bool isFileEncrypted = true);

    }
}
