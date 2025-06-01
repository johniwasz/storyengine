# Azure Blob Session Logger Tests

This project contains unit tests for the `AzureBlobSessionLogger` class that validates logging session data to Azure Blob Storage using the Azurite storage emulator.

## Prerequisites

### Install and Run Azurite

Azurite is the Azure Storage Emulator that provides a local environment for testing Azure Storage applications.

#### Using Docker (Recommended):

```bash
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite
```

#### Using npm:

```bash
npm install -g azurite
azurite
```

Azurite will run on:
- Blob service: `http://127.0.0.1:10000`
- Queue service: `http://127.0.0.1:10001`
- Table service: `http://127.0.0.1:10002`

## Running the Tests

1. Start Azurite (see above)
2. Run the tests:

```bash
dotnet test
```

## Test Coverage

The tests validate the following functionality:

### Writing Operations:
- ✅ **LogRequestAsync with StoryRequest and StoryResponse** - Tests basic logging functionality
- ✅ **LogRequestAsync with raw text** - Tests logging with raw request/response text 
- ✅ **LogRequestAsync with CanFulfillResponse** - Tests logging CanFulfill responses
- ✅ **LogRequestAsync with CanFulfillResponse and raw text** - Tests CanFulfill with raw text
- ✅ **LogRequestAsync with RequestRecordMessage** - Tests logging queue messages

### Reading Operations:
- ✅ **ReadSessionLogsAsync when log exists** - Tests reading existing session logs
- ✅ **ReadSessionLogsAsync when log doesn't exist** - Tests handling of non-existent logs

### Deleting Operations:
- ✅ **DeleteSessionLogsAsync when log exists** - Tests successful deletion of logs
- ✅ **DeleteSessionLogsAsync when log doesn't exist** - Tests handling of deletion attempts on non-existent logs

### Advanced Operations:
- ✅ **Append multiple logs to same session** - Tests that multiple requests for the same session append to the same blob
- ✅ **Separate blobs for different sessions** - Tests that different sessions create separate blobs

### Data Persistence:
- ✅ **No data persistence after test completion** - All tests clean up their data using the `DeleteAllLogsAsync()` method in the `Dispose()` method

## Connection String

The tests use the standard Azurite connection string:
```
DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;
```

## Test Container

Tests use a container named `test-session-logs` which is created automatically if it doesn't exist.

## Error Handling

If Azurite is not running, tests will fail with connection errors. Make sure Azurite is started before running the tests.