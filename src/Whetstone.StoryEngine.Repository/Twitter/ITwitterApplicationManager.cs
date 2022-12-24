using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Admin;

namespace Whetstone.StoryEngine.Repository.Twitter
{
    public interface ITwitterApplicationManager
    {

        Task<AddTwitterApplicationResponse> AddTwitterApplicationAsync(AddTwitterApplicationRequest request);

    }
}
