using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Lambda.Core;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.StoryEngine.LambdaUtilities.ConfigUpdate;
using Whetstone.StoryEngine.LambdaUtilities.Models;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.Repository.Amazon;
using YamlDotNet.Core;

namespace Whetstone.StoryEngine.LambdaUtilities
{

    public class BootstrapConfigFunction : CustomRequestFunctionBase<ConfigUpdateRequest>
    {




        internal override async Task<RequestProcessResponse> ProcessRequestAsync(ResourceRequestType requestType,
                ConfigUpdateRequest configRequest, ILambdaLogger logger)
        {
            RequestProcessResponse procResponse = new RequestProcessResponse
            {
                IsProcessed = true
            };

            if (configRequest == null)
            {
                logger.LogLine("Error: config request cannot be null");
                procResponse.IsProcessed = false;
                procResponse.TemplateStatus = "Config request is empty";
                return procResponse;
            }

            if (string.IsNullOrWhiteSpace(configRequest.Parameter))
            {
                logger.LogLine("Error: config request parameter cannot be null or empty");
                procResponse.IsProcessed = false;
                procResponse.TemplateStatus = "Config request Parameter property not set";
                return procResponse;
            }


            switch (requestType)
            {
                case ResourceRequestType.Update:
                case ResourceRequestType.Create:
                    procResponse = await UpdateParameterAsync(configRequest, logger);
                    break;
                case ResourceRequestType.Delete:
                    procResponse = await UpdateParameterAsync(configRequest, logger, true);
                    break;
            }

            return procResponse;
        }


        private async Task<RequestProcessResponse> UpdateParameterAsync(ConfigUpdateRequest configRequest, ILambdaLogger logger, bool isDelete = false)
        {
            RequestProcessResponse retResp = new RequestProcessResponse();
            retResp.IsProcessed = true;

            // Get the requested parameter.
            BootstrapConfig bootConfig = null;

            try
            {
                bootConfig = await GetBootstrapConfigAsync(configRequest.Parameter, logger);
            }
            catch (YamlException yamlEx)
            {
                logger.LogLine($"Error deserializing bootstrap config: {yamlEx}");
                retResp.IsProcessed = false;
                retResp.TemplateStatus = "Error deserializing bootstrap config";
                return retResp;

            }
            catch (Exception ex)
            {

                logger.LogLine($"Error retrieving bootstrap config: {ex}");
                retResp.IsProcessed = false;
                retResp.TemplateStatus = "Error reading bootstrap config";
                return retResp;
            }

            bool isNewParameter = false;
            if (bootConfig == null)
            {
                isNewParameter = true;
                bootConfig = new BootstrapConfig();
            }


            int entryCount = (configRequest.ConfigEntries?.Count).GetValueOrDefault(0);

            int entryIndex = 0;
            Dictionary<string, string> updatedValues = new Dictionary<string, string>();


            while (entryIndex < entryCount && retResp.IsProcessed)
            {
                var configEntry = configRequest.ConfigEntries[entryIndex];

                if (isDelete)
                    configEntry.Value = null;

                retResp = await UpdateConfiguration(bootConfig, configEntry, logger);
                if (retResp.IsProcessed && retResp.Data != null)
                {
                    if (retResp.Data.Keys.Any())
                    {
                        foreach (string keyName in retResp.Data.Keys)
                        {
                            updatedValues.Add(keyName, retResp.Data[keyName]);
                        }
                    }
                }

                entryIndex++;
            }


            if (retResp.IsProcessed)
            {
                // Update the parameter in AWS
                retResp = await WriteBootstrapConfig(configRequest.Parameter, bootConfig, configRequest.KeyId, isNewParameter, logger);
                if (updatedValues.Keys.Any())
                    retResp.Data = updatedValues;
            }

            return retResp;

        }

