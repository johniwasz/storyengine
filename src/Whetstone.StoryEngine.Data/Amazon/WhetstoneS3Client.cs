using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Whetstone.StoryEngine.Data.Amazon
{
    public class WhetstoneS3Client : AmazonS3Client
    {
        private readonly ILogger<WhetstoneS3Client> _logger;

        private readonly AmazonS3Config _s3Config;

        private readonly int _timeOut;

        public WhetstoneS3Client(IOptions<AmazonS3Config> s3Config, ILogger<WhetstoneS3Client> logger) : base(s3Config?.Value)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _s3Config = s3Config?.Value ?? throw new ArgumentNullException(nameof(s3Config));

            _timeOut = (int)(_s3Config.Timeout?.TotalMilliseconds).GetValueOrDefault(2000);

        }


        public override async Task<GetObjectResponse> GetObjectAsync(GetObjectRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return await ThrottleManager.ThrottleRequestWithRetries<GetObjectRequest, GetObjectResponse>(request,
                (requestItem, cancelToken) => base.GetObjectAsync(requestItem, cancelToken),
                _timeOut,
                _s3Config.MaxErrorRetry,
                _logger,
                cancellationToken);
        }

        public override async Task<PutObjectResponse> PutObjectAsync(PutObjectRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return await ThrottleManager.ThrottleRequestWithRetries<PutObjectRequest, PutObjectResponse>(request,
                (requestItem, cancelToken) => base.PutObjectAsync(requestItem, cancelToken),
                _timeOut,
                _s3Config.MaxErrorRetry,
                _logger,
                cancellationToken);
        }


        public override async Task<CopyObjectResponse> CopyObjectAsync(CopyObjectRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return await ThrottleManager.ThrottleRequestWithRetries<CopyObjectRequest, CopyObjectResponse>(request,
                (requestItem, cancelToken) => base.CopyObjectAsync(requestItem, cancelToken),
                _timeOut,
                _s3Config.MaxErrorRetry,
                _logger,
                cancellationToken);
        }


        public override async Task<DeleteObjectResponse> DeleteObjectAsync(DeleteObjectRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return await ThrottleManager.ThrottleRequestWithRetries<DeleteObjectRequest, DeleteObjectResponse>(request,
                (requestItem, cancelToken) => base.DeleteObjectAsync(requestItem, cancelToken),
                _timeOut,
                _s3Config.MaxErrorRetry,
                _logger,
                cancellationToken);
        }


        public override async Task<DeleteObjectsResponse> DeleteObjectsAsync(DeleteObjectsRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return await ThrottleManager.ThrottleRequestWithRetries<DeleteObjectsRequest, DeleteObjectsResponse>(request,
                (requestItem, cancelToken) => base.DeleteObjectsAsync(requestItem, cancelToken),
                _timeOut,
                _s3Config.MaxErrorRetry,
                _logger,
                cancellationToken);
        }


        public override async Task<UploadPartResponse> UploadPartAsync(UploadPartRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return await ThrottleManager.ThrottleRequestWithRetries<UploadPartRequest, UploadPartResponse>(request,
                (requestItem, cancelToken) => base.UploadPartAsync(requestItem, cancelToken),
                _timeOut,
                _s3Config.MaxErrorRetry,
                _logger,
                cancellationToken);
        }
    }
}
