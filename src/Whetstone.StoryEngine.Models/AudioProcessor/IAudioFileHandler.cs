using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Models.AudioProcessor
{
    public interface IAudioFileHandler
    {
        Task<string> ProcessAudioFile(string fileName, string filePath, string destFileName, string destFilePath);
    }
}
