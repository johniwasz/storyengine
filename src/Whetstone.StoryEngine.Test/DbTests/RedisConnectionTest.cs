using Microsoft.Extensions.Caching.Redis;
using StackExchange.Redis;
using System;
using Whetstone.StoryEngine.Models.Serialization;
using Xunit;

namespace Whetstone.StoryEngine.Test.DbTests
{
    public class RedisConnectionTest
    {
        private static readonly Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            string cacheConnection = "myredisCluster-001.jejod.0001.euc1.cache.amazonaws.com:6379";
            return ConnectionMultiplexer.Connect(cacheConnection);
        });

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }


        //[Fact]
        //public async Task ConnectToRedis()
        //{
        //    var redisCache = GetSecuredCache();

        //    DistributedCacheEntryOptions distOptions = new DistributedCacheEntryOptions
        //    {
        //        AbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 30, 0)
        //    };

        //    Whetstone.StoryEngine.Cache.DistributedCacheExtensions.SetDefaultCacheOptions(distOptions, true);


        //    try
        //    {
        //        await redisCache.SetStringAsync("mycontainer", "testvalue", "myvalue");
        //        string storedVal = await redisCache.GetStringAsync("mycontainer", "testvalue");

        //        Debug.WriteLine(storedVal);
        //    }
        //    catch (RedisConnectionException e)
        //    {
        //        Console.WriteLine(e);
        //        throw;
        //    }
        //}




        private RedisCache GetSecuredCache()
        {
            string tokenPassword =
                "NOLONGERVALID";


            var redisOpts = new ConfigurationOptions
            {
                EndPoints =
                 {


                     {"master.whe1h1z0hoh9gbbk.drbjq8.usw2.cache.amazonaws.com:6379"},
                     {"whe1h1z0hoh9gbbk-001.whe1h1z0hoh9gbbk.drbjq8.usw2.cache.amazonaws.com:6379"},
                     {"whe1h1z0hoh9gbbk-002.whe1h1z0hoh9gbbk.drbjq8.usw2.cache.amazonaws.com:6379"}
                 },
                KeepAlive = 180,
                AbortOnConnectFail = false,
                Password = tokenPassword,
                Ssl = true
            };


            RedisCache redisCache = new RedisCache(new RedisCacheOptions
            {
                InstanceName = "dev"
            });

            return redisCache;

        }




        [Fact]
        public void SerializeRedisSettings()
        {

            var yamlSerializer = YamlSerializationBuilder.GetYamlSerializer();




            var redisOpts = new ConfigurationOptions
            {
                EndPoints =
                {
                    {"dev-cache-001.prgrxr.0001.use1.cache.amazonaws.com:6379"},
                    {"dev-cache-001.prgrxr.0002.use1.cache.amazonaws.com:6379"}
                },
                KeepAlive = 180,
                AbortOnConnectFail = false

            };

            string yamlText = yamlSerializer.Serialize(redisOpts);

        }

    }
}
