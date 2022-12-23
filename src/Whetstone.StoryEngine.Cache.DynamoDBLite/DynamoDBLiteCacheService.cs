using Amazon;
using Amazon.DynamoDb;
using Carbon.Data;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Cache.Manager;
using Whetstone.StoryEngine.Cache.Models;

namespace Whetstone.StoryEngine.Cache.DynamoDBLite
{
    public class DynamoDBLiteCacheService : IDistributedCache
    {

        private readonly ICacheTtlManager _cacheTtlManager;

        private readonly RegionEndpoint _endpoint;

        private readonly int _maxCacheTimeout;

        private readonly string _tableName;
        public DynamoDBLiteCacheService(string tableName, int? maxCacheTimeout, RegionEndpoint endpoint, ICacheTtlManager cacheTtlManager)
        {
            // _dynamoDbContext = dynamoDbContext;
            //_dynamoClient = dynamoClient ?? throw new ArgumentNullException($"{nameof(dynamoClient)} cannot be null");

            _cacheTtlManager = cacheTtlManager ?? throw new ArgumentNullException($"{nameof(cacheTtlManager)} cannot be null");

            _endpoint = endpoint;

            _tableName = tableName;

            _maxCacheTimeout = maxCacheTimeout.GetValueOrDefault(1000);
        }
        public byte[] Get(string key)
        {
            throw new NotImplementedException();
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

        private async Task<CacheItem> GetCacheItemAsync(string key, CancellationToken token)
        {

            CacheItem cacheItem = null;


            IAwsCredential cred = new AwsCredential("AKIA5VYLVSRLYOFWURGN", "L+NDnsTpc5DyLlY8Kc7cW3gLmY1n9/KyK9p1ExN+");

            DynamoDbClient dbClient = new DynamoDbClient(AwsRegion.USEast1, cred);

            List<KeyValuePair<string, object>> keys = new List<KeyValuePair<string, object>>();

            keys.Add(new KeyValuePair<string, object>("CacheKey", key));



            GetItemRequest req = new GetItemRequest(_tableName, keys);
            try
            {
                GetItemResult result = await dbClient.GetItemAsync(req);
                cacheItem = result.Item.ToCacheItem();
            }
            catch(Exception ex)
            {

                Debug.WriteLine(ex);
            }
            

            return cacheItem;
        }

        public void Refresh(string key)
        {
            throw new NotImplementedException();
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }
}
