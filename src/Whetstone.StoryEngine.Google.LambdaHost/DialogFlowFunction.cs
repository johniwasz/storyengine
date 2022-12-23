
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Google.Repository;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;

// 	AlexaFunction-Dev-AlexaFunctionErrorLogAlarm-131PBD32M6C80
// aws cloudwatch set-alarm-state --region us-east-1 --alarm-name "AlexaFunction-Dev-AlexaFunctionErrorLogAlarm-131PBD32M6C80" --state-value OK --state-reason "identified alarm as a nonissue"
// aws cloudwatch set-alarm-state --region us-west-2 --alarm-name "Whetstone-DialogFlowApi-Prod-DialogFlowFunctionErrorLogAlarm-XM0YRADRAD0U" --state-value OK --state-reason "identified alarm as a nonissue"
// This is helpful when determining API Gateway ARNs
//https://docs.aws.amazon.com/general/latest/gr/aws-arns-and-namespaces.html#arn-syntax-apigateway
// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.

namespace Whetstone.StoryEngine.Google.LambdaHost
{
    public class DialogFlowFunction 
    {

        private static readonly Lazy<NativeFunction> _nativeFunction = new Lazy<NativeFunction>(GetNativeFunction);


        private static NativeFunction GetNativeFunction()
        {
            return new NativeFunction();
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
            return await _nativeFunction.Value.FunctionHandlerAsync(request, context);
        }


    }
}
