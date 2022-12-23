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
using Whetstone.StoryEngine.Reporting.Models;
using Newtonsoft.Json.Linq;

namespace Whetstone.StoryEngine.Reporting.ReportGenerator
{
    public class Program
    {
        private static Lazy<Tasks> _nativeFunction = new Lazy<Tasks>(GetNativeFunction);


        private static Tasks GetNativeFunction()
        {
            return new Tasks();
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

            try
            {
#pragma warning disable IDE0063 // Use simple 'using' statement
                using (var handlerWrapper = HandlerWrapper.GetHandlerWrapper((Func<dynamic, ILambdaContext, Task<dynamic>>)ProcessRequestAsync, ser))
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
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static async Task<dynamic> ProcessRequestAsync(dynamic request, ILambdaContext context)
        {
            dynamic retVal = null;


            JObject inboundObj = request as JObject;
    
           

            if (inboundObj.ContainsKey("allSent"))
            {

                context.Logger.LogLine("Attempting to convert dynamic object to INotificationRequest");
                string rawText = JsonConvert.SerializeObject(request);
                ReportSendStatus repStatus = JsonConvert.DeserializeObject<ReportSendStatus>(rawText);
                retVal = await _nativeFunction.Value.SendReportAsync(repStatus, context);

            }
            else
            {
                context.Logger.LogLine("Converting object to ReportRequest");
                string rawText = JsonConvert.SerializeObject(request);
                ReportRequest repRequest = JsonConvert.DeserializeObject<ReportRequest>(rawText);
                retVal = await _nativeFunction.Value.GenerateReportAsync(repRequest, context);

            }


            //await _nativeFunction.Value.FunctionHandler(request, context);

            return retVal;
        }
    }
}
