using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Azure.Services;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Messaging;
using Xunit;

namespace Whetstone.StoryEngine.Test.Azure
{
    public class AzureBlobSessionLoggerTest : IAsyncDisposable
    {
        private readonly string _azuriteConnectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;";
        private readonly string _testContainerName = $"test-logs-{Guid.NewGuid():N}";
        private AzureBlobSessionLogger _logger;
        private BlobContainerClient _containerClient;

        public AzureBlobSessionLoggerTest()
        {
            // Initialize the logger with test container
            _logger = new AzureBlobSessionLogger(_azuriteConnectionString, _testContainerName);
            _containerClient = new BlobContainerClient(_azuriteConnectionString, _testContainerName);
        }

        [Fact(DisplayName = "Log StoryRequest and StoryResponse")]
        public async Task LogRequestResponseAsync()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var request = CreateTestStoryRequest(sessionId);
            var response = CreateTestStoryResponse();

            // Act
            await _logger.LogRequestAsync(request, response);

            // Assert
            await VerifyLogEntryExists(sessionId);
        }

        [Fact(DisplayName = "Log StoryRequest and StoryResponse with raw client text")]
        public async Task LogRequestResponseWithRawTextAsync()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var request = CreateTestStoryRequest(sessionId);
            var response = CreateTestStoryResponse();
            var rawClientRequestText = "Raw request text";
            var rawClientResponseText = "Raw response text";

            // Act
            await _logger.LogRequestAsync(request, response, rawClientRequestText, rawClientResponseText);

            // Assert
            var logContent = await GetLogContent(sessionId);
            Assert.Contains(rawClientRequestText, logContent);
            Assert.Contains(rawClientResponseText, logContent);
        }

        [Fact(DisplayName = "Log StoryRequest and CanFulfillResponse")]
        public async Task LogRequestCanFulfillResponseAsync()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var request = CreateTestStoryRequest(sessionId);
            var fulfillResponse = CreateTestCanFulfillResponse();

            // Act
            await _logger.LogRequestAsync(request, fulfillResponse);

            // Assert
            await VerifyLogEntryExists(sessionId);
        }

        [Fact(DisplayName = "Log StoryRequest and CanFulfillResponse with raw client text")]
        public async Task LogRequestCanFulfillResponseWithRawTextAsync()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var request = CreateTestStoryRequest(sessionId);
            var fulfillResponse = CreateTestCanFulfillResponse();
            var rawClientRequestText = "Raw fulfill request text";
            var rawClientResponseText = "Raw fulfill response text";

            // Act
            await _logger.LogRequestAsync(request, fulfillResponse, rawClientRequestText, rawClientResponseText);

            // Assert
            var logContent = await GetLogContent(sessionId);
            Assert.Contains(rawClientRequestText, logContent);
            Assert.Contains(rawClientResponseText, logContent);
        }

        [Fact(DisplayName = "Log RequestRecordMessage")]
        public async Task LogRequestRecordMessageAsync()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var sessionQueueMsg = CreateTestRequestRecordMessage(sessionId);

            // Act
            await _logger.LogRequestAsync(sessionQueueMsg);

            // Assert
            await VerifyLogEntryExists(sessionId);
        }

        [Fact(DisplayName = "Multiple log entries append to same blob")]
        public async Task MultipleLogEntriesAppendAsync()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var request1 = CreateTestStoryRequest(sessionId);
            var response1 = CreateTestStoryResponse();
            var request2 = CreateTestStoryRequest(sessionId);
            var response2 = CreateTestStoryResponse();

            // Act
            await _logger.LogRequestAsync(request1, response1);
            await _logger.LogRequestAsync(request2, response2);

            // Assert
            var logContent = await GetLogContent(sessionId);
            var lines = logContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            // Should have at least 2 JSON entries
            Assert.True(lines.Length >= 2, "Expected multiple log entries");
        }

        [Fact(DisplayName = "Blob name follows expected format")]
        public async Task BlobNameFormatAsync()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var request = CreateTestStoryRequest(sessionId);
            var response = CreateTestStoryResponse();
            var currentDate = DateTime.UtcNow; // Capture the current date once
            var expectedBlobName = $"sessionlogs/{currentDate:yyyy-MM-dd}/{sessionId}.log";

            // Act
            await _logger.LogRequestAsync(request, response);

            // Assert
            var blobClient = _containerClient.GetAppendBlobClient(expectedBlobName);
            var exists = await blobClient.ExistsAsync();
            Assert.True(exists.Value, $"Expected blob {expectedBlobName} to exist");
        }

        private StoryRequest CreateTestStoryRequest(string sessionId)
        {
            return new StoryRequest
            {
                SessionId = sessionId,
                UserId = "test-user-id",
                RequestType = StoryRequestType.Launch,
                Locale = "en-US",
                IsNewSession = true,
                Client = Client.Alexa,
                SessionContext = new EngineSessionContext
                {
                    TitleVersion = new Models.Story.TitleVersion
                    {
                        ShortName = "test-title",
                        Version = "1.0"
                    }
                }
            };
        }

        private StoryResponse CreateTestStoryResponse()
        {
            return new StoryResponse
            {
                NodeName = "TestNode",
                ForceContinueSession = true
            };
        }

        private CanFulfillResponse CreateTestCanFulfillResponse()
        {
            return new CanFulfillResponse
            {
                CanFulfill = YesNoMaybeEnum.Yes
            };
        }

        private RequestRecordMessage CreateTestRequestRecordMessage(string sessionId)
        {
            return new RequestRecordMessage
            {
                SessionId = sessionId,
                UserId = "test-user-id",
                EngineRequestId = Guid.NewGuid(),
                EngineSessionId = Guid.NewGuid(),
                SelectionTime = DateTime.UtcNow,
                RequestType = StoryRequestType.Launch
            };
        }

        private async Task VerifyLogEntryExists(string sessionId)
        {
            var logContent = await GetLogContent(sessionId);
            Assert.False(string.IsNullOrEmpty(logContent), "Log content should not be empty");
            Assert.Contains(sessionId, logContent);
        }

        private async Task<string> GetLogContent(string sessionId)
        {
            var blobName = $"sessionlogs/{DateTime.UtcNow:yyyy-MM-dd}/{sessionId}.log";
            var blobClient = _containerClient.GetAppendBlobClient(blobName);
            
            var exists = await blobClient.ExistsAsync();
            if (!exists.Value)
            {
                throw new InvalidOperationException($"Blob {blobName} does not exist");
            }

            var downloadResponse = await blobClient.DownloadContentAsync();
            return downloadResponse.Value.Content.ToString();
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                // Clean up test container to ensure no data persists
                if (_containerClient != null)
                {
                    await _containerClient.DeleteIfExistsAsync();
                }
            }
            catch (Exception)
            {
                // Ignore cleanup errors
            }
        }
    }
}