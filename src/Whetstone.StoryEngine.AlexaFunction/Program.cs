using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.Json;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Whetstone.Alexa;

namespace Whetstone.StoryEngine.AlexaFunction
{
    public class Program
    {


        private static Lazy<AlexaFunction> _nativeFunction = new Lazy<AlexaFunction>(GetNativeFunction);


        private static AlexaFunction GetNativeFunction()
        {
            return new AlexaFunction();
        }

        public static async Task Main()
        {
            AWSSDKHandler.RegisterXRayForAllServices();
            AWSXRayRecorder recorder = new AWSXRayRecorderBuilder()
                .Build();

            AWSXRayRecorder.InitializeInstance(recorder: recorder);
            await RunAsync();
        }
        public static async Task RunAsync(
            HttpClient httpClient = null,
            CancellationToken cancellationToken = default)
        {
            var serializer = new JsonSerializer();


            int minThreads = 0;
            int minCompletionThreads = 0;

            int availThreads = 0;
            int availCompletionThreads = 0;

            int maxThreads = 0;
            int maxCompletionThreads = 0;

            ThreadPool.SetMinThreads(32, 32);


            ThreadPool.GetMinThreads(out minThreads, out minCompletionThreads);
            LambdaLogger.Log($"min threads: {minThreads}; min completion threads: {minCompletionThreads}");


            ThreadPool.GetAvailableThreads(out availThreads, out availCompletionThreads);
            LambdaLogger.Log($"available threads: {availThreads}; min completion threads: {availCompletionThreads}");


            ThreadPool.GetMaxThreads(out maxThreads, out maxCompletionThreads);
            LambdaLogger.Log($"max threads: {maxThreads}; max completion threads: {maxCompletionThreads}");

            //using var handlerWrapper = HandlerWrapper.GetHandlerWrapper<AlexaRequest, ILambdaContext, AlexaResponse>(ProcessRequestAsync, serializer);
            //using var bootstrap = new LambdaBootstrap(httpClient ?? new HttpClient(), handlerWrapper);
            //await bootstrap.RunAsync(cancellationToken);


            using (var handlerWrapper = HandlerWrapper.GetHandlerWrapper((Func<AlexaRequest, ILambdaContext, Task<AlexaResponse>>)ProcessRequestAsync, serializer))
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

        public static async Task<AlexaResponse> ProcessRequestAsync(AlexaRequest request, ILambdaContext context)
        {
            return await _nativeFunction.Value.FunctionHandlerAsync(request, context);
        }
    }
}
