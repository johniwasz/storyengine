using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Repository.Phone;

namespace Whetstone.StoryEngine.Repository.Phone
{
    public class SmsConsentDynamoDBRepository : ISmsConsentRepository
    {


        internal readonly ILogger<SmsConsentDynamoDBRepository> _logger;

        private readonly string _dynamoTableName;
        private readonly RegionEndpoint _endpoint;

        public SmsConsentDynamoDBRepository(IOptions<DynamoDBTablesConfig> dynamoConfig, IOptions<EnvironmentConfig> envConfig, ILogger<SmsConsentDynamoDBRepository> logger)
        {
            
            _dynamoTableName = dynamoConfig?.Value?.UserTable ??
                               throw new ArgumentNullException(
                                   nameof(dynamoConfig));

            if (string.IsNullOrWhiteSpace(_dynamoTableName))
                throw new ArgumentNullException(nameof(dynamoConfig), "UserTable property is empty");

            _endpoint = envConfig?.Value?.Region ??
                        throw new ArgumentException($"{nameof(envConfig)}", "Missing or RegionEndpoint property not set");



            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        
        public async Task<UserPhoneConsent> GetConsentAsync(Guid consentId)
        {

            if(consentId ==default(Guid))
                throw new ArgumentException($"{nameof(consentId)} is not set");

            UserPhoneConsent phoneConsent = null;

            using (var dbClient = new AmazonDynamoDBClient(_endpoint))
            {
                QueryRequest queryRequest = new QueryRequest
                {
                    TableName = _dynamoTableName,
                    IndexName = "firstGSI",
                    KeyConditionExpression = "gsk1 =:v_consentId",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        {":v_consentId", new AttributeValue() {S =consentId.ToString()}}
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
                        $"Table dynamodb table {_dynamoTableName} not found when trying to get consent id {consentId}");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error getting consentid {consentId} from dynamo db");
                    throw;
                }

                if (resp.Count == 1)
                {
                    phoneConsent = (UserPhoneConsent)resp.Items[0];
                    _logger.LogInformation($"Retrieved consentId {consentId} consuming {resp.ConsumedCapacity.ReadCapacityUnits} read capacity units");

                }
            }

            return phoneConsent;
        }

        public async Task<UserPhoneConsent> GetConsentAsync(string consentName, Guid phoneId)
        {
            if(string.IsNullOrWhiteSpace(consentName))
                throw new ArgumentException($"{nameof(consentName)} is not set");

            if (phoneId == default(Guid))
                throw new ArgumentException($"{nameof(phoneId)} is not set");


            UserPhoneConsent phoneConsent = null;

            string consentPrefix = UserPhoneConsent.GetSortKeyPrefix(consentName, phoneId);

            using (var dbClient = new AmazonDynamoDBClient(_endpoint))
            {


                QueryRequest queryRequest = new QueryRequest
                {
                    TableName = _dynamoTableName,

                   //    IndexName = "firstGSI",
                    KeyConditionExpression = "id=:v_phoneId and sortKey=:v_sortKey",
                    // KeyConditionExpression = "id=:v_userId AND begins_with(sortKey,:v_consentprefix)",

                    
                    FilterExpression = "gsk1=:v_consentName",

                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        {":v_phoneId", new AttributeValue() {S =phoneId.ToString()}},
                        {":v_sortKey", new AttributeValue() {S ="consentrecord"} },
                        {":v_consentName", new AttributeValue() {S =consentName} }
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
                        $"Table dynamodb table {_dynamoTableName} not found when trying to get consent record for  phoneid {phoneId}, and consent {consentName}");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error getting consent for phoneid {phoneId}, and consent {consentName} from dynamo db");
                    throw;
                }

                if (resp.Count > 0)
                {
                    string consentId = resp.Items[0]["consentId"].S;

                    Guid consentIdGuid = Guid.Parse(consentId);

                    phoneConsent = await GetConsentAsync(consentIdGuid);

                    _logger.LogInformation($"Retrieved  consent for phoneid {phoneId}, and consent {consentName}  consuming {resp.ConsumedCapacity.ReadCapacityUnits} read capacity units");


                    // Get the consentId.

                }
            }