        private async Task<RequestProcessResponse> UpdateConfiguration(BootstrapConfig bootConfig, ConfigEntry entry, ILambdaLogger logger)
        {
            RequestProcessResponse retReq = null;



            switch (entry.ConfigType)
            {
                case ConfigEntryType.ReportBucket:
                    retReq = UpdateConfig(entry, logger, new Action<string>(configVal =>
                    {
                        bootConfig.ReportBucket = configVal;
                    }));
                    break;
                case ConfigEntryType.TitleBucket:
                    retReq = UpdateConfig(entry, logger, new Action<string>(configVal =>
                    {
                        bootConfig.Bucket = configVal;
                    }));
                    break;
                case ConfigEntryType.ReportStepFunction:
                    retReq = UpdateConfig(entry, logger, new Action<string>(configVal =>
                    {
                        bootConfig.ReportStepFunction = configVal;
                    }));
                    break;
                case ConfigEntryType.SessionAuditQueue:
                    retReq = UpdateConfig(entry, logger, new Action<string>(configVal =>
                    {
                        bootConfig.SessionAuditQueue = configVal;
                    }));

                    break;
                case ConfigEntryType.SessionLoggerType:
                    retReq = UpdateEnumConfig<SessionLoggerType>(entry, logger, SessionLoggerType.Queue, (
                        configVal =>
                        {
                            bootConfig.SessionLoggerType = configVal;
                        }));
                    break;
                case ConfigEntryType.DynamoDbUserTable:
                    retReq = UpdateConfig(entry, logger, new Action<string>(configVal =>
                    {
                        if (bootConfig.DynamoDBTables == null)
                            bootConfig.DynamoDBTables = new DynamoDBTablesConfig();
                        bootConfig.DynamoDBTables.UserTable = configVal;
                    }));
                    break;
                case ConfigEntryType.DynamoDbCacheTable:
                    retReq = UpdateConfig(entry, logger, new Action<string>(configVal =>
                    {
                        if (bootConfig.CacheConfig == null)
                            bootConfig.CacheConfig = new CacheConfig();

                        bootConfig.CacheConfig.DynamoDBTableName = configVal;
                    }));
                    break;
                case ConfigEntryType.IsCacheEnabled:
                    retReq = UpdateIsCacheEnabled(bootConfig, entry.Value, logger);
                    break;
                case ConfigEntryType.CacheSlidingExpiration:
                    retReq = UpdateCacheSlidingExpiration(bootConfig, entry.Value, logger);
                    break;
                case ConfigEntryType.DefaultSmsSenderType:
                    retReq = UpdateEnumConfig<SmsSenderType>(entry, logger, SmsSenderType.Twilio, (
                        configVal =>
                        {
                            if (bootConfig.SmsConfig == null)
                                bootConfig.SmsConfig = new SmsConfig();

                            bootConfig.SmsConfig.SmsSenderType = configVal;
                        }));
                    break;
                case ConfigEntryType.MessageSendRetryLimit:
                    retReq = UpdateMessageSendRetryLimit(bootConfig, entry.Value, logger);
                    break;
                case ConfigEntryType.AdminApiClientId:
                    retReq = UpdateConfig<string>(entry, logger, (configVal =>
                    {
                        if (bootConfig.OpenIdSecurity == null)
                            bootConfig.OpenIdSecurity = new OpenIdSecurity();

                        bootConfig.OpenIdSecurity.ClientId = configVal;
                    }));
                    break;
                case ConfigEntryType.AdminApiMetadataAddress:
                    retReq = UpdateConfig<string>(entry, logger, (configVal =>
                    {
                        if (bootConfig.OpenIdSecurity == null)
                            bootConfig.OpenIdSecurity = new OpenIdSecurity();

                        bootConfig.OpenIdSecurity.MetadataAddress = configVal;
                    }));
                    break;
                case ConfigEntryType.AdminApiResponseType:
                    retReq = UpdateConfig<string>(entry, logger, (configVal =>
                    {
                        if (bootConfig.OpenIdSecurity == null)
                            bootConfig.OpenIdSecurity = new OpenIdSecurity();

                        bootConfig.OpenIdSecurity.ResponseType = configVal;
                    }));
                    break;
                case ConfigEntryType.DynamoDBMaxErrorRetries:
                    retReq = UpdateConfig<string>(entry, logger, (configVal =>
                    {
                        int? intVal = GetInteger(configVal);


                        if (bootConfig.DynamoDBTables == null)
                            bootConfig.DynamoDBTables = new DynamoDBTablesConfig();


                        bootConfig.DynamoDBTables.ErrorRetries = intVal;
                    }));
                    break;
                case ConfigEntryType.DynamoDBTimeout:
                    retReq = UpdateConfig<string>(entry, logger, (configVal =>
                    {
                        int? intVal = GetInteger(configVal);


                        if (bootConfig.DynamoDBTables == null)
                            bootConfig.DynamoDBTables = new DynamoDBTablesConfig();

                        bootConfig.DynamoDBTables.Timeout = intVal;
                    }));
                    break;
                case ConfigEntryType.AuthenticatorType:
                    retReq = UpdateEnumConfig<AuthenticatorType>(entry, logger, AuthenticatorType.Cognito, (
                        configVal =>
                        {

                            if (bootConfig.Security == null)
                                bootConfig.Security = new SecurityConfig();

                            bootConfig.Security.AuthenticatorType = configVal;
                        }));
                    break;
                case ConfigEntryType.CognitoUserClientId:
                    retReq = UpdateConfig<string>(entry, logger, (configVal =>
                    {
                        if (bootConfig.Security == null)
                            bootConfig.Security = new SecurityConfig();

                        if (bootConfig.Security.Cognito == null)
                            bootConfig.Security.Cognito = new CognitoConfig();

                        bootConfig.Security.Cognito.UserPoolClientId = configVal;
                    }));
                    break;
                case ConfigEntryType.CognitoUserPoolId:
                    retReq = UpdateConfig<string>(entry, logger, (configVal =>
                    {
                        if (bootConfig.Security == null)
                            bootConfig.Security = new SecurityConfig();

                        if (bootConfig.Security.Cognito == null)
                            bootConfig.Security.Cognito = new CognitoConfig();

                        bootConfig.Security.Cognito.UserPoolId = configVal;
                    }));
                    break;
                case ConfigEntryType.CognitoUserClientSecret:


                    if (bootConfig.Security == null)
                        bootConfig.Security = new SecurityConfig();

                    if (bootConfig.Security.Cognito == null)
                        bootConfig.Security.Cognito = new CognitoConfig();

                    // Prior configuration settings may be passing in a string rather than a JArray.
                    // If the inbound value is a string, then deal with that.
                    if (entry.Value is string)
                    {
                        retReq = UpdateConfig<string>(entry, logger, (configVal =>
                        {
                            bootConfig.Security.Cognito.UserPoolClientSecret = configVal;
                        }));
                    }
                    else if (entry.Value is JArray)
                    {

                        retReq = await UpdateConfigAsync<JArray>(entry, logger, async (configVal) =>
                        {

                            string clientSecret = await GetCognitoClientSecretAsync(configVal);
                            bootConfig.Security.Cognito.UserPoolClientSecret = clientSecret;

                            return clientSecret;

                        });
                    }

                    break;
                case ConfigEntryType.CognitoUserPoolRegion:
                    retReq = UpdateConfig<string>(entry, logger, (configVal =>
                    {
                        if (bootConfig.Security == null)
                            bootConfig.Security = new SecurityConfig();

                        if (bootConfig.Security.Cognito == null)
                            bootConfig.Security.Cognito = new CognitoConfig();

                        bootConfig.Security.Cognito.UserPoolRegion = configVal;
                    }));

                    break;

                case ConfigEntryType.SocketConnectionTableName:
                    retReq = UpdateConfig(entry, logger, new Action<string>(configVal =>
                    {
                        if (bootConfig.SocketConfig == null)
                            bootConfig.SocketConfig = new SocketConfig();

                        bootConfig.SocketConfig.SocketConnectionTableName = configVal;
                    }));
                    break;

                case ConfigEntryType.SocketWriteEndpoint:
                    retReq = UpdateConfig(entry, logger, new Action<string>(configVal =>
                    {
                        if (bootConfig.SocketConfig == null)
                            bootConfig.SocketConfig = new SocketConfig();

                        bootConfig.SocketConfig.SocketWriteEndpoint = configVal;
                    }));
                    break;

                case ConfigEntryType.PendingNotificationsTableName:
                    retReq = UpdateConfig(entry, logger, new Action<string>(configVal =>
                    {
                        if (bootConfig.SocketConfig == null)
                            bootConfig.SocketConfig = new SocketConfig();

                        bootConfig.SocketConfig.PendingNotificationsTableName = configVal;
                    }));
                    break;

                case ConfigEntryType.NotificationsLambdaArn:
                    retReq = UpdateConfig(entry, logger, new Action<string>(configVal =>
                    {
                        if (bootConfig.SocketConfig == null)
                            bootConfig.SocketConfig = new SocketConfig();

                        bootConfig.SocketConfig.NotificationsLambdaArn = configVal;
                    }));
                    break;

                case ConfigEntryType.NotificationsLambdaName:
                    retReq = UpdateConfig(entry, logger, new Action<string>(configVal =>
                    {
                        if (bootConfig.SocketConfig == null)
                            bootConfig.SocketConfig = new SocketConfig();

                        bootConfig.SocketConfig.NotificationsLambdaName = configVal;
                    }));
                    break;

            }

            return retReq;
        }

