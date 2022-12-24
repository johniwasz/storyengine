using Amazon.Lambda.APIGatewayEvents;
using Amazon.Runtime.Internal;
using System;
using System.Collections.Generic;

namespace Whetstone.StoryEngine.Google.Repository
{
    internal static class APIGatewayProxyExtensions
    {

        private const string ALIAS_PARAM = "alias";

        internal static string GetAliasRequest(this APIGatewayProxyRequest gatewayRequest)
        {
            return GetQueryStringValue(gatewayRequest, ALIAS_PARAM);
        }


        internal static string GetQueryStringValue(this APIGatewayProxyRequest gatewayRequest, string queryParamName)
        {
            if (string.IsNullOrWhiteSpace(queryParamName))
                throw new ArgumentException($"{nameof(queryParamName)} cannot be null or empty");

            string retVal = null;

            if (gatewayRequest.QueryStringParameters != null)
            {
                var queryParams = gatewayRequest.QueryStringParameters;

                if (queryParams.ContainsKey(queryParamName))
                    retVal = queryParams[queryParamName];
            }

            return retVal;
        }



        internal static APIGatewayProxyResponse BuilGatewayResponse()
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

    }
}
