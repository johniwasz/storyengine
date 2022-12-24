using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Google.Repository;
using Whetstone.StoryEngine.Models.Configuration;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Whetstone.StoryEngine.Google.Actions.LambdaHost
{
    public class ActionFunction : ClientLambdaBase
    {


        public ActionFunction() : base()
        {


        }

        /// <summary>
        /// This is the wrapper that's used by the DialogFlow lambda function.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual async Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            Stopwatch funcTime = Stopwatch.StartNew();

            ILogger<ActionFunction> dfLogger = Services.GetService<ILogger<ActionFunction>>();


            APIGatewayProxyResponse resp = null;
            IActionV1Repository dialogFlowRep = Services.GetRequiredService<IActionV1Repository>();

            try
            {
                resp = await dialogFlowRep.ProcessActionV1RequestAsync(request, context);
            }
            catch (Exception ex)
            {
                dfLogger.LogError(ex, "Unexpected error while processing DialogFlow request");
            }


            dfLogger.LogInformation($"DialogFlow request processing time: {funcTime.ElapsedMilliseconds} milliseconds");

            return resp;
        }


        protected override void ConfigureServices(IServiceCollection services, IConfiguration config, BootstrapConfig bootConfig)
        {
            services.AddTransient<IActionV1Repository, ActionV1Repository>();
        }
    }
}