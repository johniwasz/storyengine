using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.XRay.Recorder.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Repository.Amazon
{
    public class WhetstoneDynamoDbClient : AmazonDynamoDBClient
    {
        private readonly ILogger<WhetstoneDynamoDbClient> _logger;

        private readonly AmazonDynamoDBConfig _dbConfig;

        private readonly int _timeOut;

        public WhetstoneDynamoDbClient(IOptions<AmazonDynamoDBConfig> dbConfig,  ILogger<WhetstoneDynamoDbClient> logger) : base(dbConfig?.Value)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _dbConfig = dbConfig?.Value ?? throw new ArgumentNullException(nameof(dbConfig));

            _timeOut = (int) (_dbConfig.Timeout?.TotalMilliseconds).GetValueOrDefault(2000);
        }

        public override async Task<GetItemResponse> GetItemAsync(GetItemRequest request, CancellationToken cancellationToken = default)
        {
            return await ThrottleManager.ThrottleRequestWithRetries<GetItemRequest, GetItemResponse>(request, 
                (requestItem, cancelToken) => base.GetItemAsync(requestItem, cancelToken), 
                _timeOut, 
                _dbConfig.MaxErrorRetry, 
                _logger, 
                cancellationToken);
        }


        public override async Task<UpdateItemResponse> UpdateItemAsync(UpdateItemRequest request, CancellationToken cancellationToken = default)
        {

            return await ThrottleManager.ThrottleRequestWithRetries(request, 
                (requestItem, cancelToken) => base.UpdateItemAsync(requestItem, cancelToken), 
                _timeOut, 
                _dbConfig.MaxErrorRetry, 
                _logger, 
                cancellationToken);
        }


        public override async  Task<DeleteItemResponse> DeleteItemAsync(DeleteItemRequest request, CancellationToken cancellationToken = default)
        {
            return await ThrottleManager.ThrottleRequestWithRetries(request, (requestItem, cancelToken) => base.DeleteItemAsync(requestItem, cancelToken), 
                _timeOut, 
                _dbConfig.MaxErrorRetry, 
                _logger, 
                cancellationToken);
        }


        public override async Task<PutItemResponse> PutItemAsync(PutItemRequest request, CancellationToken cancellationToken = default)
        {
            return await ThrottleManager.ThrottleRequestWithRetries(request, (requestItem, cancelToken) => base.PutItemAsync(requestItem, cancelToken), 
                _timeOut, 
                _dbConfig.MaxErrorRetry, 
                _logger, 
                cancellationToken);
        }

        public override async Task<QueryResponse> QueryAsync(QueryRequest request, CancellationToken cancellationToken = default)
        {
            return await ThrottleManager.ThrottleRequestWithRetries(request, 
                (requestItem, cancelToken) => base.QueryAsync(requestItem, cancelToken), 
                _timeOut, 
                _dbConfig.MaxErrorRetry, 
                _logger, 
                cancellationToken);
        }


    }
}
