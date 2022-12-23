using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.Json;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Google.Actions.LambdaHost
{

    public class NativeFunction
    {

        private static readonly Lazy<ActionFunction> _actionFunction = new Lazy<ActionFunction>(GetActionFunction);


        private static ActionFunction GetActionFunction()
        {
            return new ActionFunction();
        }

        public static async Task Main()
        {

            AWSSDKHandler.RegisterXRayForAllServices();
            AWSXRayRecorder recorder = new AWSXRayRecorderBuilder().Build();
            AWSXRayRecorder.InitializeInstance(recorder: recorder);

            await RunAsync();
        }


        public static async Task RunAsync(
            HttpClient httpClient = null,
            CancellationToken cancellationToken = default)
        {
            var serializer = new JsonSerializer();

            ThreadPool.SetMinThreads(32, 32);


            ThreadPool.GetMinThreads(out int minThreads, out int minCompletionThreads);
            LambdaLogger.Log($"min threads: {minThreads}; min completion threads: {minCompletionThreads}");


            ThreadPool.GetAvailableThreads(out int availThreads, out int availCompletionThreads);
            LambdaLogger.Log($"available threads: {availThreads}; min completion threads: {availCompletionThreads}");


            ThreadPool.GetMaxThreads(out int maxThreads, out int maxCompletionThreads);
            LambdaLogger.Log($"max threads: {maxThreads}; max completion threads: {maxCompletionThreads}");


            using var handlerWrapper = HandlerWrapper.GetHandlerWrapper((Func<APIGatewayProxyRequest, ILambdaContext, Task<APIGatewayProxyResponse>>)ProcessRequestAsync, serializer);
            // Instantiate a LambdaBootstrap and run it.
            // It will wait for invocations from AWS Lambda and call
            // the handler function for each one.
            using var bootstrap = new LambdaBootstrap(httpClient ?? new HttpClient(), handlerWrapper);
            await bootstrap.RunAsync(cancellationToken);
        }

        public static async Task<APIGatewayProxyResponse> ProcessRequestAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return await _actionFunction.Value.FunctionHandlerAsync(request, context);
        }
    }
}
