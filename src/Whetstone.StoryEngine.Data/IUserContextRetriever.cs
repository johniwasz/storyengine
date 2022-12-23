using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.Data
{
    public interface IUserContextRetriever
    {

      //  Task<DbContextOptions<UserDataContext>> GetContextOptionsAsync();


        Task<IUserDataContext> GetUserDataContextAsync();

        Task<string> GetConnectionStringAsync();

    }
}
