using Amazon.Lambda.APIGatewayEvents;
using Amazon.Runtime.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Google.Repository
{
    public abstract class APIGatewayProxyRepositoryBase
    {
  
        private const string DIALOGFLOW_PATH = "/";
        private const string PROTO_HEADER = "X-Forwarded-Proto";


        protected readonly ILogger<APIGatewayProxyRepositoryBase> _dataLogger = null;


        public APIGatewayProxyRepositoryBase(ILogger<APIGatewayProxyRepositoryBase> logger)
        {
            _dataLogger = logger ?? throw new ArgumentNullException(nameof(logger));
           
        }



        protected bool IsValidRequest(APIGatewayProxyRequest dialogFlowRequest)
        {
            bool isPostMethdod = false;
            bool isPathValid = false;
            bool isHttps = false;

            if (!string.IsNullOrWhiteSpace(dialogFlowRequest.HttpMethod))
            {
                isPostMethdod = dialogFlowRequest.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase);
            }

            if (!string.IsNullOrWhiteSpace(dialogFlowRequest.Path))
            {
                isPathValid = dialogFlowRequest.Path.Equals(DIALOGFLOW_PATH, StringComparison.OrdinalIgnoreCase);
            }

            if (dialogFlowRequest.Headers != null)
            {
                if (dialogFlowRequest.Headers.ContainsKey(PROTO_HEADER))
                {

                    string protoVal = dialogFlowRequest.Headers[PROTO_HEADER];
                    if (!string.IsNullOrWhiteSpace(protoVal))
                    {
                        isHttps = protoVal.Equals("https", StringComparison.OrdinalIgnoreCase);
                    }
                }

            }


            if (!isPostMethdod)
                _dataLogger.LogError("Request is not a post method");

            if (!isPathValid)
                _dataLogger.LogError($"Request path is not {DIALOGFLOW_PATH}");

            if (!isHttps)
                _dataLogger.LogError($"Request does not contain header {PROTO_HEADER} or {PROTO_HEADER} header value is not https");


            bool isValid = isPostMethdod && isPathValid && isHttps;

            if (!isValid)
            {
                string jsonText = JsonConvert.SerializeObject(dialogFlowRequest);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("APIGateway request is not valid");
                sb.AppendLine("------------------------------------");
                sb.Append(jsonText);
                _dataLogger.LogError(sb.ToString());
            }

            return isValid;
        }
    }
}
