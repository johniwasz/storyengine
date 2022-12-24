using Amazon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace Whetstone.StoryEngine.ConfigurationExtensions
{
    public class ParameterStoreConfigurationSource : IConfigurationSource
    {
        protected RegionEndpoint _endPoint = null;


        private readonly ILoggerFactory _loggerFactory;

        public ParameterStoreConfigurationSource(string region, ILoggerFactory loggerFactory)
        {
            if (string.IsNullOrWhiteSpace(region))
                throw new ArgumentException($"{nameof(region)} cannot be null or empty");


            _endPoint = RegionEndpoint.GetBySystemName(region);

            _loggerFactory = loggerFactory ?? throw new ArgumentNullException($"{nameof(loggerFactory)}");
        }


        public ParameterStoreConfigurationSource(RegionEndpoint region)
        {
            _endPoint = region ?? throw new ArgumentException($"{nameof(region)} cannot be null or empty");
        }

        public virtual IConfigurationProvider Build(IConfigurationBuilder builder)
        {

            ILogger<ParameterStoreProvider> logger = _loggerFactory.CreateLogger<ParameterStoreProvider>();


            ParameterStoreProvider prov = new ParameterStoreProvider(_endPoint, logger);

            return prov;
        }
    }
}
