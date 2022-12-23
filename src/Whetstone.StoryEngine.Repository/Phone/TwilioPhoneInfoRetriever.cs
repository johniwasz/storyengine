using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Whetstone.StoryEngine.Models.Configuration;
using System.Threading.Tasks;
using Amazon;
using Whetstone.StoryEngine.Models.Data;
using Twilio.Clients;
using Twilio.Rest.Lookups.V1;
using Microsoft.Extensions.Logging;
using Whetstone.StoryEngine.Models.Messaging;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Caching.Memory;


namespace Whetstone.StoryEngine.Repository.Phone
{

    public class TwilioPhoneInfoRetriever : SecretStoreReaderBase<TwilioCredentials>, IPhoneInfoRetriever
    {

        internal readonly ILogger<TwilioPhoneInfoRetriever> _logger;

        private const string PHONECACHEKEYPREFIX = "PhoneInfo";

        private const string TWILIO_COUNTRYCODE = "mobile_country_code";
        private const string TWILIO_NETWORKCODE = "mobile_network_code";
        private const string TWILIO_CARRIERNAME = "name";
        private const string TWILIO_PHONETYPE = "type";
        private const string TWILIO_ERRORCODE = "error_code";

        private readonly TwilioConfig _twilioConfig;

        private readonly RegionEndpoint _endpoint;

        private readonly string _dynamoTableName;

        private readonly IMemoryCache _memCache;

        public TwilioPhoneInfoRetriever(IOptions<DynamoDBTablesConfig> dynamoConfig,
            IOptions<EnvironmentConfig> envConfig, IOptions<TwilioConfig> twilioConfig, 
            ISecretStoreReader secureStoreReader, IMemoryCache memCache,
            ILogger<TwilioPhoneInfoRetriever> logger) : base(secureStoreReader, memCache)
        {

            _twilioConfig = twilioConfig?.Value ??
                            throw new ArgumentNullException(
                                $"{nameof(twilioConfig)}");

            _dynamoTableName = dynamoConfig?.Value?.UserTable ??
                               throw new ArgumentNullException(
                                   $"{nameof(dynamoConfig)}", "UserTable is null or missing the UserTable property");

            if (string.IsNullOrWhiteSpace(_dynamoTableName))
                throw new ArgumentNullException($"{nameof(dynamoConfig)}", "UserTable property is empty");

            _endpoint = envConfig?.Value?.Region ??
                        throw new ArgumentNullException($"{nameof(envConfig)}", "RegionEndpoint property not set");

            if (string.IsNullOrWhiteSpace(_twilioConfig.LiveCredentials))
                throw new ArgumentNullException($"{nameof(twilioConfig)}", "Twilio configuration does not contain LiveCredentials");

            if (string.IsNullOrWhiteSpace(_twilioConfig.TestCredentials))
                throw new ArgumentNullException($"{nameof(twilioConfig)}", "Twilio configuration does not contain TestCredentials");

            _memCache = memCache ?? throw new ArgumentNullException($"{nameof(memCache)}");

            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");

        }


        /// <summary>
        /// Retrieves the phone number info, including the phone type and mobile carrier.
        /// </summary>
        /// <remarks>
        /// This attempts to retrieve the phone number from cache, then from the database, and then finally from Twilio.
        /// </remarks>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public async Task<DataPhone> GetPhoneInfoAsync(string phoneNumber)
        {

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException($"{nameof(phoneNumber)} cannot be null or empty");


            string formattedPhoneNumber = PhoneUtility.FormatE164(phoneNumber);

            // Attempt to get the phone number from DynamoDB. If the user previously granted consent,
            // then the phone number will be present.

            DataPhone retPhone = await GetDynamoDbPhoneInfoAsync(phoneNumber);

            if(retPhone==null)
                retPhone = GetPhoneFromCache(phoneNumber);

            if (retPhone == null)
            {
                // Phone number not found in database.

                retPhone = await GetTwilioPhoneInfoAsync(formattedPhoneNumber);
                retPhone.PhoneNumber = formattedPhoneNumber;
                retPhone.SystemStatus = SystemPhoneStatus.SendPermitted;

                _logger.LogInformation(
                    $"Phone number info for phone REDACTED retrieved from Twilio account. Phone type: {retPhone.Type}");

                // Save the phone number to the cache, but not to the dynamo db table. 
                // Do not save to the dynamodb table until after the user has granted consent.
                SavePhoneInCache(retPhone);
            }
            else
            {
                _logger.LogInformation($"Phone number retrieved from memory cache");
            }

            return retPhone;
        }

        private DataPhone GetPhoneFromCache(string phoneNumber)
        {


            string cacheKey = GetCacheKey(phoneNumber);
            DataPhone retPhone = _memCache.Get<DataPhone>(cacheKey);
            return retPhone;
        }

        private void SavePhoneInCache(DataPhone phoneInfo)
        {
            string key = GetCacheKey(phoneInfo.PhoneNumber);

            _memCache.Set(key, phoneInfo, new TimeSpan(0, 4, 0));
        }


