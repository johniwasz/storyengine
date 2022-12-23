using System;
using System.Collections.Generic;
using System.Linq;
using Amazon;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Whetstone.StoryEngine.ConfigurationExtensions;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.DependencyInjection
{
    public class RedisManager
    {

        internal static IDistributedCache GetRedisCache(RegionEndpoint curRegion, CacheConfig cacheConfig)
        {


            if (curRegion == null)
                throw new ArgumentException($"{nameof(curRegion)}");

            if (cacheConfig == null)
                throw new ArgumentNullException($"{nameof(cacheConfig)}");


            ConfigurationOptions redisOptions = GetRedisConfigOptions(cacheConfig);

            IDistributedCache distCache = GetRedisCache(redisOptions);


            DistributedCacheEntryOptions distCacheOpts = new DistributedCacheEntryOptions();
            distCacheOpts.SlidingExpiration = new TimeSpan(0, 0, cacheConfig.DefaultSlidingExpirationSeconds);

            Whetstone.StoryEngine.Cache.DistributedCacheExtensions.SetDefaultCacheOptions(distCacheOpts, cacheConfig.IsEnabled);

            return GetRedisCache( redisOptions);
        }

        internal static IDistributedCache GetRedisCache( ConfigurationOptions configOpts)
        {

            if (configOpts == null)
                throw new ArgumentNullException($"{nameof(configOpts)} cannot be null");

            //configOpts.ConnectTimeout = 1000;
            //configOpts.ConnectRetry = 0;
            RedisCache redisCache = new RedisCache(new RedisCacheOptions
            {
                //InstanceName = environment,
                ConfigurationOptions = configOpts
            });

            return redisCache;
        }

        internal static ConfigurationOptions GetRedisConfigOptions(CacheConfig cacheConfig)
        {
            var redisOpts = new ConfigurationOptions
            {
                EndPoints =
                {
                    cacheConfig.PrimaryEndpoint
                },
                KeepAlive = 180,
                AbortOnConnectFail = false,
                Ssl = true
            };

            if (cacheConfig.ReadOnlyEndpoints != null)
            {

                foreach (string readonlyEndpoint in cacheConfig.ReadOnlyEndpoints)
                {
                    redisOpts.EndPoints.Add(readonlyEndpoint);
                }
            }

            if(!string.IsNullOrWhiteSpace(cacheConfig.Token))
                redisOpts.Password = cacheConfig.Token;

            return redisOpts;
        }



        public static void FlushRedis( IOptions<CacheConfig> cacheConfigOptions, ILogger logger)
        {

            CacheConfig cacheConfig = cacheConfigOptions.Value;


            bool isCacheEnabled = cacheConfig.IsEnabled;

            if (isCacheEnabled)
            {

                ConfigurationOptions redisServerConfig = GetRedisConfigOptions(cacheConfig);

                if (redisServerConfig != null)
                {
                    try
                    {

                        redisServerConfig.AllowAdmin = true;


                        var redisCon = ConnectionMultiplexer.Connect(redisServerConfig);

                        var redisServer = redisCon.GetServer(redisServerConfig.EndPoints[0]);
                        string redisServerName = redisServer.ToString();

;                       redisServer.FlushAllDatabases();
                       

                        logger.LogInformation($"Flushed REDIS cache {redisServerName}");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Error flushing REDIS cache", ex);
                    }
                }
                else
                    logger.LogInformation("No REDIS cache configured to flush");
            }
            else
            {
                logger.LogInformation("Caching is not enabled");
            }

        }



    }
}
