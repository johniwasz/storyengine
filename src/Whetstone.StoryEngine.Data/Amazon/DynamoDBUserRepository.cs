using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.XRay.Recorder.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Data;

namespace Whetstone.StoryEngine.Data.Amazon
{
    public class DynamoDBUserRepository : UserRepositoryBase, IStoryUserRepository
    {


        private readonly ILogger<DynamoDBUserRepository> _dataLogger;


        private readonly string _tableName;

        private IAmazonDynamoDB _dbClient;

        public DynamoDBUserRepository(IOptions<DynamoUserTableConfig> userTableOptions, IAmazonDynamoDB dbClient, ILogger<DynamoDBUserRepository> logger)
        {
            DynamoUserTableConfig tableConfig = userTableOptions?.Value ?? throw new ArgumentNullException(nameof(userTableOptions), "Value cannot be null");

            string userTableName = tableConfig.TableName;

            if (string.IsNullOrWhiteSpace(userTableName))
                throw new ArgumentNullException(nameof(userTableOptions), "TableName property cannot be null or empty");

            _tableName = userTableName;

            _dbClient = dbClient ?? throw new ArgumentNullException(nameof(dbClient));

            _dataLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<DataTitleClientUser> GetUserAsync(StoryRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));


            DataTitleClientUser retUser = null;
            if (!string.IsNullOrWhiteSpace(request.UserId))
            {

                //await AWSXRayRecorder.Instance.TraceMethodAsync($"GetUserAsync({request.UserId})",

                await AWSXRayRecorder.Instance.TraceMethodAsync($"GetUserDynamoDBAsync",
                    async () =>
                    {

                        string hashCode = request.GetUserHashKey();

                        QueryRequest queryRequest = new QueryRequest
                        {
                            TableName = _tableName,
                            Limit = 1,
                            IndexName = "firstGSI",
                            KeyConditionExpression = "gsk1 =:v_hashKey and sortKey =:v_sortKey",
                            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                        {
                            {":v_hashKey", new AttributeValue() {S = hashCode}},
                            {":v_sortKey", new AttributeValue() {S = "user"}}
                        },
                            ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL,
                            ConsistentRead = false
                        };

                        Stopwatch userReadTimer = new Stopwatch();
                        QueryResponse resp;
                        try
                        {
                            _dataLogger.LogDebug(
                                $"Retrieving user with hash key {hashCode} from dynamodb.");

                            userReadTimer.Start();
                            resp = await _dbClient.QueryAsync(queryRequest);
                        }
                        catch (ResourceNotFoundException tableNotFound)
                        {
                            _dataLogger.LogError(tableNotFound,
                                $"Table dynamoDB table {_tableName} not found when trying to get user with hashKey {hashCode}");
                            throw;
                        }
                        catch (AmazonDynamoDBException dbEx)
                        {
                            _dataLogger.LogError(dbEx, $"Error getting user from dynamoDB with hash key {hashCode}");
                            throw;

                        }
                        catch (Exception ex)
                        {
                            _dataLogger.LogError(ex, $"Error getting user from dynamoDB with hash key {hashCode}");
                            throw;
                        }
                        finally
                        {
                            userReadTimer.Stop();

                        }

                        if (resp.Count == 1)
                        {
                            retUser = (DataTitleClientUser)resp.Items[0];
                            _dataLogger.LogInformation(
                                $"Retrieved user with hash key {hashCode} from dynamodb. Consumed {resp.ConsumedCapacity.ReadCapacityUnits} read capacity units in {userReadTimer.ElapsedMilliseconds}ms");

                        }
                        else
                        {
                            _dataLogger.LogInformation(
                                $"User with {hashCode} not found in dynamodb. Query consumed {resp.ConsumedCapacity.ReadCapacityUnits} read capacity units in {userReadTimer.ElapsedMilliseconds}ms");

                        }


                    });
            }


            if (retUser == null)
            {
                // Create new user and save to the database.
                retUser = BootstrapUser(request);
                await SaveUserAsync(retUser);
            }

            else
            {
                // Last access date is when the request came in.
                // this and the bootstrap user logic should be the only
                // places this value is set.
                retUser.LastAccessedDate = request.RequestTime;
            }


            return retUser;
        }



        public async Task SaveUserAsync(DataTitleClientUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (user.LastAccessedDate == default)
                user.LastAccessedDate = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(user.HashKey))
                user.HashKey = user.GenerateHashKey();

            string hashKey = user.HashKey;


            //await AWSXRayRecorder.Instance.TraceMethodAsync($"SaveUserAsync({hashKey})",
            await AWSXRayRecorder.Instance.TraceMethodAsync($"SaveUserDynamoDBAsync",
            async () =>
            {
                PutItemRequest putItem = new PutItemRequest
                {
                    TableName = _tableName,
                    Item = (Dictionary<string, AttributeValue>)user,
                    // indicate the item is a user in the sort key.
                    ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL
                };

                PutItemResponse resp;

                try
                {
                    Stopwatch saveTimer = new Stopwatch();

                    _dataLogger.LogDebug(
                        $"Saving user with hash key {hashKey} to dynamodb.");

                    saveTimer.Start();
                    resp = await _dbClient.PutItemAsync(putItem);
                    saveTimer.Stop();


                    _dataLogger.LogInformation($"Saved user with hash key {hashKey} to dynamodb: Used {resp.ConsumedCapacity.WriteCapacityUnits} write capacity units in {saveTimer.ElapsedMilliseconds}ms");
                }
                catch (Exception ex)
                {
                    _dataLogger.LogError(ex, $"Error saving user record with hashKey {hashKey}");
                    throw;
                }
            });
        }
    }
}
