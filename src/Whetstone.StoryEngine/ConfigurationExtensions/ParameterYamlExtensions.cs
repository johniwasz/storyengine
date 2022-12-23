using System;
using System.Collections.Generic;
using System.Text;
using Amazon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Whetstone.StoryEngine.ConfigurationExtensions
{
    public static class ParameterYamlExtensions
    {




        public static IConfigurationBuilder AddParameterYamlStore(this IConfigurationBuilder builder, RegionEndpoint regionEndpoint, string parameterKey, ILoggerFactory loggerFactory)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (regionEndpoint == null)
                throw new ArgumentNullException(nameof(regionEndpoint));

            if (string.IsNullOrWhiteSpace(parameterKey))
                throw new ArgumentNullException(nameof(parameterKey));


            ParameterYamlConfigurationSource configSource = new ParameterYamlConfigurationSource(regionEndpoint, parameterKey, loggerFactory);

            builder.Add(configSource);
            return builder;
        }

    }
}
