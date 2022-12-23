using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Model;
using Amazon.Runtime.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Bixby.Repository.Models;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Repository;

namespace Whetstone.StoryEngine.Bixby.Repository
{
    // Statilean URI:
    // https://mdix56flo5.execute-api.us-east-1.amazonaws.com/Prod


    public class BixbyCallbackRepository : IBixbyCallbackRepository
    {

        private const string ALIAS_PARAM = "alias";
        private const string BixbyCallback_PATH = "/";
        private const string PROTO_HEADER = "X-Forwarded-Proto";

        private readonly IStoryRequestProcessor _storyProcessor;
        private readonly IMediaLinker _mediaLinker;
        private readonly IAppMappingReader _appMappingReader;

        private readonly AuditClientMessagesConfig _auditConfig;
        private readonly ISessionLogger _sessionLogger;
        private readonly ILogger _dataLogger = null; 


        public BixbyCallbackRepository(IOptions<AuditClientMessagesConfig> auditConfig, IStoryRequestProcessor storyProcessor, IMediaLinker mediaLinker, IAppMappingReader appMappingReader,
             ISessionLogger sessionLogger, ILogger<BixbyCallbackRepository> logger)
        {

            _dataLogger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");


            _storyProcessor = storyProcessor ?? throw new ArgumentNullException($"{nameof(storyProcessor)}");

            _mediaLinker = mediaLinker ?? throw new ArgumentNullException($"{nameof(mediaLinker)}");

            _appMappingReader = appMappingReader ?? throw new ArgumentNullException($"{nameof(appMappingReader)}");


            _sessionLogger = sessionLogger ?? throw new ArgumentNullException($"{nameof(sessionLogger)}");



            _auditConfig = auditConfig.Value ?? throw new ArgumentNullException($"{nameof(auditConfig)}");

        }


