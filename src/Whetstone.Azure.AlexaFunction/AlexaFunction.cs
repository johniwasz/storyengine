using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Whetstone.Alexa;
using System.Net.Http;
using Autofac.Core;
using Org.BouncyCastle.Asn1.Ocsp;
using Whetstone.StoryEngine.AlexaProcessor;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Diagnostics;
using Microsoft.Azure.Functions;
using Microsoft.Azure.Functions.Worker;

namespace Whetstone.Azure.AlexaFunction
{
    public class AlexaFunction
    {

        private readonly ILogger<AlexaFunction> _logger;
        private readonly IAlexaRequestProcessor _alexaRequestProcessor;

        public AlexaFunction(IAlexaRequestProcessor alexaProcesor, ILogger<AlexaFunction> logger)
        {
            this._alexaRequestProcessor = alexaProcesor ?? throw new ArgumentNullException(nameof(alexaProcesor));

            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));            
        }

        [Function("AlexaFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            AlexaResponse? response = default;
          
            AlexaRequest? request = JsonConvert.DeserializeObject<AlexaRequest>(requestBody);

            Stopwatch funcTime = Stopwatch.StartNew();

            try
            {
                response = await _alexaRequestProcessor.ProcessAlexaRequestAsync(request);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Unexpected error processing Alexa request");
            }


            this._logger.LogInformation($"Alexa request processing time: {funcTime.ElapsedMilliseconds} milliseconds");


            return new OkObjectResult(response);
        }
    }
}
