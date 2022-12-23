using System;
using Amazon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Whetstone.StoryEngine.ConfigurationExtensions
{
    public class ParameterYamlConfigurationSource: IConfigurationSource
    {

        private readonly string _parameterKey;
        private readonly RegionEndpoint _region;
        private readonly ILoggerFactory _loggerFactory;


        public ParameterYamlConfigurationSource(RegionEndpoint region, string parameterKey, ILoggerFactory loggerFactory)
        {

            _region = region ?? throw new ArgumentNullException(nameof(region));

            if (string.IsNullOrWhiteSpace(parameterKey))
                throw new ArgumentNullException(nameof(parameterKey));

            _parameterKey = parameterKey;


            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {

            ILogger<ParameterYamlStoreProvider> paramLogger = _loggerFactory.CreateLogger<ParameterYamlStoreProvider>();


            ParameterYamlStoreProvider prov = new ParameterYamlStoreProvider(_region,  _parameterKey, paramLogger);

            return prov;
        }


    }
}
