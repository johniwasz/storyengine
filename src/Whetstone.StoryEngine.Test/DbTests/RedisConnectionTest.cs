﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using StackExchange.Redis;
using Xunit;
using Whetstone.StoryEngine;
using Whetstone.StoryEngine.ConfigurationExtensions;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;

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


            //ParameterStoreProvider storeProvider = new ParameterStoreProvider(RegionEndpoint.USEast1);

            //storeProvider.TryGet("/storyengine/dev/cachetoken", out var tokenPassword);

            string tokenPassword =
                "NUki9d^g6yUsF!0ju#h3AM$+PfSecjJQ$cyw|rqoZvL*j|XANZ#UDIJUBxGX-401c!x*?d4zwDm-HEE!Wrn^98Z7BiT8Sb*pZt7iQbZf!*WI2OlT#!wiFXD2A+lX!HF7";


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
                InstanceName = "dev",
                ConfigurationOptions = redisOpts
            });

            return redisCache;

        }


  

        [Fact]
        public void SerializeRedisSettings()
        {

            var yamlSerializer = YamlSerializationBuilder.GetYamlSerializer();




           var  redisOpts = new ConfigurationOptions
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