        private async Task<string> GetCognitoClientSecretAsync(JArray configVal)
        {

            // expecting two values
            if (configVal == null)
                throw new ArgumentNullException(nameof(configVal));

            if (configVal.Count != 2)
                throw new ArgumentException($"Expected two values. {configVal.Count} found");

            DescribeUserPoolClientRequest clientReq = new DescribeUserPoolClientRequest
            {
                UserPoolId = configVal[0].Value<string>(),
                ClientId = configVal[1].Value<string>()
            };

            string clientSecret;

            try
            {
                using IAmazonCognitoIdentityProvider cogProvider = new AmazonCognitoIdentityProviderClient();
                var clientInfo = await cogProvider.DescribeUserPoolClientAsync(clientReq);

                clientSecret = clientInfo.UserPoolClient.ClientSecret;
            }
            catch (Exception ex)
            {

                throw new Exception(
                    $"Error getting client secret for client id {clientReq.ClientId} and user pool id {clientReq.UserPoolId}",
                    ex);
            }

            return clientSecret;
        }

        private int? GetInteger(string configVal)
        {
            int? retVal = null;

            if (int.TryParse(configVal, out int parsedInt))
                retVal = parsedInt;

            return retVal;
        }

        private RequestProcessResponse UpdateConfig<T>(ConfigEntry entry, ILambdaLogger logger, Action<T> configUpdater) where T : class
        {
            RequestProcessResponse resp = new RequestProcessResponse();
            T configValue = default(T);
            string entryDescription = entry.ConfigType.ToString();

            if (entry.Value != default(T))
            {

                if (entry.Value is T s)
                {
                    configValue = s;
                }
                else
                {
                    resp.IsProcessed = false;
                    resp.TemplateStatus = $"{entryDescription} value is not a {typeof(T)}";
                    return resp;
                }
            }

            resp.Data = new Dictionary<string, string>();


            string configText = (string)(object)configValue;

            if (string.IsNullOrWhiteSpace(configText))
            {
                logger.Log($"Removing {entryDescription} entry");
                resp.Data.Add(entryDescription, string.Empty);
            }
            else
            {
                logger.Log($"Setting {entryDescription} entry to {configValue}");
                resp.Data.Add(entryDescription, configText);
            }


            configUpdater(configValue);

            resp.IsProcessed = true;

            return resp;
        }


