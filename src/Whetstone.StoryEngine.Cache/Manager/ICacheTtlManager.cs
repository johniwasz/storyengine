using System;
using Microsoft.Extensions.Caching.Distributed;
using Whetstone.StoryEngine.Cache.Models;

namespace Whetstone.StoryEngine.Cache.Manager
{
    public interface ICacheTtlManager
    {
        long ToUnixTime(DateTime date);
        CacheOptions ToCacheOptions(DistributedCacheEntryOptions options);
        long ToTtl(CacheOptions cacheOptions);
    }
}
