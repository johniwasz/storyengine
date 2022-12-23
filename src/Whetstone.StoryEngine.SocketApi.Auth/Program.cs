using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;

namespace Whetstone.StoryEngine.SocketApi.Auth
{
    public class Program
    {
        private static Lazy<AuthFunction> _nativeFunction = new Lazy<AuthFunction>(GetNativeFunction);


        private static AuthFunction GetNativeFunction()
        {
            return new AuthFunction();
        }

        public static async Task Main()
            => await RunAsync();

        public static async Task RunAsync(
            HttpClient httpClient = null,
            CancellationToken cancellationToken = default)
        {
            Amazon.Lambda.Serialization.Json.JsonSerializer ser =
                new Amazon.Lambda.Serialization.Json.JsonSerializer(x => x.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);


            //using var handlerWrapper = HandlerWrapper.GetHandlerWrapper<AlexaRequest, ILambdaContext, AlexaResponse>(ProcessRequestAsync, serializer);
            //using var bootstrap = new LambdaBootstrap(httpClient ?? new HttpClient(), handlerWrapper);
            //await bootstrap.RunAsync(cancellationToken);

#pragma warning disable IDE0063 // Use simple 'using' statement
            using (var handlerWrapper = HandlerWrapper.GetHandlerWrapper((Func<APIGatewayCustomAuthorizerRequest, ILambdaContext, APIGatewayCustomAuthorizerResponse>)ProcessRequestAsync, ser))
#pragma warning restore IDE0063 // Use simple 'using' statement
            {
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
        }

    

        public static APIGatewayCustomAuthorizerResponse ProcessRequestAsync(APIGatewayCustomAuthorizerRequest request, ILambdaContext context)
        {
            return _nativeFunction.Value.AuthHandler(request, context);           
        }
    }
}
