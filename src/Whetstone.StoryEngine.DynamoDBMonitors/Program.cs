using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.DynamoDBMonitors
{


    public class Program
    {
        private static Lazy<DynamoDBTableMonitorFunction> _nativeFunction = new Lazy<DynamoDBTableMonitorFunction>(GetNativeFunction);


        private static DynamoDBTableMonitorFunction GetNativeFunction()
        {
            return new DynamoDBTableMonitorFunction();
        }

        public static async Task Main()
            => await RunAsync();

        public static async Task RunAsync(
            HttpClient httpClient = null,
            CancellationToken cancellationToken = default)
        {
            var serializer = new JsonSerializer();

            //using var handlerWrapper = HandlerWrapper.GetHandlerWrapper<AlexaRequest, ILambdaContext, AlexaResponse>(ProcessRequestAsync, serializer);
            //using var bootstrap = new LambdaBootstrap(httpClient ?? new HttpClient(), handlerWrapper);
            //await bootstrap.RunAsync(cancellationToken);


            using (var handlerWrapper = HandlerWrapper.GetHandlerWrapper((Func<DynamoDBEvent, ILambdaContext, Task>)ProcessRequestAsync, serializer))
            {
                // Instantiate a LambdaBootstrap and run it.
                // It will wait for invocations from AWS Lambda and call
                // the handler function for each one.
                using (var bootstrap = new LambdaBootstrap(httpClient ?? new HttpClient(), handlerWrapper))
                {
                    await bootstrap.RunAsync(cancellationToken);
                }
            }
        }

        public static async Task ProcessRequestAsync(DynamoDBEvent request, ILambdaContext context)
        {
            await _nativeFunction.Value.SyncUserRecords(request, context);
        }


    }
}
