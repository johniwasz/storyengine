using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Data
{
    public interface IStoryTitleImporter
    {


        Task ImportFromZipAsync(byte[] importZip);

    }
}
