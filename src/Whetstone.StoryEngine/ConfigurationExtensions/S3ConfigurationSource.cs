using Amazon;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.ConfigurationExtensions
{
    public class S3ConfigurationSource : IConfigurationSource
    {
        public string Path { get; }

        public RegionEndpoint Endpoint { get; }

        public string Environment { get; }

        public string BucketName { get; }

        public IDistributedCache Cache { get; }


        public S3ConfigurationSource(string environment, RegionEndpoint endpoint, string path, string bucketName, IDistributedCache cache)
        {
            Endpoint = endpoint;
            Path = path;
            BucketName = bucketName;
            Environment = environment;
            Cache = cache;
        }


        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            S3YamlConfigurationProvider prov = new S3YamlConfigurationProvider(this);
            return prov;
        }
    }
}
