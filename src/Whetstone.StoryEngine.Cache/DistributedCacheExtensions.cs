using Microsoft.Extensions.Caching.Distributed;
using Nito.AsyncEx;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Cache
{
    public static class DistributedCacheExtensions
    {
        private static bool _isEnabled = true;
        private static DistributedCacheEntryOptions _defaultOptions;

        public static void SetDefaultCacheOptions(DistributedCacheEntryOptions defaultOptions, bool isEnabled)
        {
            _isEnabled = isEnabled;
            _defaultOptions = defaultOptions;
        }


        public static async Task SetAsync<T>(this IDistributedCache cache, string container, string key, T val)
        {
            if (_isEnabled)
            {
                string contKey = BuildKey(container, key);
                await cache.SetObjectAsync(contKey, val, _defaultOptions);
            }
        }

        public static async Task SetAsync<T>(this IDistributedCache cache, string container, string key, T val, DistributedCacheEntryOptions cacheOptions)
        {

            if (_isEnabled)
            {
                string contKey = BuildKey(container, key);
                await cache.SetObjectAsync(contKey, val, cacheOptions);
            }

        }


        public static void Set<T>(this IDistributedCache cache, string container, string key, T val, DistributedCacheEntryOptions cacheOptions)
        {
            if (_isEnabled)
            {
                string contKey = BuildKey(container, key);
                AsyncContext.Run(async () => await cache.SetObjectAsync(contKey, val, cacheOptions));
            }
        }

        public static T Get<T>(this IDistributedCache cache, string container, string key)
        {
            if (_isEnabled)
            {
                string contKey = BuildKey(container, key);
                return AsyncContext.Run(async () => await cache.GetObjectAsync<T>(contKey));
            }
            return default(T);
        }

        public static async Task RemoveAsync<T>(this IDistributedCache cache, string container, string key)
        {
            if (_isEnabled)
            {
                string contKey = BuildKey(container, key);
                await cache.RemoveAsync(contKey);
            }
        }

        public static async Task<string> GetStringWithDefaultOptionsAsync(this IDistributedCache cache, string container, string key)
        {
            if (_isEnabled)
            {
                string contKey = BuildKey(container, key);
                return await cache.GetStringAsync(contKey);
            }
        
            return null;
        }



        public static async Task SetStringWithDefaultOptionsAsync(this IDistributedCache cache, string container, string key, string val)
        {
            if (_isEnabled)
            {
                string contKey = BuildKey(container, key);
                await cache.SetStringAsync(contKey, val, _defaultOptions);
            }
        }

        public static async Task SetStringAsync(this IDistributedCache cache, string container, string key, string val)
        {
            if (_isEnabled)
            {
                string contKey = BuildKey(container, key);
                await cache.SetStringAsync(contKey, val, _defaultOptions);
            }
        }


        public static async Task<string> GetStringAsync(this IDistributedCache cache, string container, string key)
        {
            string contKey = BuildKey(container, key);
            return await cache.GetStringAsync(contKey);
        }

        public static async Task<T> GetAsync<T>(this IDistributedCache cache, string container, string key)
        {
            string contKey = BuildKey(container, key);
            T cacheVal = await cache.GetObjectAsync<T>(contKey);


            return cacheVal;

        }

        public static async Task RemoveAsync(this IDistributedCache cache, string container, string key)
        {
            if (_isEnabled)
            {
                string contKey = BuildKey(container, key);
                await cache.RemoveAsync(contKey);
            }
        }


        private static string BuildKey(string container, string key)
        {
            return string.Concat(container, ":", key).ToLower();
        }
        
    }
}
