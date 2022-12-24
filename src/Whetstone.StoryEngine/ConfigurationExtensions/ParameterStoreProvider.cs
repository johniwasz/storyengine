using Amazon;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;

namespace Whetstone.StoryEngine.ConfigurationExtensions
{
    public class ParameterStoreProvider : ConfigurationProvider
    {
        private readonly ILogger<ParameterStoreProvider> _logger;
        private readonly RegionEndpoint _curRegion = null;

        private readonly ConcurrentDictionary<string, string> _paramStoreDictionary = new ConcurrentDictionary<string, string>();

        public ParameterStoreProvider(RegionEndpoint endpoint, ILogger<ParameterStoreProvider> logger)
        {


            _curRegion = endpoint ?? throw new ArgumentNullException($"{nameof(endpoint)}");

            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");

        }



        public override void Set(string key, string value)
        {
            throw new NotSupportedException("Setting a value in the parameter store is not supported");
        }


        public override bool TryGet(string key, out string value)
        {

            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException($"{nameof(key)} cannot be null or empty");

            bool isFound = false;

            if (_paramStoreDictionary.ContainsKey(key))
            {
                value = _paramStoreDictionary[key];
            }
            else
            {

                try
                {
                    GetParameterResponse getResp = null;
                    using (var ssmClient = new AmazonSimpleSystemsManagementClient(_curRegion))
                    {
                        getResp = AsyncContext.Run(async () => await ssmClient.GetParameterAsync(new GetParameterRequest
                        {
                            Name = key,
                            WithDecryption = true
                        }));
                    }

                    isFound = true;
                    string paramValue = getResp.Parameter.Value;
                    _paramStoreDictionary.AddOrUpdate(key, paramValue, (keyText, oldValue) => paramValue);
                    value = paramValue;
                }
                catch (AggregateException aggEx)
                {
                    foreach (var ex in aggEx.InnerExceptions)
                    {
                        if (ex is ParameterNotFoundException)
                        {
                            _logger.LogDebug($"Parameter {key} not found in parameter store");
                            // _paramStoreDictionary.AddOrUpdate(key, (string) null, (keyText, oldValue) => null);

                        }
                        else
                        {
                            _logger.LogError(ex, $"Error getting {key} from parameter store");
                        }
                    }

                    value = null;

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error getting {key} from parameter store");
                    value = null;
                }
            }

            return isFound;
        }
    }
}
