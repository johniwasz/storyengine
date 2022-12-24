using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.IO;
using Whetstone.StoryEngine.Cache;

namespace Whetstone.StoryEngine.ConfigurationExtensions
{
    public class S3YamlConfigurationProvider : ConfigurationProvider
    {

        private const string CONFIGKEY = "configfile";
        private readonly string _configPath;
        private readonly IDistributedCache _cache;
        private readonly RegionEndpoint _endpoint;
        private readonly string _bucketName;
        private readonly string _environment;

        public S3YamlConfigurationProvider(S3ConfigurationSource source)
        {
            _configPath = source.Path;
            _cache = source.Cache;
            _endpoint = source.Endpoint;
            _bucketName = source.BucketName;
            _environment = source.Environment;
        }

        public override void Load()
        {
            SortedDictionary<string, string> configData = null;
            // Bring it in from S3 storage.

            if (_cache != null)
            {
                // load from the cache

                configData = _cache.Get<SortedDictionary<string, string>>(_environment, CONFIGKEY);
            }

            if (configData == null)
            {
                configData = GetConfigFromS3(_endpoint);

                // Apply configuration time. This keeps the config in the cache until programmatically removed.
                DistributedCacheEntryOptions cacheStoreOptions = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.MaxValue
                };

                _cache?.Set(_environment, CONFIGKEY, configData, cacheStoreOptions);
            }

            if (configData != null)
            {
                Dictionary<string, string> envConfig = new Dictionary<string, string>();

                foreach (string sortKey in configData.Keys)
                {
                    envConfig.Add(string.Concat(_environment, ":", sortKey), configData[sortKey]);
                }

                Data = envConfig;
            }

        }


        private SortedDictionary<string, string> GetConfigFromS3(RegionEndpoint endpoint)
        {
            var parser = new YamlConfigurationFileParser();
            SortedDictionary<string, string> configData = null;

            using (IAmazonS3 client = new AmazonS3Client(endpoint))
            {

                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = _configPath
                };

                using (GetObjectResponse response = AsyncContext.Run(async () => await client.GetObjectAsync(request)))
                {
                    using (BufferedStream buffer = new BufferedStream(response.ResponseStream))
                    {
                        configData = parser.Parse(buffer);
                    }
                }
            }

            return configData;
        }

    }
}
