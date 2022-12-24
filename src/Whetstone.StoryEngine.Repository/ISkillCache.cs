using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Repository
{
    public interface ISkillCache
    {

        Task<T> GetCacheValueAsync<T>(string skillId, string cacheItemName);

        Task SetCacheValueAsync<T>(string skillId, string cacheItemName, T cacheItem);

    }
}