        private async Task<RequestProcessResponse> UpdateConfigAsync<T>(ConfigEntry entry, ILambdaLogger logger, Func<T, Task<string>> configUpdater) where T : class
        {
            RequestProcessResponse resp = new RequestProcessResponse();
            T configValue = default(T);
            string entryDescription = entry.ConfigType.ToString();

            if (entry.Value != default(T))
            {

                if (entry.Value is T s)
                {
                    configValue = s;
                }
                else
                {
                    resp.IsProcessed = false;
                    resp.TemplateStatus = $"{entryDescription} value is not a {typeof(T)}";
                    return resp;
                }
            }

            resp.Data = new Dictionary<string, string>();

            string configText = await configUpdater(configValue);

            if (string.IsNullOrWhiteSpace(configText))
            {
                logger.LogLine($"Removing {entryDescription} entry");
                resp.Data.Add(entryDescription, string.Empty);
            }
            else
            {
                logger.LogLine($"Setting {entryDescription} entry to {configText}");
                resp.Data.Add(entryDescription, configText);
            }

            resp.IsProcessed = true;

            return resp;
        }


        private RequestProcessResponse UpdateEnumConfig<T>(ConfigEntry entry, ILambdaLogger logger, T defaultValue, Action<T> configUpdater) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            string entryName = entry.ConfigType.ToString();

