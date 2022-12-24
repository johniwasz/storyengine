using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Repository
{


    /// <summary>
    /// Interface is responsible for creating a fully qualified URL to an image or mp3 file for use by Alexa.
    /// </summary>
    public interface IMediaLinker
    {

        string GetFileLink(TitleVersion titleVer, string fileName);
    }
}
