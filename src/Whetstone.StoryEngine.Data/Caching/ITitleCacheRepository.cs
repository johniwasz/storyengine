using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data.Caching
{
    public interface ITitleCacheRepository
    {

        Task<StoryTitle> GetStoryTitleAsync(TitleVersion titleVersion);

        Task SetTitleVersionAsync(StoryTitle title);



        /// <summary>
        /// If isPurge is true, then it is removed from the distributed cache.
        /// </summary>
        /// <param name="titleVersion"></param>
        /// <param name="isPurge">If true, then it is removed from any distributed cache backing store.</param>
        /// <returns></returns>
        Task RemoveTitleVersionAsync(TitleVersion titleVersion, bool isPurge);

        Task SetAppMappingAsync(Client clientType, string clientAppId, TitleVersion titleVersion);

        Task<TitleVersion> GetAppMappingAsync(Client clientType, string clientAppId, string alias);


        /// <summary>
        /// If isPurge is true, then it is also removed from the distributed cache.
        /// </summary>
        /// <param name="clientType"></param>
        /// <param name="clientAppId"></param>
        /// <param name="alias"></param>
        /// <param name="isPurge"></param>
        /// <returns></returns>
        Task RemoveAppMappingAsync(Client clientType, string clientAppId, string alias, bool isPurge);



    }
}