            RequestProcessResponse resp = new RequestProcessResponse();
            T? enumVal = null;
            if (entry.Value != null)
            {

                if (entry.Value is string enumText)
                {
                    if (!string.IsNullOrWhiteSpace(enumText))
                    {
                        if (Enum.TryParse(enumText, true, out T parsedType))
                        {
                            enumVal = (T)parsedType;
                        }
                        else
                        {
                            resp.IsProcessed = false;
                            resp.TemplateStatus = $"Attempting to set {entryName} to invalid value {enumText.Substring(0, 100)}";
                            return resp;
                        }
                    }

                }
                else
                {
                    resp.IsProcessed = false;
                    resp.TemplateStatus = $"{entry.ConfigType} value is not a string";
                    return resp;
                }
            }

            resp.Data = new Dictionary<string, string>();
            if (enumVal.HasValue)
            {
                resp.Data.Add(entryName, enumVal.Value.ToString());
                logger.Log($"Setting {entryName} entry to {enumVal.Value}");
            }
            else
            {
                resp.Data.Add(entryName, defaultValue.ToString());
                logger.Log($"Removing {entryName} entry");
            }


            configUpdater(enumVal.GetValueOrDefault(defaultValue));

            resp.IsProcessed = true;

            return resp;
        }

        private RequestProcessResponse UpdateMessageSendRetryLimit(BootstrapConfig bootConfig, object value, ILambdaLogger logger)
        {
            RequestProcessResponse resp = new RequestProcessResponse();
            int? messageSendRetryLimitVal = null;
            if (value != null)
            {

                if (value is int i)
                {
                    messageSendRetryLimitVal = i;
                }
                else if (value is string s)
                {
                    if (int.TryParse(s, out int intResult))
                    {
                        messageSendRetryLimitVal = intResult;
                    }

                }
                else
                {
                    resp.IsProcessed = false;
                    resp.TemplateStatus = "MessageSendRetryLimit value is not a string";
                    return resp;
                }
            }

            resp.Data = new Dictionary<string, string>();
            if (!messageSendRetryLimitVal.HasValue)
            {
                logger.Log($"Removing MessageSendRetryLimit entry");
                resp.Data.Add(ConfigEntryType.MessageSendRetryLimit.ToString(), "3");
            }
            else
            {
                logger.Log($"Setting MessageSendRetryLimit entry to {messageSendRetryLimitVal.Value}");
                resp.Data.Add(ConfigEntryType.MessageSendRetryLimit.ToString(), messageSendRetryLimitVal.Value.ToString());
            }

            if (bootConfig.SmsConfig == null)
                bootConfig.SmsConfig = new SmsConfig();

            // Default to enabled unless explicitly set to false;
            bootConfig.SmsConfig.MessageSendRetryLimit = messageSendRetryLimitVal.GetValueOrDefault(3);

            resp.IsProcessed = true;

            return resp;
        }





