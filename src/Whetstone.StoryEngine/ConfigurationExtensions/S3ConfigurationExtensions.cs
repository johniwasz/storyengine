using Amazon;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using System;

namespace Whetstone.StoryEngine.ConfigurationExtensions
{
    public static class S3ConfigurationExtensions
    {



        public static IConfigurationBuilder AddS3YamlFile(this IConfigurationBuilder builder, string environment,
            RegionEndpoint endpoint, string path, string bucketName)
        {

            return AddS3YamlFile(builder, environment, endpoint, path, bucketName, null);

        }

        /// <summary>
        /// Adds a YAML configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">Path relative to the base path stored in 
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="environment">Environment of the hosting Lambda function read from the ENVIRONMENT setting.</param>
        /// <param name="endpoint">Region where the S3 Yaml file is hosted</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddS3YamlFile(this IConfigurationBuilder builder, string environment, RegionEndpoint endpoint, string path, string bucketName, IDistributedCache cache)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException($"Invalid file path {nameof(path)}");
            }

            var source = new S3ConfigurationSource(environment, endpoint, path, bucketName, cache);

            builder.Add(source);
            return builder;
        }


    }
}
