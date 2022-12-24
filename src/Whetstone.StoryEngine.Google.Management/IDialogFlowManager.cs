using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Google.Management
{
    public interface IDialogFlowManager
    {


        /// <summary>
        /// Imports the title.
        /// </summary>
        /// <param name="projectId">The Google project identifier.</param>
        /// <param name="title">The story title definition</param>
        Task ImportTitleAsync(string projectId, StoryTitle title);


        /// <summary>
        /// This creates a zip file that can be imported into a DialogFlow project.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="languages"></param>
        /// <returns></returns>
        Task<byte[]> ExportTitleNlpAsync(StoryTitle title, string[] languages);

    }
}