        private RequestProcessResponse UpdateCacheSlidingExpiration(BootstrapConfig bootConfig, dynamic value, ILambdaLogger logger)
        {
            RequestProcessResponse resp = new RequestProcessResponse();
            int? slidingExpirationVal = null;
            if (value != null)
            {

                if (value is int i)
                {
                    slidingExpirationVal = i;
                }
                else if (value is string s)
                {
                    if (int.TryParse(s, out int intResult))
                    {
                        slidingExpirationVal = intResult;
                    }

                }
                else
                {
                    resp.IsProcessed = false;
                    resp.TemplateStatus = "DynamoDbCacheTable value is not a string";
                    return resp;
                }
            }

            resp.Data = new Dictionary<string, string>();
            if (!slidingExpirationVal.HasValue)
            {
                logger.Log($"Removing CacheSlidingExpiration entry");
                resp.Data.Add(ConfigEntryType.CacheSlidingExpiration.ToString(), "10000");
            }
            else
            {
                logger.Log($"Setting CacheSlidingExpiration entry to {slidingExpirationVal.Value}");
                resp.Data.Add(ConfigEntryType.CacheSlidingExpiration.ToString(), slidingExpirationVal.Value.ToString());
            }

            if (bootConfig.CacheConfig == null)
                bootConfig.CacheConfig = new CacheConfig();

            // Default to enabled unless explicitly set to false;
            bootConfig.CacheConfig.DefaultSlidingExpirationSeconds = slidingExpirationVal.GetValueOrDefault(10000);

            resp.IsProcessed = true;

            return resp;
        }

        private RequestProcessResponse UpdateIsCacheEnabled(BootstrapConfig bootConfig, object value, ILambdaLogger logger)
        {
            RequestProcessResponse resp = new RequestProcessResponse();
            bool? isCacheEnabledVal = null;
            if (value != null)
            {

                if (value is bool b)
                {
                    isCacheEnabledVal = b;
                }
                else if (value is string)
                {
                    if (bool.TryParse((string)value, out bool boolResult))
                    {
                        isCacheEnabledVal = boolResult;
                    }

                }
                else
                {
                    resp.IsProcessed = false;
                    resp.TemplateStatus = "DynamoDbCacheTable value is not a string";
                    return resp;
                }
            }

            resp.Data = new Dictionary<string, string>();
            if (!isCacheEnabledVal.HasValue)
            {
                logger.Log($"Removing IsCacheEnabled entry");
                resp.Data.Add(ConfigEntryType.IsCacheEnabled.ToString(), true.ToString());
            }
            else
            {
                logger.Log($"Setting IsCacheEnabled entry to {isCacheEnabledVal.Value}");
                resp.Data.Add(ConfigEntryType.IsCacheEnabled.ToString(), isCacheEnabledVal.Value.ToString());
            }

            if (bootConfig.CacheConfig == null)
                bootConfig.CacheConfig = new CacheConfig();

            // Default to enabled unless explicitly set to false;
            bootConfig.CacheConfig.IsEnabled = isCacheEnabledVal.GetValueOrDefault(true);

            resp.IsProcessed = true;

            return resp;
        }


