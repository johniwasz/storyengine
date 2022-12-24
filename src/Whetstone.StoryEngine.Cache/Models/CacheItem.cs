namespace Whetstone.StoryEngine.Cache.Models
{
    public class CacheItem : ICacheTable
    {



        public string CacheKey { get; set; }

        public byte[] Value { get; set; }

        public long Ttl { get; set; }
        public CacheOptions CacheOptions { get; set; }


    }
}