using Amazon.Lambda.Core;
using Amazon.XRay.Recorder.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Threading.Tasks;
using Whetstone.Alexa;
using Whetstone.StoryEngine.AlexaProcessor.Configuration;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Repository;

namespace Whetstone.StoryEngine.AlexaProcessor
{
    public class AlexaRequestProcessor : IAlexaRequestProcessor
    {
        private const int MAXREQUESTSENDTIME = 150;

        private readonly ILogger _logger;

        private readonly IStoryRequestProcessor _storyProcessor = null;
        private readonly IMediaLinker _mediaLinker = null;

        private readonly ISessionLogger _sessionLogger = null;

        private readonly AlexaConfig _alexaConfig = null;


        public AlexaRequestProcessor(IOptions<AlexaConfig> alexaConfig,
            IStoryRequestProcessor storyProcessor, IMediaLinker mediaLinker, ISessionLogger sessionLogger, ILogger<AlexaRequestProcessor> logger)
        {

            if (alexaConfig == null)
                throw new ArgumentNullException(nameof(alexaConfig));



            _storyProcessor = storyProcessor ?? throw new ArgumentNullException(nameof(storyProcessor));
            _mediaLinker = mediaLinker ?? throw new ArgumentNullException(nameof(mediaLinker));


            _alexaConfig = alexaConfig.Value ??
                           throw new ArgumentNullException(nameof(alexaConfig));

            _sessionLogger = sessionLogger ?? throw new ArgumentNullException(nameof(sessionLogger));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<AlexaResponse> ProcessRequestAsync(AlexaRequest request, string alias)
        {



            if (!string.IsNullOrWhiteSpace(alias))
                AWSXRayRecorder.Instance.AddMetadata("alias", alias);

            StoryRequest storyReq = AWSXRayRecorder.Instance.TraceMethod("ToStoryRequest", () => request.ToStoryRequest());


            // One of Alexa's policy requirements is that the request cannot take more then 150 seconds to get from the Echo to the skill processing logic.
            // This check enforces that policy.
            // If the story request is a ping request, then ignore the request time.
            // For testing purposes, if Alexa policy is not enforced, then don't bother about the request timings.
            // For the diff to be one second to satisfy conditions
            var diff = (!storyReq.IsPingRequest.GetValueOrDefault(false) && _alexaConfig.EnforceAlexaPolicyCheck)
                ? DateTime.UtcNow - request.Request.Timestamp
                : new TimeSpan(0, 0, 0, 1);


            if ((Math.Abs((decimal)diff.TotalSeconds) <= MAXREQUESTSENDTIME) || storyReq.IsPingRequest.GetValueOrDefault(false))
            {

                if (!storyReq.IsPingRequest.GetValueOrDefault(false))
                    _logger.LogDebug($"Alexa request was timestamped at {request.Request.Timestamp} which is {diff.Milliseconds}ms ago and is less than the {MAXREQUESTSENDTIME * 1000}ms maximum.");


                storyReq.Alias = alias;

                CanFulfillResponse canFulfillResp = null;
                StoryResponse response = null;

                AlexaResponse resp = null;
                if (storyReq.RequestType == StoryRequestType.CanFulfillIntent)
                {
                    canFulfillResp = await _storyProcessor.CanFulfillIntentAsync(storyReq);

                    resp = new AlexaResponse
                    {
                        Response = new AlexaResponseAttributes()
                    };

                    try
                    {
                        resp.Response.CanFulfillIntent = canFulfillResp.ToCanFulfillResponse();
                    }
                    catch (Exception ex)
                    {
                        AWSXRayRecorder.Instance.AddException(ex);
                        canFulfillResp.ResponseConversionError = $"Error converting CanFulfillResponse to AlexaResponse: {ex.ToString()}";
                    }
                }
                else
                {
                    response = await _storyProcessor.ProcessStoryRequestAsync(storyReq);
                    try
                    {

                        resp = AWSXRayRecorder.Instance.TraceMethod("ToAlexaResponseAsync", () => response.ToAlexaResponse(storyReq.SessionContext, _mediaLinker, _logger));

                    }
                    catch (Exception ex)
                    {
                        AWSXRayRecorder.Instance.AddException(ex);
                        response.ResponseConversionError = $"Error converting StoryResponse to AlexaResponse: {ex}";
                    }


                }


                bool recordVerbose = false;

                // If the title version is configured to record the full client messages,
                // then write the full request and responses to the database.
                bool versionFlag =
                    (storyReq.SessionContext?.TitleVersion?.LogFullClientMessages).GetValueOrDefault(false);


                if (_alexaConfig.AuditClientMessages || versionFlag)
                    recordVerbose = true;
                else
                {
                    if (response != null)
                        recordVerbose = !string.IsNullOrWhiteSpace(response.EngineErrorText) || !string.IsNullOrWhiteSpace(response.ResponseConversionError);
                    else if (canFulfillResp != null)
                        recordVerbose = !string.IsNullOrWhiteSpace(canFulfillResp.EngineErrorText) || !string.IsNullOrWhiteSpace(canFulfillResp.ResponseConversionError);
                }


                // If the request is a ping request, then the function has exited before here.
                // If the configuration indicate that messages are logged and audited, then send the messages to the 
                // queue or if the response has an error, then audit the request and response body.

                if (recordVerbose)
                {
                    string requestBody = JsonConvert.SerializeObject(request);
                    string responseBody = JsonConvert.SerializeObject(response);

                    if (canFulfillResp == null)
                        await _sessionLogger.LogRequestAsync(storyReq, response, requestBody, responseBody);
                    else
                        await _sessionLogger.LogRequestAsync(storyReq, canFulfillResp, requestBody, responseBody);
                }
                else
                {

                    if (canFulfillResp == null)
                        await _sessionLogger.LogRequestAsync(storyReq, response);
                    else
                        await _sessionLogger.LogRequestAsync(storyReq, canFulfillResp);
                }
                return resp;
            }
            else
            {
                throw new Exception($"Alexa request was timestamped {diff.TotalSeconds:0.00} seconds ago. It exceeds the {MAXREQUESTSENDTIME} second maximum");

            }
        }

        public async Task<AlexaResponse> ProcessAlexaRequestAsync(AlexaRequest request)
        {
            return await ProcessAlexaRequestAsync(request);
        }



        public async Task<AlexaResponse> ProcessAlexaRequestAsync(AlexaRequest request, string alias)
        {

            _logger.LogInformation("Starting Alexa processing");


            if (request == null)
                throw new ArgumentException("AlexaRequest is null");


            AlexaResponse response;
            try
            {
                response = await ProcessRequestAsync(request, alias);
            }
            catch (Exception ex)
            {

                JsonSerializerSettings serSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                string requestContent = JsonConvert.SerializeObject(request, serSettings);

                string errMsg = $"Error processing the skill request: {requestContent}";
                _logger.LogError(ErrorEvents.ProcessRequestError, ex, errMsg);
                Exception informedEx = new Exception(errMsg, ex);

                AWSXRayRecorder.Instance.AddException(informedEx);

                throw informedEx;
                // If there is an unhandled exception, then throw it so that the Alexa request goes to the dead letter queue.

            }

            return response;
        }
    }
}
