using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Amazon.Lambda.Core;
using Whetstone.StoryEngine.Data;
using Whetstone.Alexa;
using Microsoft.Extensions.DependencyInjection;
using Whetstone.StoryEngine.Repository;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Whetstone.StoryEngine.AlexaProcessor.Configuration;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Models.Configuration;
//using Amazon.XRay.Recorder.Handlers.AwsSdk;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
//
namespace Whetstone.StoryEngine.AlexaProcessor
{
 public abstract class AlexaFunctionBase : ClientLambdaBase
 {    

        public AlexaFunctionBase() : base()
        {
        }

        public AlexaFunctionBase(IServiceProvider serviceProvider)
        {

            this.Services = serviceProvider;
        }

        public virtual async Task<AlexaResponse> FunctionHandlerAsync(AlexaRequest request, ILambdaContext context)
        {
            ILogger<AlexaFunctionBase> dataLogger = Services.GetService<ILogger<AlexaFunctionBase>>();

            if (_coldStartTimer.IsRunning)
            {
                dataLogger.LogInformation($"Total cold start time: {_coldStartTimer.ElapsedMilliseconds}ms");
            }



            Stopwatch funcTime = Stopwatch.StartNew();
            DateTime? alexaRequestTime = request?.Request?.Timestamp;

            if (alexaRequestTime.HasValue && _coldStartTimer.IsRunning)
            {   
                //  _dataLogger.LogInformation();
                var compareTime = DateTime.UtcNow;

                var coldStartDelay = compareTime - alexaRequestTime.Value;

                dataLogger.LogInformation($"Cold start timer: {_coldStartTimer.ElapsedMilliseconds}ms, Alexa Request difference time: {coldStartDelay.Milliseconds}, Request Time: {alexaRequestTime.Value}, Compare Time: {compareTime}");
            }



            AlexaResponse resp = null;
            IAlexaRequestProcessor alexaProcessor = Services.GetRequiredService<IAlexaRequestProcessor>();

            try
            {
                resp = await alexaProcessor.ProcessAlexaLambdaRequestAsync(request, context);
            }
            catch (Exception ex)
            {
                dataLogger.LogError(ex, "Unexpected error processing Alexa request");
            }


            dataLogger.LogInformation($"Alexa request processing time: {funcTime.ElapsedMilliseconds} milliseconds");

            if (_coldStartTimer.IsRunning)
            {
                _coldStartTimer.Stop();
                dataLogger.LogInformation($"Total cold start time to handle request: {_coldStartTimer.ElapsedMilliseconds}ms");
            }

            return resp;
        }

         protected override void ConfigureServices(IServiceCollection services, IConfiguration config, BootstrapConfig bootConfig)
       
        {

            bool enforceAlexaPolicy = bootConfig.EnforceAlexaPolicy;

            services.Configure<AlexaConfig>(
                options =>
                {
                    options.EnforceAlexaPolicyCheck = enforceAlexaPolicy;
                    
                });

            services.AddTransient<IAlexaRequestProcessor, AlexaRequestProcessor>();
        }
    }
}