        public async Task<APIGatewayProxyResponse> ProcessBixbyCallbackRequestAsync(
            APIGatewayProxyRequest BixbyCallbackRequest, ILambdaContext context)
        {

            bool isValidRequest = IsValidRequest(BixbyCallbackRequest);

            if (!isValidRequest)
            {
                // If the response is not valid, then return a 400. 
                APIGatewayProxyResponse proxyResp = BuildBixbyCallbackResponse();
                proxyResp.StatusCode = 400;
                return proxyResp;
            }

            BixbyRequest_V1 request;
            string alias = GetAliasRequest(BixbyCallbackRequest);

            //TODO get alias from query string.
            string bodyText = BixbyCallbackRequest.Body;

            try
            {
                request = JsonConvert.DeserializeObject< BixbyRequest_V1>(bodyText);
            }
            catch (Exception ex)
            {
                _dataLogger.LogError(ex, $"Web hook request could not be parsed: {bodyText}");
                APIGatewayProxyResponse proxyResp = BuildBixbyCallbackResponse();
                proxyResp.StatusCode = 400;
                return proxyResp;
            }


            StoryRequest storyReq = await request.ToStoryRequestAsync(_appMappingReader, alias);

            string jsonResponse = await ProcessFlowRequestAsync(storyReq,  bodyText);

            //BixbyCallbackResponse_V1 bixbyResponse = new BixbyCallbackResponse_V1();

            //bixbyResponse.Dlg = "Kilroy was here";
            //bixbyResponse.DisplayDlg = "Kilroy was here";
            //bixbyResponse.Url = "https://prod-sbsstoryengine.s3.amazonaws.com/stories/whetstonetechnologies/0.3/image/Whetstone_logo_combo_720x480.png";
            //bixbyResponse.FollowUpPrompt = "Yes or no?";

            //string jsonResponse = JsonConvert.SerializeObject(bixbyResponse);


            APIGatewayProxyResponse apiResp = BuildBixbyCallbackResponse();

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

        private APIGatewayProxyResponse BuildBixbyCallbackResponse()
        {
            APIGatewayProxyResponse gatewayResponse = new APIGatewayProxyResponse
            {
                StatusCode = 200,
                IsBase64Encoded = true,
                MultiValueHeaders = new AutoConstructedDictionary<string, IList<string>>(),
                Headers = new Dictionary<string, string>()
            };

            gatewayResponse.MultiValueHeaders.Add("Strict-Transport-Security", new List<string>() { "max-age=2592000" });

            gatewayResponse.MultiValueHeaders.Add("Cache-Control", new List<string>() { "no-store, must-revalidate, no-cache" });

            gatewayResponse.MultiValueHeaders.Add("Expires", new List<string>() { "0" });

            gatewayResponse.MultiValueHeaders.Add("Pragma", new List<string>() { "no-cache" });

            gatewayResponse.MultiValueHeaders.Add("X-Content-Type-Options", new List<string>() { "nosniff" });

            gatewayResponse.MultiValueHeaders.Add("Content-Type", new List<string>() { "application/json" });

            return gatewayResponse;
        }

        private string GetAliasRequest(APIGatewayProxyRequest BixbyCallbackRequest)
        {
            string alias = null;

            if (BixbyCallbackRequest.QueryStringParameters != null)
            {
                var queryParams = BixbyCallbackRequest.QueryStringParameters;

                if (queryParams.ContainsKey(ALIAS_PARAM))
                    alias = queryParams[ALIAS_PARAM];
            }

            if(!string.IsNullOrWhiteSpace(alias))
                _dataLogger.LogInformation($"Requesting alias {alias}");

            return alias;
        }

        private bool IsValidRequest(APIGatewayProxyRequest BixbyCallbackRequest)
        {
            bool isPostMethdod = false;
            bool isPathValid = false;
            bool isHttps = false;

            if (!string.IsNullOrWhiteSpace(BixbyCallbackRequest.HttpMethod))
            {
                isPostMethdod = BixbyCallbackRequest.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase);
            }

            if (!string.IsNullOrWhiteSpace(BixbyCallbackRequest.Path))
            {
                isPathValid = BixbyCallbackRequest.Path.Equals(BixbyCallback_PATH, StringComparison.OrdinalIgnoreCase);
            }

            if (BixbyCallbackRequest.Headers != null)
            {
                if (BixbyCallbackRequest.Headers.ContainsKey(PROTO_HEADER))
                {

                    string protoVal = BixbyCallbackRequest.Headers[PROTO_HEADER];
                    if (!string.IsNullOrWhiteSpace(protoVal))
                    {
                        isHttps = protoVal.Equals("https", StringComparison.OrdinalIgnoreCase);
                    }
                }

            }
            

            if (!isPostMethdod)
                _dataLogger.LogError("Request is not a post method");

            if (!isPathValid)
                _dataLogger.LogError($"Request path is not {BixbyCallback_PATH}");

            if(!isHttps)
                _dataLogger.LogError($"Request does not contain header {PROTO_HEADER} or {PROTO_HEADER} header value is not https");


            bool isValid = isPostMethdod && isPathValid && isHttps;

            if (!isValid)
            {
                string jsonText = JsonConvert.SerializeObject(BixbyCallbackRequest);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("APIGateway request is not valid");
                sb.AppendLine("------------------------------------");
                sb.Append(jsonText);
                _dataLogger.LogError(sb.ToString());
            }

            return isValid;
        }

        private async Task<string> ProcessFlowRequestAsync(StoryRequest storyReq,  string bodyText)
        {
            StoryResponse storyResp = null;

            try
            {
                storyResp = await _storyProcessor.ProcessStoryRequestAsync(storyReq);
            }
            catch (Exception ex)
            {
                _dataLogger.LogError(ex, "Error in story processor");
            }


            string responseJson = null;

            try
            {
                BixbyCallbackResponse_V1 bixbyResponse =  storyResp.ToBixbyCallbackResponse(_mediaLinker, _dataLogger);
                responseJson = JsonConvert.SerializeObject(bixbyResponse);
            }
            catch (Exception ex)
            {
                _dataLogger.LogError(ex, "Error converting StoryResponse to BixbyResponse_V1");


                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Error converting StoryResponse to BixbyResponse_V1:");

                sb.AppendLine(ex.ToString());

                var conversionErrorText = sb.ToString();
                if (storyResp == null)
                    storyResp = new StoryResponse();

                storyResp.ResponseConversionError = conversionErrorText;
            }


            // If the configuration setting indicates that messages are logged and audited, then send the messages to the 
            // queue.




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
                    _dataLogger.LogError(ex, "Error sending response message for audit");

                }

            }

            return responseJson;

        }



    }
}
