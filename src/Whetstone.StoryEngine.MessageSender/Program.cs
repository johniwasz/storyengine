using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Messaging;
using Newtonsoft.Json;

namespace Whetstone.StoryEngine.MessageSender
{
    public class Program
    {
        private static Lazy<MessageSenderTasksFunction> _nativeFunction = new Lazy<MessageSenderTasksFunction>(GetNativeFunction);


        private static MessageSenderTasksFunction GetNativeFunction()
        {
            return new MessageSenderTasksFunction();
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
            using (var handlerWrapper = HandlerWrapper.GetHandlerWrapper((Func<IOutboundNotificationRecord, ILambdaContext, Task<IOutboundNotificationRecord>>)ProcessRequestAsync, ser))
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

    

        public static async Task<IOutboundNotificationRecord> ProcessRequestAsync(IOutboundNotificationRecord request, ILambdaContext context)
        {

                return await _nativeFunction.Value.DispatchMessageAsync(request, context);
            


            //await _nativeFunction.Value.FunctionHandler(request, context);

        }
    }
}
