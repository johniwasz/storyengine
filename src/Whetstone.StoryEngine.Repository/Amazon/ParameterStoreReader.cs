using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Options;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.Repository.Amazon
{
    public class ParameterStoreReader : IParameterStoreReader
    {
        private readonly ILogger<ParameterStoreReader> _logger;
        private readonly RegionEndpoint _curRegion = null;

        public ParameterStoreReader(IOptions<EnvironmentConfig> envOptions, ILogger<ParameterStoreReader> logger)
        {

            if (envOptions == null)
                throw new ArgumentNullException(nameof(envOptions));

            _curRegion = envOptions.Value?.Region ?? throw new ArgumentNullException(nameof(envOptions), "Region property cannot be null");


            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GetValueAsync(string parameterName)
        {
            Stopwatch parameterTime = new Stopwatch();
            string paramValue = null;
            try
            {
                GetParameterResponse getResp = null;

                using (var ssmClient = new AmazonSimpleSystemsManagementClient(_curRegion))
                {
                    parameterTime.Start();
                    getResp = await ssmClient.GetParameterAsync(new GetParameterRequest
                    {
                        Name = parameterName,
                        WithDecryption = true
                    });
                    parameterTime.Stop();
                    _logger.LogInformation(
                        $"Retrieved contents of parameter {parameterName} in {parameterTime.ElapsedMilliseconds}ms");
                }

                paramValue = getResp.Parameter.Value;

            }
            catch (ParameterNotFoundException)
            {
                parameterTime.Stop();
                _logger.LogInformation($"Parameter {parameterName} not found in {parameterTime.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error retrieving parameter {parameterName}");
                throw;
            }

            return paramValue;
        }
    }
}
