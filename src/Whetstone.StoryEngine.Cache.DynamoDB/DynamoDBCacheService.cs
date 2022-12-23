using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.XRay.Recorder.Core;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nito.AsyncEx;
using Whetstone.StoryEngine.Cache.Manager;
using Whetstone.StoryEngine.Cache.Models;


namespace Whetstone.StoryEngine.Cache.DynamoDB
{

    public class DynamoDBCacheService : IDistributedCache
    {
        private readonly ICacheTtlManager _cacheTtlManager;

        private readonly string _tableName;

        private readonly IAmazonDynamoDB _dbClient;

        private ILogger<DynamoDBCacheService> _logger;

        private readonly int _engineTimeout;

        public DynamoDBCacheService(IOptions<DynamoDBCacheConfig> cacheConfig, IAmazonDynamoDB dynamoDbClient, ICacheTtlManager cacheTtlManager, ILogger<DynamoDBCacheService> cacheLogger)
        {

            _cacheTtlManager = cacheTtlManager ?? throw new ArgumentNullException(nameof(cacheTtlManager));


            if(cacheConfig?.Value==null)
                throw new ArgumentNullException(nameof(cacheConfig.Value));

            if (string.IsNullOrWhiteSpace(cacheConfig?.Value?.TableName))
                throw new ArgumentNullException(nameof(cacheConfig));

            _tableName = cacheConfig.Value.TableName;

            _engineTimeout = cacheConfig.Value.Timeout;

            _logger = cacheLogger ?? throw new ArgumentNullException(nameof(cacheLogger));

            _dbClient = dynamoDbClient ?? throw new ArgumentNullException(nameof(dynamoDbClient));
        }

        #region get item

        public byte[] Get(string key)
        {
            return AsyncContext.Run(async () => await GetAsync(key));
        }

        public async Task<byte[]> GetAsync(string key)
        {
            CancellationToken token = new CancellationToken();

            return await GetAsync(key, token);
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {

            CacheItem cacheItem = await GetCacheItemAsync(key, token);

            if (cacheItem == null)
                return null;

            if (cacheItem.Ttl < _cacheTtlManager.ToUnixTime(DateTime.UtcNow))
            {
                await RemoveAsync(cacheItem.CacheKey, token);
                return null;
            }

            return cacheItem.Value;
        }

        private async Task<CacheItem> GetCacheItemAsync(string key, CancellationToken token = default)
        {
            return await AWSXRayRecorder.Instance.TraceMethodAsync($"GetCacheItemAsync",
                async () => await GetCacheItemInternalAsync(key, token));
        }

        private async Task<CacheItem> GetCacheItemInternalAsync(string key, CancellationToken token)
        {
            AWSXRayRecorder.Instance.AddMetadata("key-get", key);
            CacheItem cacheItem = default(CacheItem);


            var keyDictionary = new Dictionary<string, AttributeValue>();
            keyDictionary.Add(ConversionExtensions.FIELD_HASHKEY, new AttributeValue() { S = key });


            GetItemRequest queryRequest = new GetItemRequest
            {
                TableName = _tableName,
                Key = keyDictionary,
                ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL,
                ConsistentRead = false
            };


            GetItemResponse resp;

            Stopwatch getItemTimer = new Stopwatch();
            // Stopwatch getItemTimer = new Stopwatch();
            bool isError = true;

            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(_engineTimeout);


                _logger.LogDebug($"About to get cache key {key} from DynamoDB table {_tableName}");


                getItemTimer.Start();
                resp = await _dbClient.GetItemAsync(queryRequest, token);
                getItemTimer.Stop();


                if (resp?.Item != null)
                    cacheItem = resp.Item.ToCacheItem();

                isError = false;

            }
            catch (ObjectDisposedException)
            {
                _logger.LogError($"Object disposed getting cache key {key} from table {_tableName}");
            }
            catch (TaskCanceledException)
            {
                _logger.LogError($"Task cancelled getting cache key {key} from table {_tableName}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting cache key {key} from table {_tableName}");
            }
            finally
            {
                getItemTimer.Stop();
                if (isError)
                    _logger.LogDebug($"Errored out getting {key} from DynamoDB table {_tableName}: {getItemTimer.ElapsedMilliseconds}");
                else
                {
                    string isCacheItemNull = cacheItem is null ? "null" : "not null";
                    _logger.LogDebug($"Cache item is {isCacheItemNull}. Time to get cache key {key} from DynamoDB table {_tableName}: {getItemTimer.ElapsedMilliseconds}");
                }

            }

            return cacheItem;


        }


        #endregion get item


        #region set item
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            AsyncContext.Run(async () => await SetAsync(key, value, options));
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            CancellationToken token = new CancellationToken();
            await SetAsync(key, value, options, token);
        }


        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {

            await AWSXRayRecorder.Instance.TraceMethodAsync($"SetCacheItemAsync",
              async () => await SaveCacheItemInternal(key, value, options, token));
        }

