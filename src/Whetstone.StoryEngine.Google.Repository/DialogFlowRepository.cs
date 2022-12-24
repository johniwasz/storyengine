using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.XRay.Recorder.Core;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Google.Repository.Models;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Repository;

namespace Whetstone.StoryEngine.Google.Repository
{
    // Statilean URI:
    // https://mdix56flo5.execute-api.us-east-1.amazonaws.com/Prod


    public class DialogFlowRepository : APIGatewayProxyRepositoryBase, IDialogFlowRepository
    {
        private static readonly JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));



        private readonly IStoryRequestProcessor _storyProcessor;
        private readonly IMediaLinker _mediaLinker;
        private readonly IAppMappingReader _appMappingReader;

        private readonly AuditClientMessagesConfig _auditConfig;
        private readonly ISessionLogger _sessionLogger;


        public DialogFlowRepository(IOptions<AuditClientMessagesConfig> auditConfig, IStoryRequestProcessor storyProcessor, IMediaLinker mediaLinker, IAppMappingReader appMappingReader,
             ISessionLogger sessionLogger, ILogger<DialogFlowRepository> logger) : base(logger)
        {
            _storyProcessor = storyProcessor ?? throw new ArgumentNullException(nameof(storyProcessor));
            _mediaLinker = mediaLinker ?? throw new ArgumentNullException(nameof(mediaLinker));
            _appMappingReader = appMappingReader ?? throw new ArgumentNullException(nameof(appMappingReader));
            _sessionLogger = sessionLogger ?? throw new ArgumentNullException(nameof(sessionLogger));
            _auditConfig = auditConfig.Value ?? throw new ArgumentNullException(nameof(auditConfig));
        }


        public async Task<APIGatewayProxyResponse> ProcessDialogFlowRequestAsync(
            APIGatewayProxyRequest dialogFlowRequest, ILambdaContext context)
        {

            bool isValidRequest = IsValidRequest(dialogFlowRequest);

            if (!isValidRequest)
            {
                // If the response is not valid, then return a 400. 
                APIGatewayProxyResponse proxyResp = APIGatewayProxyExtensions.BuilGatewayResponse();
                proxyResp.StatusCode = 400;
                return proxyResp;
            }

            WebhookRequest request = null;
            string alias = dialogFlowRequest.GetAliasRequest();

            //TODO get alias from query string.
            string bodyText = dialogFlowRequest.Body;

            try
            {
                request = jsonParser.Parse<WebhookRequest>(bodyText);
            }
            catch (InvalidProtocolBufferException ex)
            {
                _dataLogger.LogError(ex, $"Web hook request could not be parsed: {bodyText}");
                APIGatewayProxyResponse proxyResp = APIGatewayProxyExtensions.BuilGatewayResponse();
                proxyResp.StatusCode = 400;
                return proxyResp;
            }


            StoryRequest storyReq = null;


            bool isRepromptRequest = request.IsRepromptRequest();


            storyReq = await AWSXRayRecorder.Instance.TraceMethodAsync("ToStoryRequestAsync",
                async () => await request.ToStoryRequestAsync(_appMappingReader, alias, _dataLogger));

            SurfaceCapabilities surfaceCaps = request.GetSurfaceCapabilities();


            string jsonResponse = await ProcessFlowRequestAsync(storyReq, surfaceCaps, isRepromptRequest, bodyText, request.Session);


            APIGatewayProxyResponse apiResp = APIGatewayProxyExtensions.BuilGatewayResponse();

            if (string.IsNullOrWhiteSpace(jsonResponse))
            {
                apiResp.StatusCode = 500;
            }
            else
            {
                apiResp.StatusCode = 200;
                apiResp.Body = jsonResponse;

            }

            return apiResp;
        }



        private async Task<string> ProcessFlowRequestAsync(StoryRequest storyReq, SurfaceCapabilities surfaceCaps, bool isRepromptRequest, string bodyText, string contextPrefix)
        {
            bool isPingRequest = storyReq.IsPingRequest.GetValueOrDefault(false);
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

            string responseJson = null;

            try
            {
                WebhookResponse dfResp;


                if (isRepromptRequest)
                    dfResp = AWSXRayRecorder.Instance.TraceMethod("ToDialogFlowReprompt",
                        () => storyResp.ToDialogFlowReprompt(surfaceCaps, _mediaLinker, _dataLogger, storyReq.UserId, contextPrefix));
                else
                {

                    if (storyReq.Client == Client.FacebookMessenger)
                    {
                        dfResp = AWSXRayRecorder.Instance.TraceMethod("ToFacebookMessengerResponse",
                            () => storyResp.ToFacebookMessengerResponse(_mediaLinker, _dataLogger,
                                contextPrefix));
                    }
                    else
                        dfResp = AWSXRayRecorder.Instance.TraceMethod("ToDialogFlowResponse",
                            () => storyResp.ToDialogFlowResponse(surfaceCaps, _mediaLinker, _dataLogger, storyReq.UserId,
                                contextPrefix));

                }



                responseJson = dfResp.ToString();
            }
            catch (Exception ex)
            {
                AWSXRayRecorder.Instance.AddException(ex);
                _dataLogger.LogError(ex, "Error converting StoryResponse to WebHookResponse");


                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Error converting StoryResponse to WebHookResponse:");

                sb.AppendLine(ex.ToString());

                var conversionErrorText = sb.ToString();
                if (storyResp == null)
                    storyResp = new StoryResponse();

                storyResp.ResponseConversionError = conversionErrorText;
            }


            // If the request is not a ping request and the configuration setting
            // indicates that messages are logged and audited, then send the messages to the 
            // queue.

            if (!isPingRequest)
            {
                // if there is an error to report, then capture the request and response messages.

                bool versionAuditFlag =
                    (storyReq.SessionContext?.TitleVersion?.LogFullClientMessages).GetValueOrDefault(false);


                if (_auditConfig.AuditClientMessages
                    || versionAuditFlag
                    || !string.IsNullOrWhiteSpace(storyResp?.EngineErrorText)
                    || !string.IsNullOrWhiteSpace(storyResp?.ResponseConversionError))
                {
                    try
                    {
                        await _sessionLogger.LogRequestAsync(storyReq, storyResp, bodyText, responseJson);

                    }
                    catch (Exception ex)
                    {
                        AWSXRayRecorder.Instance.AddException(ex);
                        _dataLogger.LogError(ex, "Error sending response message with client body text for audit");
                    }

                }
                else
                {
                    try
                    {
                        await _sessionLogger.LogRequestAsync(storyReq, storyResp);
                    }
                    catch (Exception ex)
                    {
                        AWSXRayRecorder.Instance.AddException(ex);
                        _dataLogger.LogError(ex, "Error sending response message for audit");

                    }

                }

            }

            return responseJson;

        }



    }
}
