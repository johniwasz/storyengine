using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Bixby.LambdaHost
{
    public class Function
    {


        private static readonly Lazy<NativeFunction> _nativeFunction = new Lazy<NativeFunction>(GetNativeFunction);


        private static NativeFunction GetNativeFunction()
        {
            return new NativeFunction();
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


            using var handlerWrapper = HandlerWrapper.GetHandlerWrapper(
                (Func<APIGatewayProxyRequest, ILambdaContext, Task<APIGatewayProxyResponse>>)ProcessRequestAsync, serializer);
            // Instantiate a LambdaBootstrap and run it.
            // It will wait for invocations from AWS Lambda and call
            // the handler function for each one.
#pragma warning disable IDE0063 // Use simple 'using' statement
            using (var bootstrap = new LambdaBootstrap(httpClient ?? new HttpClient(), handlerWrapper))
#pragma warning restore IDE0063 // Use simple 'using' statement
            {
                await bootstrap.RunAsync(cancellationToken);
            }
        }

        public static async Task<APIGatewayProxyResponse> ProcessRequestAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await _nativeFunction.Value.FunctionHandlerAsync(request, context);
        }
    }
}
