namespace Whetstone.StoryEngine.Cache.Models
{
    public interface ICacheTable
    {
        string CacheKey { get; set; }

        byte[] Value { get; set; }

        long Ttl { get; set; }

        CacheOptions CacheOptions { get; set; }
    }
}