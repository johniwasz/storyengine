## Running Tests with Azurite

To demonstrate the tests working, follow these steps:

### 1. Start Azurite
```bash
# Using Docker (recommended)
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite

# Or using npm
npm install -g azurite
azurite
```

### 2. Run the tests
```bash
cd src/Whetstone.StoryEngine.AzureServices.Tests
dotnet test --logger "console;verbosity=detailed"
```

### Expected Test Results (when Azurite is running):
- ✅ LogRequestAsync_WithStoryRequestAndResponse_WritesToBlob
- ✅ LogRequestAsync_WithRawText_WritesToBlob  
- ✅ LogRequestAsync_WithCanFulfillResponse_WritesToBlob
- ✅ LogRequestAsync_WithCanFulfillResponseAndRawText_WritesToBlob
- ✅ LogRequestAsync_WithRequestRecordMessage_WritesToBlob
- ✅ ReadSessionLogsAsync_WhenLogExists_ReturnsContent
- ✅ ReadSessionLogsAsync_WhenLogDoesNotExist_ReturnsNull
- ✅ DeleteSessionLogsAsync_WhenLogExists_ReturnsTrue
- ✅ DeleteSessionLogsAsync_WhenLogDoesNotExist_ReturnsFalse
- ✅ AppendMultipleLogs_SameSession_AppendsToSameBlob
- ✅ LoggingWithDifferentSessionIds_CreatesSeperateBlobs

### If Azurite is not running:
Tests will fail with connection errors to `127.0.0.1:10000`, which is expected behavior.