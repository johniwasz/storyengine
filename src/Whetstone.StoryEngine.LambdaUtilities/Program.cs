using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.LambdaUtilities
{
    public class Program
    {

        private static Lazy<KeyPolicyCustomActionFunction> _keyPolicyFunction = new Lazy<KeyPolicyCustomActionFunction>(GetKeyPolicyFunction);


        private static Lazy<BootstrapConfigFunction> _bootstrapConfigFunction = new Lazy<BootstrapConfigFunction>(GetBootstrapConfigFunction);



        private static KeyPolicyCustomActionFunction GetKeyPolicyFunction()
        {
            return new KeyPolicyCustomActionFunction();
        }

        private static BootstrapConfigFunction GetBootstrapConfigFunction()
        {
            return new BootstrapConfigFunction();
        }

        public static async Task Main()
            => await RunAsync();

        public static async Task RunAsync(
            HttpClient httpClient = null,
            CancellationToken cancellationToken = default)
        {
            var serializer = new Amazon.Lambda.Serialization.Json.JsonSerializer();



            using (var handlerWrapper = HandlerWrapper.GetHandlerWrapper((Func<CustomResourceRequest, ILambdaContext, Task>)ProcessRequestAsync, serializer))
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

        private static async Task ProcessRequestAsync(CustomResourceRequest request, ILambdaContext context)
        {

            if (request.ResourceProperties.Key != null)
            {

                await _keyPolicyFunction.Value.FunctionHandler(request, context);

            }
            else
            {
                await _bootstrapConfigFunction.Value.FunctionHandler(request, context);

            }
        }


    }
}
