using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Data;

namespace Whetstone.StoryEngine.Data
{
    public interface IStoryUserRepository
    {


        Task<DataTitleClientUser> GetUserAsync(StoryRequest request);


        /// <summary>
        /// This creates a new user object without saving the user. User this method when a user connects to the system for the first time.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A populated StoryUser class.</returns>
        DataTitleClientUser BootstrapUser(StoryRequest request);

        Task SaveUserAsync(DataTitleClientUser user);




    }
}
