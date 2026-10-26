# AzureBlobSessionLogger Unit Tests

This document describes how to run the unit tests for the AzureBlobSessionLogger.

## Prerequisites

The tests require the Azurite Storage Emulator to be running locally. 

### Installing and Running Azurite

1. Install Azurite using npm:
   ```bash
   npm install -g azurite
   ```

2. Start the Azurite blob service:
   ```bash
   azurite-blob --location ./azurite-data --debug ./azurite-debug.log
   ```
   
   This will start the blob service on `http://127.0.0.1:10000`

## Running the Tests

Once Azurite is running, you can execute the AzureBlobSessionLogger tests:

```bash
dotnet test src/Whetstone.StoryEngine.Test/Whetstone.StoryEngine.Test.csproj --filter "FullyQualifiedName~AzureBlobSessionLoggerTest"
```

## Test Coverage

The tests cover all methods of the ISessionLogger interface:

- `LogRequestAsync(StoryRequest request, StoryResponse response)`
- `LogRequestAsync(StoryRequest request, StoryResponse response, string rawClientRequestText, string rawClientResponseText)`
- `LogRequestAsync(StoryRequest request, CanFulfillResponse fulfillResponse)`
- `LogRequestAsync(StoryRequest request, CanFulfillResponse fulfillResponse, string rawClientRequestText, string rawClientResponseText)`
- `LogRequestAsync(RequestRecordMessage sessionQueueMsg)`

Additional test scenarios:
- Multiple log entries appending to the same blob
- Proper blob naming format validation
- Automatic cleanup to ensure no data persists after tests

## Clean Up

The tests use the `IAsyncDisposable` pattern to automatically clean up test containers after each test run, ensuring no data persists between test executions.