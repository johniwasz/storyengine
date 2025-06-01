using Azure.Storage.Blobs;
using System;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Azure.Services;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Story;
using Xunit;

namespace Whetstone.StoryEngine.Test
{
    /// <summary>
    /// Unit tests for AzureBlobSessionLogger using Azurite storage emulator.
    /// </summary>
    public class AzureBlobSessionLoggerTests : IDisposable
    {
        private const string AzuriteConnectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;";
        private const string TestContainerName = "test-session-logs";
        
        private readonly AzureBlobSessionLogger _logger;
        private readonly string _testSessionId;

        public AzureBlobSessionLoggerTests()
        {
            _testSessionId = Guid.NewGuid().ToString("N");
            _logger = new AzureBlobSessionLogger(AzuriteConnectionString, TestContainerName);
        }

        [Fact]
        public async Task LogRequestAsync_WithStoryRequestAndResponse_WritesToBlob()
        {
            // Arrange
            var request = CreateTestStoryRequest();
            var response = CreateTestStoryResponse();

            // Act
            await _logger.LogRequestAsync(request, response);

            // Assert
            var logContent = await _logger.ReadSessionLogsAsync(_testSessionId);
            Assert.NotNull(logContent);
            Assert.Contains(_testSessionId, logContent);
            Assert.Contains("TestIntent", logContent);
        }

        [Fact]
        public async Task LogRequestAsync_WithRawText_WritesToBlob()
        {
            // Arrange
            var request = CreateTestStoryRequest();
            var response = CreateTestStoryResponse();
            const string rawRequestText = "Raw request text";
            const string rawResponseText = "Raw response text";

            // Act
            await _logger.LogRequestAsync(request, response, rawRequestText, rawResponseText);

            // Assert
            var logContent = await _logger.ReadSessionLogsAsync(_testSessionId);
            Assert.NotNull(logContent);
            Assert.Contains(rawRequestText, logContent);
            Assert.Contains(rawResponseText, logContent);
        }

        [Fact]
        public async Task LogRequestAsync_WithCanFulfillResponse_WritesToBlob()
        {
            // Arrange
            var request = CreateTestStoryRequest();
            var fulfillResponse = CreateTestCanFulfillResponse();

            // Act
            await _logger.LogRequestAsync(request, fulfillResponse);

            // Assert
            var logContent = await _logger.ReadSessionLogsAsync(_testSessionId);
            Assert.NotNull(logContent);
            Assert.Contains(_testSessionId, logContent);
            Assert.Contains("CanFulfill", logContent);
        }

        [Fact]
        public async Task LogRequestAsync_WithRequestRecordMessage_WritesToBlob()
        {
            // Arrange
            var sessionQueueMsg = CreateTestRequestRecordMessage();

            // Act
            await _logger.LogRequestAsync(sessionQueueMsg);

            // Assert
            var logContent = await _logger.ReadSessionLogsAsync(_testSessionId);
            Assert.NotNull(logContent);
            Assert.Contains(_testSessionId, logContent);
            Assert.Contains("SessionQueueMessage", logContent);
        }

        [Fact]
        public async Task ReadSessionLogsAsync_WhenLogExists_ReturnsContent()
        {
            // Arrange
            var request = CreateTestStoryRequest();
            var response = CreateTestStoryResponse();
            await _logger.LogRequestAsync(request, response);

            // Act
            var logContent = await _logger.ReadSessionLogsAsync(_testSessionId);

            // Assert
            Assert.NotNull(logContent);
            Assert.Contains(_testSessionId, logContent);
        }

        [Fact]
        public async Task ReadSessionLogsAsync_WhenLogDoesNotExist_ReturnsNull()
        {
            // Arrange
            var nonExistentSessionId = Guid.NewGuid().ToString("N");

            // Act
            var logContent = await _logger.ReadSessionLogsAsync(nonExistentSessionId);

            // Assert
            Assert.Null(logContent);
        }

        [Fact]
        public async Task DeleteSessionLogsAsync_WhenLogExists_ReturnsTrue()
        {
            // Arrange
            var request = CreateTestStoryRequest();
            var response = CreateTestStoryResponse();
            await _logger.LogRequestAsync(request, response);

            // Act
            var deleted = await _logger.DeleteSessionLogsAsync(_testSessionId);

            // Assert
            Assert.True(deleted);
            
            // Verify log is actually deleted
            var logContent = await _logger.ReadSessionLogsAsync(_testSessionId);
            Assert.Null(logContent);
        }

        [Fact]
        public async Task DeleteSessionLogsAsync_WhenLogDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var nonExistentSessionId = Guid.NewGuid().ToString("N");

            // Act
            var deleted = await _logger.DeleteSessionLogsAsync(nonExistentSessionId);

            // Assert
            Assert.False(deleted);
        }

        [Fact]
        public async Task AppendMultipleLogs_SameSession_AppendsToSameBlob()
        {
            // Arrange
            var request1 = CreateTestStoryRequest();
            var response1 = CreateTestStoryResponse();
            response1.OutputSpeech = new PlainTextOutputSpeech { Text = "First response" };

            var request2 = CreateTestStoryRequest();
            var response2 = CreateTestStoryResponse();
            response2.OutputSpeech = new PlainTextOutputSpeech { Text = "Second response" };

            // Act
            await _logger.LogRequestAsync(request1, response1);
            await _logger.LogRequestAsync(request2, response2);

            // Assert
            var logContent = await _logger.ReadSessionLogsAsync(_testSessionId);
            Assert.NotNull(logContent);
            Assert.Contains("First response", logContent);
            Assert.Contains("Second response", logContent);
        }

        public void Dispose()
        {
            // Clean up test data
            try
            {
                _logger.DeleteAllLogsAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        private StoryRequest CreateTestStoryRequest()
        {
            return new StoryRequest
            {
                SessionId = _testSessionId,
                UserId = Guid.NewGuid().ToString("N"),
                ApplicationId = "test-app",
                RequestId = Guid.NewGuid().ToString("N"),
                Client = Client.Alexa,
                RequestType = StoryRequestType.Intent,
                IntentName = "TestIntent",
                IsNewSession = true,
                Locale = "en-US"
            };
        }

        private StoryResponse CreateTestStoryResponse()
        {
            return new StoryResponse
            {
                OutputSpeech = new PlainTextOutputSpeech { Text = "Test response" },
                ShouldEndSession = false
            };
        }

        private CanFulfillResponse CreateTestCanFulfillResponse()
        {
            return new CanFulfillResponse
            {
                CanFulfill = CanFulfillIntent.Yes
            };
        }

        private RequestRecordMessage CreateTestRequestRecordMessage()
        {
            return new RequestRecordMessage
            {
                SessionId = _testSessionId,
                UserId = Guid.NewGuid().ToString("N"),
                RequestId = Guid.NewGuid().ToString("N"),
                IntentName = "TestIntent",
                SelectionTime = DateTime.UtcNow,
                ProcessDuration = TimeSpan.FromMilliseconds(100),
                Locale = "en-US"
            };
        }
    }
}