            return phoneConsent;
        }

        public async Task<UserPhoneConsent> SaveConsentAsync(UserPhoneConsent phoneConsent)
        {
            if (phoneConsent == null)
                throw new ArgumentException($"{nameof(phoneConsent)} cannot be null or empty");



            if (!phoneConsent.SmsConsentDate.HasValue)
                phoneConsent.SmsConsentDate = DateTime.UtcNow;


            // Save the intersection of the user record with the phone consent.

            // Each consent record is a new audit. This is an insert-always scenario.
            // The consent record in DynamoDB in an intersection of the user and the phone number.
            // It will be stored using the sort key of the user record.
            string sortKey = null;
            using (var dbClient = new AmazonDynamoDBClient(_endpoint))

            {
                double writeUnit = 0;
                try
                {

                    PutRequest putReq = new PutRequest();


                    PutItemRequest putItem = new PutItemRequest();
                    putItem.TableName = _dynamoTableName;
                    putReq.Item = (Dictionary<string, AttributeValue>)phoneConsent;


                    if (putReq.Item != null)
                    {
                        if (putReq.Item.ContainsKey("sortKey"))
                            sortKey = putReq.Item["sortKey"].S;
                    }

                    putItem.ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL;

                    BatchWriteItemResponse resp = null;

                    WriteRequest writeReq = new WriteRequest(putReq);

                    BatchWriteItemRequest batchRequest = new BatchWriteItemRequest();
                    batchRequest.ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL;
                    batchRequest.RequestItems = new Dictionary<string, List<WriteRequest>>();



                    List<WriteRequest> writeRequests = new List<WriteRequest>
                    {
                        writeReq
                    };

                    // Apply a consent and key mapping
                    Dictionary<string, AttributeValue> abbreviatedConsent = new Dictionary<string, AttributeValue>();
                    abbreviatedConsent.Add("id", new AttributeValue(phoneConsent.PhoneId.ToString()));
                    abbreviatedConsent.Add("sortKey", new AttributeValue("consentrecord"));
                    abbreviatedConsent.Add("gsk1", new AttributeValue(phoneConsent.Name));
                    abbreviatedConsent.Add("consentDate", new AttributeValue(phoneConsent.SmsConsentDate.Value.ToString(CultureInfo.InvariantCulture)));
                    abbreviatedConsent.Add("consentId", new AttributeValue(phoneConsent.Id.Value.ToString()));

                    PutRequest shortConsentReq = new PutRequest(abbreviatedConsent);
                    
                    WriteRequest abbvReq = new WriteRequest(shortConsentReq);
                    
                    writeRequests.Add(abbvReq);

                    batchRequest.RequestItems.Add(_dynamoTableName, writeRequests);

                    try
                    {
                        resp = await dbClient.BatchWriteItemAsync(batchRequest);
                        //resp = await dbClient.PutItemAsync(putItem);
                    }
                    catch (Exception ex)
                    {

                        if (!string.IsNullOrWhiteSpace(sortKey))
                            _logger.LogError(ex,
                                $"Error saving consent record with sort key {sortKey} and user {phoneConsent.TitleClientUserId}");
                        else
                            _logger.LogError(ex,
                                $"Error saving consent record for user {phoneConsent.TitleClientUserId}");


                        throw;
                    }


                    foreach (var capacity in resp.ConsumedCapacity)
                    {
                        writeUnit += capacity.WriteCapacityUnits;
                    }


                    _logger.LogInformation(
                        $"Saved phone consent for user with hashKey {phoneConsent.TitleClientUserId} for consent {phoneConsent.Name} and phone id {phoneConsent.PhoneId} to dynamodb: Used {writeUnit} write capacity units");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error saving consent {phoneConsent.Id} and consent name {phoneConsent.Name} for user {phoneConsent.TitleClientUserId} and phone {phoneConsent.PhoneId} on request {phoneConsent.EngineRequestId}");
                    throw;
                }
            }


            return phoneConsent;
        }


     

    }
}
