using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Messaging.Sms;

namespace Whetstone.StoryEngine.InboundSmsHandler
{

    public class Program
    {
        private static Lazy<SmsTasks> _nativeFunction = new Lazy<SmsTasks>(GetNativeFunction);


        private static SmsTasks GetNativeFunction()
        {
            return new SmsTasks();
        }

        public static async Task Main()
            => await RunAsync();

        public static async Task RunAsync(
            HttpClient httpClient = null,
            CancellationToken cancellationToken = default)
        {
            Amazon.Lambda.Serialization.Json.JsonSerializer ser =
                new Amazon.Lambda.Serialization.Json.JsonSerializer(x => x.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);


#pragma warning disable IDE0063 // Use simple 'using' statement
            using (var handlerWrapper = HandlerWrapper.GetHandlerWrapper((Func<InboundSmsMessage, ILambdaContext, Task>)ProcessRequestAsync, ser))
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

        public static async Task ProcessRequestAsync(InboundSmsMessage request, ILambdaContext context)
        {
            await _nativeFunction.Value.SmsHandlerTask(request, context);
        }
    }
}
