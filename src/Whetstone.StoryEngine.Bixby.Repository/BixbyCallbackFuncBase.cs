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

namespace Whetstone.StoryEngine.Bixby.Repository
{
    public abstract class BixbyCallbackFuncBase : ClientLambdaBase
    {


        public BixbyCallbackFuncBase() : base()
        {


        }

        /// <summary>
        /// This is the wrapper that's used by the BixbyCallback lambda function.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual async Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            Stopwatch funcTime = Stopwatch.StartNew();

            APIGatewayProxyResponse resp = null;
            IBixbyCallbackRepository bixbyCallbackRep = Services.GetRequiredService<IBixbyCallbackRepository>();

            ILogger<BixbyCallbackFuncBase> bixbyLogger = Services.GetRequiredService<ILogger<BixbyCallbackFuncBase>>();

            try
            {
                resp = await bixbyCallbackRep.ProcessBixbyCallbackRequestAsync(request, context);
            }
            catch (Exception ex)
            {
                bixbyLogger.LogError(ex, "Unexpected error while processing BixbyCallback request");
            }


            bixbyLogger.LogInformation($"BixbyCallback request processing time: {funcTime.ElapsedMilliseconds} milliseconds");

            return resp;
        }


        protected override void ConfigureServices(IServiceCollection services, IConfiguration config, BootstrapConfig bootConfig)
        {
            services.AddTransient<IBixbyCallbackRepository, BixbyCallbackRepository>();
        }
    }
}
