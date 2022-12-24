using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Cache;

namespace Whetstone.StoryEngine.Repository
{
    public class SkillCache : ISkillCache
    {

        private const string CONTAINERPREFIX = "skillcache";


        private IDistributedCache _cache;
        private readonly ILogger<SkillCache> _logger;

        public SkillCache(IDistributedCache cache, ILogger<SkillCache> logger)
        {
            _cache = cache ?? throw new ArgumentNullException($"{nameof(cache)}");

            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");
        }

        public async Task<T> GetCacheValueAsync<T>(string skillId, string cacheItemName)
        {
            if (string.IsNullOrWhiteSpace(skillId))
                throw new ArgumentException("skillId cannot be null or empty");

            if (string.IsNullOrWhiteSpace(cacheItemName))
                throw new ArgumentException("cacheItemName cannot be null or empty");



            string cacheKey = GetCacheKey(skillId, cacheItemName);

            T retItem = await _cache.GetAsync<T>(CONTAINERPREFIX, cacheKey);

            _logger.LogDebug("Retrieved skill item in cache with key {0}", cacheKey);


            return retItem;
        }

        public async Task SetCacheValueAsync<T>(string skillId, string cacheItemName, T cacheItem)
        {

            if (string.IsNullOrWhiteSpace(skillId))
                throw new ArgumentException("skillId cannot be null or empty");

            if (string.IsNullOrWhiteSpace(cacheItemName))
                throw new ArgumentException("cacheItemName cannot be null or empty");



            string cacheKey = GetCacheKey(skillId, cacheItemName);

            await _cache.SetAsync<T>(CONTAINERPREFIX, cacheKey, cacheItem);

            _logger.LogDebug("Stored skill item in cache with key {0}", cacheKey);


        }


        private string GetCacheKey(string skillId, string cacheItemName)
        {
            string retVal = string.Concat(skillId, ":", cacheItemName);
            return retVal;
        }

    }
}
