using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.Json;
using Amazon.Lambda.SQSEvents;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Whetstone.Queue.SessionLogger
{
    public class Program
    {

        private static readonly Lazy<SessionQueueFunction> _nativeFunction = new Lazy<SessionQueueFunction>(GetNativeFunction);


        private static SessionQueueFunction GetNativeFunction()
        {
            return new SessionQueueFunction();
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


            using var handlerWrapper = HandlerWrapper.GetHandlerWrapper((Func<SQSEvent, ILambdaContext, Task>)ProcessRequestAsync, serializer);
            // Instantiate a LambdaBootstrap and run it.
            // It will wait for invocations from AWS Lambda and call
            // the handler function for each one.
            using var bootstrap = new LambdaBootstrap(httpClient ?? new HttpClient(), handlerWrapper);
            await bootstrap.RunAsync(cancellationToken);
        }

        public static async Task ProcessRequestAsync(SQSEvent request, ILambdaContext context)
        {
           await _nativeFunction.Value.QueueFunctionHandler(request, context);
        }


    }
}
