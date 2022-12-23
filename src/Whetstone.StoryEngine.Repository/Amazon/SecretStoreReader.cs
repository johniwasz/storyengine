using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.Repository.Amazon
{
    public class SecretStoreReader : ISecretStoreReader
    {
        private readonly ILogger<SecretStoreReader> _logger = null;
        private readonly RegionEndpoint _curRegion = null;

        public SecretStoreReader(IOptions<EnvironmentConfig> envOptions, ILogger<SecretStoreReader> logger)
        {

            if (envOptions == null)
                throw new ArgumentNullException(nameof(envOptions));

            _curRegion = envOptions.Value?.Region ?? throw new ArgumentNullException(nameof(envOptions), "Region property cannot be null");

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }



        public async Task<string> GetValueAsync(string parameterName)
        {

            if (string.IsNullOrWhiteSpace(parameterName))
                throw new ArgumentNullException(nameof(parameterName));

            string paramValue = null;

            GetSecretValueRequest request = new GetSecretValueRequest
            {
                SecretId = parameterName, VersionStage = "AWSCURRENT"
            };
            // VersionStage defaults to AWSCURRENT if unspecified.

            GetSecretValueResponse response = null;

            // In this sample we only handle the specific exceptions for the 'GetSecretValue' API.
            // See https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
            // We rethrow the exception by default.
            Stopwatch secretResponseTime = Stopwatch.StartNew();
            bool isError = true;
            try
            {

                using (IAmazonSecretsManager client =
                    new AmazonSecretsManagerClient(_curRegion))
                {
                    response = await client.GetSecretValueAsync(request);
                }

                isError = false;
            }
            catch (DecryptionFailureException)
            {
                // Secrets Manager can't decrypt the protected secret text using the provided KMS key.
                // Deal with the exception here, and/or rethrow at your discretion.
                throw;
            }
            catch (InternalServiceErrorException)
            {
                // An error occurred on the server side.
                // Deal with the exception here, and/or rethrow at your discretion.
                throw;
            }
            catch (InvalidParameterException)
            {
                // You provided an invalid value for a parameter.
                // Deal with the exception here, and/or rethrow at your discretion
                throw;
            }
            catch (InvalidRequestException)
            {
                // You provided a parameter value that is not valid for the current state of the resource.
                // Deal with the exception here, and/or rethrow at your discretion.
                throw;
            }
            catch (ResourceNotFoundException)
            {
                // We can't find the resource that you asked for.
                // Deal with the exception here, and/or rethrow at your discretion.
                _logger.LogWarning($"Secret {parameterName} not found in secret store");
            }
            catch (System.AggregateException)
            {
                // More than one of the above exceptions were triggered.
                // Deal with the exception here, and/or rethrow at your discretion.
                throw;
            }
            finally
            {
                secretResponseTime.Stop();
                _logger.LogInformation(
                    isError
                        ? $"Error getting secret {parameterName}: {secretResponseTime.ElapsedMilliseconds}"
                        : $"Time to retrieve secret {parameterName}: {secretResponseTime.ElapsedMilliseconds}");
            }


            if (response != null)
            {
                // Decrypts secret using the associated KMS CMK.
                // Depending on whether the secret is a string or binary, one of these fields will be populated.
                if (response.SecretString != null)
                {
                    paramValue = response.SecretString;
                }
                else
                {
                    using (MemoryStream memoryStream = response.SecretBinary)
                    {
                        StreamReader reader = new StreamReader(memoryStream);
                        paramValue =
                            System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));
                    }
                }
            }



            return paramValue;
        }
    }
}
