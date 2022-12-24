using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.Google.Repository
{
    public abstract class DialogFlowFuncBase : ClientLambdaBase
    {


        public DialogFlowFuncBase() : base()
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

            ILogger<DialogFlowFuncBase> dfLogger = Services.GetService<ILogger<DialogFlowFuncBase>>();


            APIGatewayProxyResponse resp = null;
            IDialogFlowRepository dialogFlowRep = Services.GetRequiredService<IDialogFlowRepository>();

            try
            {
                resp = await dialogFlowRep.ProcessDialogFlowRequestAsync(request, context);
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
            services.AddTransient<IDialogFlowRepository, DialogFlowRepository>();

            //services.AddTransient<IFileRepository, >
        }
    }
}
