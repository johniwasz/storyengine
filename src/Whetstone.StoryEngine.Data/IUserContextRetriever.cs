using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Data
{
    public interface IUserContextRetriever
    {

        //  Task<DbContextOptions<UserDataContext>> GetContextOptionsAsync();


        Task<IUserDataContext> GetUserDataContextAsync();

        Task<string> GetConnectionStringAsync();

    }
}
