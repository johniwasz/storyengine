using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Newtonsoft.Json;
using System.Text;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Messaging;

namespace Whetstone.StoryEngine.Azure.Services
{
    /// <summary>
    /// ISessionLogger implementation that logs session data to Azure Blob Storage.
    /// </summary>
    public class AzureBlobSessionLogger : ISessionLogger
    {
        private readonly BlobContainerClient _containerClient;

        /// <summary>
        /// Initializes a new instance of AzureBlobSessionLogger.
        /// </summary>
        /// <param name="connectionString">Azure Blob Storage connection string.</param>
        /// <param name="containerName">Blob container name.</param>
        public AzureBlobSessionLogger(string connectionString, string containerName)
        {
            _containerClient = new BlobContainerClient(connectionString, containerName);
            _containerClient.CreateIfNotExists();
        }


        public async Task LogRequestAsync(StoryRequest request, StoryResponse response)
        {
            var log = new { Timestamp = DateTime.UtcNow, Request = request, Response = response };
            await LogToBlobAsync(GetBlobName(request.SessionId), log);
        }

        public async Task LogRequestAsync(StoryRequest request, StoryResponse response, string rawClientRequestText, string rawClientResponseText)
        {
            var log = new
            {
                Timestamp = DateTime.UtcNow,
                Request = request,
                Response = response,
                RawClientRequestText = rawClientRequestText,
                RawClientResponseText = rawClientResponseText
            };
            await LogToBlobAsync(GetBlobName(request.SessionId), log);
        }

        public async Task LogRequestAsync(StoryRequest request, CanFulfillResponse fulfillResponse)
        {
            var log = new { Timestamp = DateTime.UtcNow, Request = request, FulfillResponse = fulfillResponse };
            await LogToBlobAsync(GetBlobName(request.SessionId), log);
        }

        public async Task LogRequestAsync(StoryRequest request, CanFulfillResponse fulfillResponse, string rawClientRequestText, string rawClientResponseText)
        {
            var log = new
            {
                Timestamp = DateTime.UtcNow,
                Request = request,
                FulfillResponse = fulfillResponse,
                RawClientRequestText = rawClientRequestText,
                RawClientResponseText = rawClientResponseText
            };
            await LogToBlobAsync(GetBlobName(request.SessionId), log);
        }

        public async Task LogRequestAsync(RequestRecordMessage sessionQueueMsg)
        {
            var log = new { Timestamp = DateTime.UtcNow, SessionQueueMessage = sessionQueueMsg };
            await LogToBlobAsync(GetBlobName(sessionQueueMsg.SessionId), log);
        }

        private async Task LogToBlobAsync(string blobName, object logObject)
        {
            var blobClient = _containerClient.GetAppendBlobClient(blobName);
            if (!await blobClient.ExistsAsync())
            {
                await blobClient.CreateAsync();
            }

            string json = JsonConvert.SerializeObject(logObject, Formatting.Indented);
            byte[] bytes = Encoding.UTF8.GetBytes(json + Environment.NewLine);
            using var stream = new MemoryStream(bytes);

            await blobClient.AppendBlockAsync(stream);
        }

        private string GetBlobName(string sessionId)
        {
            string date = DateTime.UtcNow.ToString("yyyy-MM-dd");
            return $"sessionlogs/{date}/{sessionId}.log";
        }
    }

}
