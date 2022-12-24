using System.Collections.Generic;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data
{
    public interface IIntentRepository
    {

        void AddOrUpdate(List<Intent> intents);

        // Task AddOrUpdateAsync(List<Intent> intents);


        Task CreateAsync(Intent intents);

        Task CreateAsync(List<Intent> intents);

        void Create(List<Intent> intents);

        Task<Intent> GetByIdAsync(string id);


        Task<IDictionary<long?, string>> GetIntentNamesByIdsAsync(IEnumerable<long?> intentIds);

        Task<IEnumerable<Intent>> GetByIdsAsync(List<long?> intentIds);


        List<Intent> GetIntentsByTitle(string titleId);


        List<Intent> GetGlobalIntents();
    }
}
