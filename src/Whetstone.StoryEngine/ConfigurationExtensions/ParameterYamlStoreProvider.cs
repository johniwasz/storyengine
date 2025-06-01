using Amazon;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

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

                GetParameterResponse getResp = null;


                paramValue = null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting parameter key {_parameterKey} in region {_curRegion.SystemName}", ex);
            }

            SortedDictionary<string, string> configData = null;

            if (paramValue is not null)
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(paramValue);
                //byte[] byteArray = Encoding.ASCII.GetBytes(contents);

                var parser = new YamlConfigurationFileParser();

                using (MemoryStream stream = new MemoryStream(byteArray))
                {
                    configData = parser.Parse(stream);
                }
            }
            this.Data = configData;
        }
    }
}