        private string GetCacheKey(string phoneNumber)
        {

            return $"{PHONECACHEKEYPREFIX}:{phoneNumber}";
        }


        private async Task<DataPhone> GetTwilioPhoneInfoAsync(string phoneNumber)
        {
            DataPhone retPhone = null;

            TwilioCredentials creds = await GetCredentialsAsync(_twilioConfig.LiveCredentials);

            if (creds == null)
                throw new Exception(
                    $"Could not get Twilio credentials for {_twilioConfig.LiveCredentials} when sending message");

            var client = new TwilioRestClient(creds.AccountSid, creds.Token, "us1");
            bool isVerfied = false;

            var type = new List<string> {"carrier"};
            PhoneNumberResource phoneResponse = null;
            try
            {
                phoneResponse = await PhoneNumberResource.FetchAsync(
                    type: type,
                    pathPhoneNumber: new Twilio.Types.PhoneNumber(phoneNumber),
                    client: client

                );
                isVerfied = true;
            }
            catch (Twilio.Exceptions.ApiException ex)
            {

                if (ex.Status == 404)
                {
                    retPhone = new DataPhone();

                    retPhone.PhoneNumber = phoneNumber;
                    retPhone.Type = PhoneTypeEnum.Invalid;
                    retPhone.PhoneService = PhoneUtility.TwilioService;
                    retPhone.CanGetSmsMessage = false;
                    retPhone.CreateDate = DateTime.UtcNow;
                    
                    // the phone number is not found. It is invalid.
                    _logger.LogWarning(
                        $"Phone number REDACTED could not be resolved with Twilio lookups with Twilio config");

                }
                else
                {

                    _logger.LogError(ex, $"Error validating phone number REDACTED using Twilio config");
                }
            }



            if (phoneResponse != null)
            {
                retPhone = new DataPhone();
                retPhone.PhoneService = PhoneUtility.TwilioService;

                Uri phoneUri = phoneResponse.Url;
                retPhone.Url = phoneUri.ToString();
                retPhone.IsVerified = isVerfied;


                Dictionary<string, string> carrierInfo = phoneResponse.Carrier;

                retPhone.CountryCode = phoneResponse.CountryCode;
                retPhone.NationalFormat = phoneResponse.NationalFormat;
                retPhone.PhoneNumber = phoneResponse.PhoneNumber.ToString();

                if (carrierInfo != null)
                {
                    if (carrierInfo.ContainsKey(TWILIO_CARRIERNAME))
                        retPhone.CarrierName = carrierInfo[TWILIO_CARRIERNAME];

                    if (carrierInfo.ContainsKey(TWILIO_COUNTRYCODE))
                        retPhone.CarrierCountryCode = carrierInfo[TWILIO_COUNTRYCODE];

                    if (carrierInfo.ContainsKey(TWILIO_ERRORCODE))
                        retPhone.CarrierErrorCode = carrierInfo[TWILIO_ERRORCODE];

                    if (carrierInfo.ContainsKey(TWILIO_NETWORKCODE))
                        retPhone.CarrierCountryCode = carrierInfo[TWILIO_NETWORKCODE];

                    if (carrierInfo.ContainsKey(TWILIO_PHONETYPE))
                    {
                        string phoneTypeText = carrierInfo[TWILIO_PHONETYPE];
                        // If the phone type text is null, then 
                        // the carrier information could not be found or is not supported.
                        // This will happen with international numbers and 1-8xx toll free numbers.

                        if (!string.IsNullOrWhiteSpace(phoneTypeText))
                        {

                            if (phoneTypeText.Equals("mobile", StringComparison.OrdinalIgnoreCase))
                                retPhone.Type = PhoneTypeEnum.Mobile;
                            else if (phoneTypeText.Equals("landline", StringComparison.OrdinalIgnoreCase))
                                retPhone.Type = PhoneTypeEnum.Landline;
                            else if (phoneTypeText.Equals("voip", StringComparison.OrdinalIgnoreCase))
                                retPhone.Type = PhoneTypeEnum.Voip;
                            else
                                retPhone.Type = PhoneTypeEnum.Other;
                        }
                        else
                        {

                            if (!string.IsNullOrWhiteSpace(retPhone.CarrierErrorCode))
                            {
                                if (retPhone.CarrierErrorCode.Equals(PhoneUtility
                                    .Twilio_Error_UnprovisionedOrOutOfCoverage))
                                {

                                    retPhone.Type = PhoneTypeEnum.OutOfCoverage;
                                }
                                else
                                    retPhone.Type = PhoneTypeEnum.Other;
                            }
                            else
                                retPhone.Type = PhoneTypeEnum.Other;

                        }
                    }



                    retPhone.CanGetSmsMessage = PhoneUtility.PhoneSupportsSms(retPhone.Type);
                }

            }

            return retPhone;
        }

