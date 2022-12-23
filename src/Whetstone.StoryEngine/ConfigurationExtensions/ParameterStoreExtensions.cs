using Amazon;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.ConfigurationExtensions
{
    public static class ParameterStoreExtensions
    {

        public static IConfigurationBuilder AddParameterStore(this IConfigurationBuilder builder, string regionEndpoint)
        {

            RegionEndpoint endpoint = RegionEndpoint.GetBySystemName(regionEndpoint);

            return AddParameterStore(builder, endpoint);
        }


        public static IConfigurationBuilder AddParameterStore(this IConfigurationBuilder builder, RegionEndpoint regionEndpoint)
        {
            if (builder == null)
            {
                throw new ArgumentNullException($"{nameof(builder)}");
            }

            if (regionEndpoint == null)
                throw new ArgumentNullException($"{nameof(regionEndpoint)}");

            ParameterStoreConfigurationSource configSource = new ParameterStoreConfigurationSource(regionEndpoint);
            builder.Add(configSource);
            return builder;
        }


    }
}