        private async Task SaveCacheItemInternal(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {

            AWSXRayRecorder.Instance.AddAnnotation("key-set", key);

            CacheItem cacheItem = InitializeCacheItem(key, value, options);


            PutItemRequest putItem = new PutItemRequest
            {
                TableName = _tableName,
                Item = cacheItem.ToDynamoDictionary()
            };

            Stopwatch saveTimer = new Stopwatch();

            saveTimer.Start();
            try
            {
                _logger.LogDebug($"About to save item with {cacheItem.CacheKey} to DynamoDB table {_tableName}");
                await _dbClient.PutItemAsync(putItem, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving item {cacheItem.CacheKey} to DynamoDB table {_tableName}");
                throw;
            }
            finally
            {
                saveTimer.Stop();
                _logger.LogDebug($"Time to save item with {cacheItem.CacheKey} to DynamoDB table {_tableName}: {saveTimer.ElapsedMilliseconds}");
            }


        }


        private CacheItem InitializeCacheItem(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            var cacheItem = new CacheItem();

            cacheItem.CacheKey = key;
            cacheItem.Value = value;
            cacheItem.CacheOptions = _cacheTtlManager.ToCacheOptions(options);
            cacheItem.Ttl = _cacheTtlManager.ToTtl(cacheItem.CacheOptions);

            return cacheItem;
        }


        #endregion set item


        #region refresh item

        public void Refresh(string key)
        {
            AsyncContext.Run(async () => await RefreshAsync(key));

        }

        public async Task RefreshAsync(string key)
        {
            CancellationToken token = default;
            await RefreshAsync(key, token);
        }

        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            await AWSXRayRecorder.Instance.TraceMethodAsync($"RefreshCacheItemAsync",
                 async () => await RefreshCacheItemInternal(key, token));
        }

        private async Task RefreshCacheItemInternal(string key, CancellationToken token = default)
        {
            CacheItem cacheItem = await GetCacheItemAsync(key, token);

            AWSXRayRecorder.Instance.AddAnnotation("key-refresh", key);

            if (cacheItem != null)
            {
                if (cacheItem.CacheOptions.Type == CacheExpiryType.Sliding)
                {
                    // Define attribute updates
                    Dictionary<string, AttributeValueUpdate> updates = new Dictionary<string, AttributeValueUpdate>();
                    // Update item's Setting attribute
                    updates[ConversionExtensions.FIELD_TTL] = new AttributeValueUpdate()
                    {
                        Action = AttributeAction.PUT,
                        Value = new AttributeValue { N = _cacheTtlManager.ToTtl(cacheItem.CacheOptions).ToString() }
                    };
                    // R

                    UpdateItemRequest updateRequest = new UpdateItemRequest
                    {
                        TableName = _tableName,
                        Key = GetTableKey(cacheItem),
                        AttributeUpdates = updates
                    };

                    Stopwatch updateTimer = new Stopwatch();
                    UpdateItemResponse resp;

                    try
                    {
                        _logger.LogDebug($"About to update item to slide expiration with {key} in DynamoDB table {_tableName}");
                        updateTimer.Start();
                        resp = await _dbClient.UpdateItemAsync(updateRequest, token);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error sliding expiration time of {key} in DynamoDB table {_tableName}");
                        throw;
                    }
                    finally
                    {
                        updateTimer.Stop();
                        _logger.LogDebug($"Time to update item with {key} with sliding expiration in DynamoDB table {_tableName}: {updateTimer.ElapsedMilliseconds}");
                    }

                }
            }

        }

        #endregion

        #region remove item

        public async Task RemoveAsync(string key)
        {
            CancellationToken token = default;
            await RemoveAsync(key, token);
        }

        public void Remove(string key)
        {
            AsyncContext.Run(async () => await RemoveAsync(key));
        }
        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            await AWSXRayRecorder.Instance.TraceMethodAsync($"RemoveCacheItemAsync",
                 async () => await RemoveCacheItemInternal(key, token));
        }

        private async Task RemoveCacheItemInternal(string key, CancellationToken token)
        {

            AWSXRayRecorder.Instance.AddAnnotation("key-remove", key);
            DeleteItemRequest deleteRequest = new DeleteItemRequest
            {
                TableName = _tableName,
                Key = GetTableKey(key),
                ReturnConsumedCapacity = ReturnConsumedCapacity.TOTAL,
            };

            Stopwatch deleteTimer = new Stopwatch();
            DeleteItemResponse resp;

            try
            {
                _logger.LogDebug($"About to delete item with {key} from DynamoDB table {_tableName}");
                deleteTimer.Start();
                resp = await _dbClient.DeleteItemAsync(deleteRequest, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting item {key} from DynamoDB table {_tableName}");
                throw;

            }
            finally
            {
                deleteTimer.Stop();
                _logger.LogDebug($"Time to delete item with {key} from DynamoDB table {_tableName}: {deleteTimer.ElapsedMilliseconds}");
            }


        }
        #endregion


        #region helper functions
        private Dictionary<string, AttributeValue> GetTableKey(CacheItem item)
        {
            var itemKey = new Dictionary<string, AttributeValue>();

            itemKey.Add(ConversionExtensions.FIELD_HASHKEY, new AttributeValue { S = item.CacheKey });

            return itemKey;
        }

        private Dictionary<string, AttributeValue> GetTableKey(string key)
        {
            var itemKey = new Dictionary<string, AttributeValue>();

            itemKey.Add(ConversionExtensions.FIELD_HASHKEY, new AttributeValue { S = key });

            return itemKey;
        }

        #endregion
    }
}
