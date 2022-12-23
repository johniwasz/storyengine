using System;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.Extensions.Logging;

using Whetstone.StoryEngine.Security;

namespace Whetstone.StoryEngine.SocketApi.Repository
{
    public class WebAuthorizer : IWebAuthorizer
    {
        private readonly ILogger<WebAuthorizer> _logger = null;
        private readonly IJwtTokenParser _authenticator = null;

        public WebAuthorizer( ILogger<WebAuthorizer> logger, IJwtTokenParser authenticator )
        {
            if (logger == null)
                throw new InvalidOperationException("logger is null!");

            if (authenticator == null)
                throw new InvalidOperationException("authenticator is null!");


            _logger = logger;
            _authenticator = authenticator;
        }

        public JwtSecurityToken GetValidJwtFromAuthToken(string authToken)
        {
            _logger.LogDebug("Parsing Auth Token");

            JwtSecurityToken jwt = _authenticator.ParseAuthToken(authToken, true);

            _logger.LogDebug($"AuthToken Parsed and Validated - Required Claim Sub: {jwt.Subject}");

            if (String.IsNullOrEmpty(jwt.Subject))
            {
                throw new InvalidOperationException("Unable to get Sub from JWT");
            }

            return jwt;
        }

        public string GetValidUserIdFromAuthToken(string authToken)
        {
            // Get a valid JSON Web Token. The standardized userid is the Subject Claim.
            JwtSecurityToken jwt = this.GetValidJwtFromAuthToken(authToken);
            return jwt.Subject;
        }
    }
}
