using System.Text;
using Amazon;


namespace Whetstone.StoryEngine.Cache.Settings
{
    public class DistributedCacheDynamoDbSettings
    {

        /// <summary>
        /// AWS Region
        /// </summary>
        public RegionEndpoint RegionEndpoint { get; set; }

        /// <summary>
        /// Default time to live for cache items
        /// </summary>
        public long DefaultTtl { get; set; }

        /// <summary>
        /// Determines how long the cache operations wait before timing out. Value is in milliseconds. Defaults to 1000.
        /// </summary>
        public int? Timeout { get; set; }



        /// <summary>
        /// Start up settings
        /// </summary>
        public DistributedCacheDynamoDbStartUpSettings StartUpSettings { get; set; }

        /// <summary>
        /// Settings for Dynamo db cache provider
        /// </summary>
        /// <param name="accessKey"></param>
        /// <param name="accessSecret"></param>
        /// <param name="reginEndpoint"></param>
        public DistributedCacheDynamoDbSettings( RegionEndpoint regionEndpoint, int? timeout)
        {

            RegionEndpoint = regionEndpoint;

            Timeout = timeout;

            DefaultTtl = CacheTableAttributes.Ttl;

            StartUpSettings = new DistributedCacheDynamoDbStartUpSettings
            {
                ReadCapacityUnits = CacheTableAttributes.ReadCapacityUnits,
                WriteCapacityUnits = CacheTableAttributes.WriteCapacityUnits,
                CreateDbOnStartUp = CacheTableAttributes.CreateTableOnStartUp
            };
        }

    }
}
