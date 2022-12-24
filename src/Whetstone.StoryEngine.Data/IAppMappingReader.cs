using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data
{
    public interface IAppMappingReader
    {


        /// <summary>
        /// Given the app identifier for a skill, this retrieves the name of the title mapped to the skill.
        /// </summary>
        /// <param name="appId">For Alexa, this is the skill id. For Google Home or Invoke, it's that platform's skill.</param>
        /// <returns>Name of the title to play</returns>
        Task<TitleVersion> GetTitleAsync(Client clientType, string appId, string alias);

    }


}
