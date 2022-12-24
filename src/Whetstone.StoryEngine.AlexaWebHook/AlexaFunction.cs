using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Whetstone.Alexa;
using Whetstone.StoryEngine.AlexaProcessor;

namespace Whetstone.StoryEngine.AlexaFunction
{
    public class AlexaFunction
    {
        private readonly ILogger _logger;
        private readonly IAlexaRequestProcessor _alexaProcessor;

        public AlexaFunction(IAlexaRequestProcessor alexaProcessor, ILogger<AlexaFunction> logger)
        {
            _alexaProcessor = alexaProcessor ?? throw new ArgumentNullException(nameof(alexaProcessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Function("AlexaFunction")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            AlexaResponse? response = default;

            AlexaRequest? request = JsonConvert.DeserializeObject<AlexaRequest>(requestBody);

            Stopwatch funcTime = Stopwatch.StartNew();

            try
            {
                response = await _alexaProcessor.ProcessAlexaRequestAsync(request);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Unexpected error processing Alexa request");
            }


            this._logger.LogInformation($"Alexa request processing time: {funcTime.ElapsedMilliseconds} milliseconds");

            var httpResp = req.CreateResponse(HttpStatusCode.OK);

            await httpResp.WriteAsJsonAsync(response);
            
            return httpResp;
        }
    }
}
