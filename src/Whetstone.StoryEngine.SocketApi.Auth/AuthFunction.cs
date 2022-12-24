using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Security;
using Whetstone.StoryEngine.SocketApi.Repository;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Whetstone.StoryEngine.SocketApi.Auth
{
    public class AuthFunction : AuthLambdaBase
    {
        private readonly ILogger<AuthFunction> _logger = null;
        private readonly IWebAuthorizer _authorizer = null;

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public AuthFunction() :
            base()
        {
            _logger = Services.GetService<ILogger<AuthFunction>>();
            _authorizer = Services.GetService<IWebAuthorizer>();
        }

        #region Authentication/Authorization
        public APIGatewayCustomAuthorizerResponse AuthHandler(APIGatewayCustomAuthorizerRequest request, ILambdaContext context)
        {
            _logger.LogDebug("AuthHandler Start.");

            bool authorized = false;
            string userId = null;
            string authToken = this.GetRequestQueryParam(request, context, "auth");
            string apiKey = this.GetRequestQueryParam(request, context, "api");
            string clientId = this.GetRequestQueryParam(request, context, "clientId");

            _logger.LogDebug("Validating the AuthToken");

            try
            {
                userId = _authorizer.GetValidUserIdFromAuthToken(authToken);
                authorized = true;
            }
            catch (TokenIsExpiredException)
            {
                _logger.LogError("Auth Token Expired Exception");
            }
            catch (TokenValidationException tokenException)
            {
                _logger.LogError("Auth Token Exception: " + tokenException.ToString());
            }

            // Lambda Authorizer needs to return an IAM policy that shows whether or not the
            // user has access to the resource.
            APIGatewayCustomAuthorizerPolicy policy = new APIGatewayCustomAuthorizerPolicy
            {
                Version = "2012-10-17",
                Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>()
            };

            policy.Statement.Add(new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
            {
                Action = new HashSet<string>(new string[] { "execute-api:Invoke" }),
                Effect = authorized ? "Allow" : "Deny",
                Resource = new HashSet<string>(new string[] { request.MethodArn })
            });

            APIGatewayCustomAuthorizerContextOutput contextOutput = new APIGatewayCustomAuthorizerContextOutput();
            contextOutput["Auth"] = authorized ? authToken : "InvalidUser";
            contextOutput["ClientId"] = clientId;
            contextOutput["Path"] = request.MethodArn;

            _logger.LogDebug("Building Authorization Response");

            APIGatewayCustomAuthorizerResponse response = new APIGatewayCustomAuthorizerResponse
            {
                PrincipalID = authorized ? userId : "InvalidUser",
                Context = contextOutput,
                UsageIdentifierKey = apiKey,
                PolicyDocument = policy
            };

            _logger.LogDebug("AuthHandler end");

            return response;

        }

        private string GetRequestQueryParam(APIGatewayCustomAuthorizerRequest request, ILambdaContext context, string queryParam)
        {
            string paramValue = String.Empty;

            if (request.QueryStringParameters.Count > 0 &&
                request.QueryStringParameters.ContainsKey(queryParam))
            {
                // URL Decode the parameter value
                paramValue = request.QueryStringParameters[queryParam];
                paramValue = System.Web.HttpUtility.UrlDecode(paramValue);
                _logger.LogDebug($"{queryParam} query value: {paramValue}");

            }
            else
            {
                _logger.LogDebug($"No {queryParam} query value found.");
            }

            return paramValue;
        }
        #endregion

        protected override void ConfigureServices(IServiceCollection services, IConfiguration config, BootstrapConfig bootConfig)
        {
            base.ConfigureServices(services, config, bootConfig);

            if (bootConfig.Security.AuthenticatorType == AuthenticatorType.Cognito)
            {
                services.AddCognitoAuthentication(bootConfig.Security.Cognito);
                services.AddTransient<IWebAuthorizer, WebAuthorizer>();
            }
            else
            {
                throw new InvalidOperationException($"Unsupported Authenticator Type: {bootConfig.Security.AuthenticatorType}");
            }

        }

    }
}