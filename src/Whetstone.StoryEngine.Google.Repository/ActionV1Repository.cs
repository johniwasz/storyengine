using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime.Internal;
using Amazon.XRay.Recorder.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.Google.Actions.V1;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Repository;

namespace Whetstone.StoryEngine.Google.Repository
{
    public class ActionV1Repository : APIGatewayProxyRepositoryBase, IActionV1Repository
    {

        private readonly IStoryRequestProcessor _storyProcessor;
        private readonly IMediaLinker _mediaLinker;
        private readonly IAppMappingReader _appMappingReader;

        private readonly AuditClientMessagesConfig _auditConfig;
        private readonly ISessionLogger _sessionLogger;


        public ActionV1Repository(IOptions<AuditClientMessagesConfig> auditConfig, IStoryRequestProcessor storyProcessor, IMediaLinker mediaLinker, IAppMappingReader appMappingReader,
             ISessionLogger sessionLogger, ILogger<ActionV1Repository> logger) : base(logger)
        {           
            _storyProcessor = storyProcessor ?? throw new ArgumentNullException(nameof(storyProcessor));
            _mediaLinker = mediaLinker ?? throw new ArgumentNullException(nameof(mediaLinker));
            _appMappingReader = appMappingReader ?? throw new ArgumentNullException(nameof(appMappingReader));
            _sessionLogger = sessionLogger ?? throw new ArgumentNullException(nameof(sessionLogger));
            _auditConfig = auditConfig.Value ?? throw new ArgumentNullException(nameof(auditConfig));
        }


        public async Task<APIGatewayProxyResponse> ProcessActionV1RequestAsync(APIGatewayProxyRequest gatewayRequest, ILambdaContext context)
        {

            bool isValidRequest = IsValidRequest(gatewayRequest);

            if (!isValidRequest)
            {
                // If the response is not valid, then return a 400. 
                APIGatewayProxyResponse proxyResp = APIGatewayProxyExtensions.BuilGatewayResponse();
                proxyResp.StatusCode = 400;
                return proxyResp;
            }

            HandlerRequest request;


            string bodyText = gatewayRequest.Body;
            try
            {              
                request = HandlerRequest.FromJson(bodyText);
            }
            catch (Exception ex)
            {
                _dataLogger.LogError(ex, $"Web hook request could not be parsed: {bodyText}");
                APIGatewayProxyResponse proxyResp = APIGatewayProxyExtensions.BuilGatewayResponse();
                proxyResp.StatusCode = 400;
                return proxyResp;
            }


            // Covert the HandlerRequest to a StoryRequest

            StoryRequest storyReq = await gatewayRequest.ToStoryRequestAsync(_appMappingReader, _dataLogger, request);


            StoryResponse storyResp = null;

            try
            {
                storyResp = await _storyProcessor.ProcessStoryRequestAsync(storyReq);
            }
            catch (Exception ex)
            {
                AWSXRayRecorder.Instance.AddException(ex);
                _dataLogger.LogError(ex, "Error in story processor");
            }

            if (storyResp != null)
            {
                APIGatewayProxyResponse resp = storyResp.ToAPIGatewayProxyResponse(storyReq, request, _mediaLinker, _dataLogger);
                return resp;
            }


            return null;
        }




    }
}
