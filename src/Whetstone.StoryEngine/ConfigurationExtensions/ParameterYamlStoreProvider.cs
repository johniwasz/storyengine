using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Amazon;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;

namespace Whetstone.StoryEngine.ConfigurationExtensions
{
    public class ParameterYamlStoreProvider : ConfigurationProvider
    {
        private readonly RegionEndpoint _curRegion;
        private readonly ILogger<ParameterYamlStoreProvider> _yamlLogger;

        private readonly string _parameterKey;
        public ParameterYamlStoreProvider(RegionEndpoint endpoint, string parameterKey, ILogger<ParameterYamlStoreProvider> logger)
        {

            if (string.IsNullOrWhiteSpace(parameterKey))
                throw new ArgumentNullException($"{nameof(parameterKey)}");

            _parameterKey = parameterKey;


            _curRegion = endpoint ?? throw new ArgumentNullException($"{nameof(endpoint)}");

            _yamlLogger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");

        }



        public override void Load()
        {

            string paramValue;

            try
            {

                _yamlLogger.LogDebug($"Retrieving parameter {_parameterKey}");
                GetParameterResponse getResp = null;
                _yamlLogger.LogDebug($"Getting parameter store {_parameterKey} in region {_curRegion.SystemName}");


                Stopwatch paramRetrievalTime = new Stopwatch();
                paramRetrievalTime.Start();
                using (var ssmClient = new AmazonSimpleSystemsManagementClient(_curRegion))
                {
                    getResp = AsyncContext.Run(async () => await ssmClient.GetParameterAsync(new GetParameterRequest
                    {
                        Name = _parameterKey,
                        WithDecryption = true
                    }));
                }
                paramRetrievalTime.Stop();

                _yamlLogger.LogDebug($"Parameter data in {_parameterKey} in region {_curRegion.SystemName} retrieved in {paramRetrievalTime.ElapsedMilliseconds}ms");
                paramValue = getResp.Parameter.Value;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting parameter key {_parameterKey} in region {_curRegion.SystemName}", ex);
            }

            var parser = new YamlConfigurationFileParser();
            SortedDictionary<string, string> configData = null;

            byte[] byteArray = Encoding.UTF8.GetBytes(paramValue);
            //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
          
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                configData = parser.Parse(stream);
            }

            this.Data = configData;
        }
    }
}
