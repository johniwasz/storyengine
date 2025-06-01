using System;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Azure.Services;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Messaging;
using Xunit;

namespace Whetstone.StoryEngine.AzureServices.Tests
{
    /// <summary>
    /// Unit tests for AzureBlobSessionLogger using Azurite storage emulator.
    /// These tests require Azurite to be running locally on port 10000.
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
            Assert.Contains("Test response", logContent);
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
            Assert.Contains(_testSessionId, logContent);
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
            Assert.Contains("FulfillResponse", logContent);
            Assert.Contains("Yes", logContent);
        }

        [Fact]
        public async Task LogRequestAsync_WithCanFulfillResponseAndRawText_WritesToBlob()
        {
            // Arrange
            var request = CreateTestStoryRequest();
            var fulfillResponse = CreateTestCanFulfillResponse();
            const string rawRequestText = "Raw CanFulfill request";
            const string rawResponseText = "Raw CanFulfill response";

            // Act
            await _logger.LogRequestAsync(request, fulfillResponse, rawRequestText, rawResponseText);

            // Assert
            var logContent = await _logger.ReadSessionLogsAsync(_testSessionId);
            Assert.NotNull(logContent);
            Assert.Contains(rawRequestText, logContent);
            Assert.Contains(rawResponseText, logContent);
            Assert.Contains("FulfillResponse", logContent);
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
            Assert.Contains("TestIntent", logContent);
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
            Assert.Contains("Timestamp", logContent);
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

            // Verify log exists before deletion
            var logContentBeforeDelete = await _logger.ReadSessionLogsAsync(_testSessionId);
            Assert.NotNull(logContentBeforeDelete);

            // Act
            var deleted = await _logger.DeleteSessionLogsAsync(_testSessionId);

            // Assert
            Assert.True(deleted);
            
            // Verify log is actually deleted
            var logContentAfterDelete = await _logger.ReadSessionLogsAsync(_testSessionId);
            Assert.Null(logContentAfterDelete);
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
            
            // Verify both log entries are present (each should have a timestamp)
            var timestampCount = logContent.Split("Timestamp").Length - 1;
            Assert.Equal(2, timestampCount);
        }

        [Fact]
        public async Task LoggingWithDifferentSessionIds_CreatesSeperateBlobs()
        {
            // Arrange
            var session1Id = Guid.NewGuid().ToString("N");
            var session2Id = Guid.NewGuid().ToString("N");

            var request1 = CreateTestStoryRequest();
            request1.SessionId = session1Id;
            var response1 = CreateTestStoryResponse();
            response1.OutputSpeech = new PlainTextOutputSpeech { Text = "Session 1 response" };

            var request2 = CreateTestStoryRequest();
            request2.SessionId = session2Id;
            var response2 = CreateTestStoryResponse();
            response2.OutputSpeech = new PlainTextOutputSpeech { Text = "Session 2 response" };

            // Act
            await _logger.LogRequestAsync(request1, response1);
            await _logger.LogRequestAsync(request2, response2);

            // Assert
            var session1Logs = await _logger.ReadSessionLogsAsync(session1Id);
            var session2Logs = await _logger.ReadSessionLogsAsync(session2Id);

            Assert.NotNull(session1Logs);
            Assert.NotNull(session2Logs);
            Assert.Contains("Session 1 response", session1Logs);
            Assert.Contains("Session 2 response", session2Logs);
            Assert.DoesNotContain("Session 2 response", session1Logs);
            Assert.DoesNotContain("Session 1 response", session2Logs);

            // Cleanup
            await _logger.DeleteSessionLogsAsync(session1Id);
            await _logger.DeleteSessionLogsAsync(session2Id);
        }

        public void Dispose()
        {
            // Clean up test data - ensure no data persists after tests complete
            try
            {
                _logger.DeleteAllLogsAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // Ignore cleanup errors - Azurite might not be running
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