        private async Task<DataPhone> GetDynamoDbPhoneInfoAsync(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException($"{nameof(phoneNumber)} cannot be null or empty");

            DataPhone retPhone = null;

            using (var dbClient = new AmazonDynamoDBClient(_endpoint))
            {
                QueryRequest queryRequest = new QueryRequest
                {
                    TableName = _dynamoTableName, 
                    IndexName =  "firstGSI",
                    KeyConditionExpression = "gsk1 =:v_phoneNumber and sortKey =:v_sortKey",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        {":v_phoneNumber", new AttributeValue() {S =phoneNumber}},
                        {":v_sortKey", new AttributeValue() {S = "phone"}}
                    },
                    ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL,
                    ConsistentRead = false
                    
                };


                QueryResponse resp;
                try
                {
                    resp = await dbClient.QueryAsync(queryRequest);
                }
                catch (ResourceNotFoundException tableNotFound)
                {
                    _logger.LogError(tableNotFound,
                        $"Table dynamodb table {_dynamoTableName} not found when trying to get phone info with a phone number");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error getting phoneInfo from dynamo db using the phone number");
                    throw;
                }

                if (resp.Count >0)
                {
                    retPhone = (DataPhone)resp.Items[0];
                    _logger.LogInformation($"Retrieved phone with hashKey {retPhone.Id.Value} consuming {resp.ConsumedCapacity.ReadCapacityUnits} read capacity units");
                    if(resp.Count>1)
                        _logger.LogInformation($"{resp.Count} records returned for phone number. Took first number with id {retPhone.Id.Value}");
                }



            }

            return retPhone;
        }

        public async Task SaveDatabasePhoneInfoAsync(DataPhone phoneInfo)
        {

            if (phoneInfo.CreateDate.Equals(default(DateTime)))
                phoneInfo.CreateDate = DateTime.UtcNow;


            if (!phoneInfo.Id.HasValue)
            {
                phoneInfo.Id = Guid.NewGuid();
            }

            using (var dbClient = new AmazonDynamoDBClient(_endpoint))

            {
                PutItemRequest putItem = new PutItemRequest();
                putItem.TableName = _dynamoTableName;

                putItem.Item = (Dictionary<string, AttributeValue>)phoneInfo;


                putItem.ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL;

                PutItemResponse resp = null;

                try
                {
                    resp = await dbClient.PutItemAsync(putItem);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error saving phone record with hashKey {phoneInfo.Id}");
                    throw;
                }



                _logger.LogInformation(
                    $"Saved phone with hash key {phoneInfo.Id} to dynamodb: Used {resp.ConsumedCapacity.WriteCapacityUnits} write capacity units");
            }
        }





        //private string GetCacheKey(string phoneNumber)
        //{
        //    return $"{PHONECACHEKEYPREFIX}:{phoneNumber}";
        //}




        /// <summary>
        /// Retrieve the phone info from the database.
        /// </summary>
        /// <param name="environment">Executing environment.</param>
        /// <param name="phoneId">Phone ID of the </param>
        /// <returns></returns>
        public async Task<DataPhone> GetPhoneInfoAsync(Guid phoneId)
        {


            if (phoneId.Equals(default(Guid)))
                throw new ArgumentException($"{nameof(phoneId)} is not set");


            DataPhone retPhone = null;

            string idtext = phoneId.ToString();


            using (var dbClient = new AmazonDynamoDBClient(_endpoint))
            {
                QueryRequest queryRequest = new QueryRequest
                {
                    TableName = _dynamoTableName,
                    KeyConditionExpression = "id =:v_id and sortKey =:v_sortKey",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        {":v_id", new AttributeValue() {S =idtext}},
                        {":v_sortKey", new AttributeValue() {S = "phone"}}
                    },
                    ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL,
                    ConsistentRead = true
                };

                Stopwatch phonequerytime = new Stopwatch();
                QueryResponse resp;
                try
                {
                    phonequerytime.Start();
                    resp = await dbClient.QueryAsync(queryRequest);
                }
                catch (ResourceNotFoundException tableNotFound)
                {
                    _logger.LogError(tableNotFound,
                        $"Table dynamodb table {_dynamoTableName} not found when trying to get phone with hashKey {idtext}");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error getting phone from dynamo db with hash key {idtext}");
                    throw;
                }
                finally
                {
                    phonequerytime.Stop();
                }

                if (resp.Count == 1)
                {
                    retPhone = (DataPhone)resp.Items[0];
                    _logger.LogInformation($"Retrieved phone with id {idtext} consuming {resp.ConsumedCapacity.ReadCapacityUnits} read capacity units in {phonequerytime.ElapsedMilliseconds}ms");

                }
                else
                {
                    _logger.LogInformation($"Phone with id {idtext} not found. Consumed {resp.ConsumedCapacity.ReadCapacityUnits} read capacity units in {phonequerytime.ElapsedMilliseconds}ms");

                }



            }

            return retPhone;
        }


    }
}