        private RequestProcessResponse UpdateDynamoDbCacheTable(BootstrapConfig bootConfig, dynamic value, ILambdaLogger logger)
        {
            RequestProcessResponse resp = new RequestProcessResponse();
            string cacheTableVal = null;
            if (value != null)
            {

                if (value is string)
                {
                    cacheTableVal = (string)value;
                }
                else
                {
                    resp.IsProcessed = false;
                    resp.TemplateStatus = "DynamoDbCacheTable value is not a string";
                    return resp;
                }
            }

            resp.Data = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(cacheTableVal))
            {
                logger.Log($"Removing DynamoDbCacheTable entry");
                resp.Data.Add(ConfigEntryType.DynamoDbCacheTable.ToString(), string.Empty);
            }
            else
            {
                logger.Log($"Setting DynamoDbCacheTable entry to {cacheTableVal}");
                resp.Data.Add(ConfigEntryType.DynamoDbCacheTable.ToString(), cacheTableVal);
            }

            if (bootConfig.CacheConfig == null)
                bootConfig.CacheConfig = new CacheConfig();

            bootConfig.CacheConfig.DynamoDBTableName = cacheTableVal;

            resp.IsProcessed = true;

            return resp;
        }








        private async Task<BootstrapConfig> GetBootstrapConfigAsync(string paramName, ILambdaLogger logger)
        {
            BootstrapConfig configVal = null;
            RegionEndpoint region = StoryEngine.ContainerSettingsReader.GetAwsDefaultEndpoint();

            EnvironmentConfig envConfig = new EnvironmentConfig
            {
                Region = region
            };
            IOptions<EnvironmentConfig> envOpts = Options.Create(envConfig);

            ILoggerFactory logFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });


            var storeLogger = logFactory.CreateLogger<ParameterStoreReader>();


            IParameterStoreReader paramReader = new ParameterStoreReader(envOpts, storeLogger);

            logger.LogLine($"Getting parameter {paramName}");

            string paramValue = await paramReader.GetValueAsync(paramName);

            if (!string.IsNullOrWhiteSpace(paramValue))
            {
                var yamlDeser = YamlSerializationBuilder.GetYamlDeserializer();


                configVal = yamlDeser.Deserialize<BootstrapConfig>(paramValue);



                logger.LogLine($"Deserialized parameter {paramName}");
            }
            else
            {
                logger.LogLine($"Parameter {paramName} not found");
            }

            return configVal;
        }

        private async Task<RequestProcessResponse> WriteBootstrapConfig(string paramName, BootstrapConfig bootConfig, string keyId, bool isNew, ILambdaLogger logger)
        {
            RequestProcessResponse procResp = new RequestProcessResponse();
            procResp.IsProcessed = true;

            Stopwatch parameterTime = new Stopwatch();

            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();
            string paramValue = yamlSer.Serialize(bootConfig);



            PutParameterRequest putReq = new PutParameterRequest
            {
                Name = paramName,
                Value = paramValue,
                Type = ParameterType.String
            };


            if (!string.IsNullOrWhiteSpace(keyId))
            {
                putReq.Type = ParameterType.SecureString;
                putReq.KeyId = keyId;
            }

            putReq.Overwrite = !isNew;

            try
            {
                PutParameterResponse putResp = null;
                RegionEndpoint region = StoryEngine.ContainerSettingsReader.GetAwsDefaultEndpoint();
                using (var ssmClient = new AmazonSimpleSystemsManagementClient(region))
                {
                    parameterTime.Start();
                    putResp = await ssmClient.PutParameterAsync(putReq);
                    parameterTime.Stop();
                    logger.Log(
                        $"Updated contents of parameter {paramName} in {parameterTime.ElapsedMilliseconds}ms");
                }


            }
            catch (Exception ex)
            {
                procResp.IsProcessed = false;
                procResp.TemplateStatus = $"Error writing to parameter {paramName}";
                logger.Log($"Error writing to parameter {paramName}: {ex}");
            }


            return procResp;
        }

    }
}
