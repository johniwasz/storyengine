using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using Whetstone.StoryEngine.Models.Configuration;


namespace Whetstone.StoryEngine.Security.Amazon
{
    public class CognitoTokenParser : IJwtTokenParser
    {
        private readonly CognitoConfig _cogOptions;
        private readonly ILogger<CognitoTokenParser> _logger;

        public static LazyConcurrentDictionary<CognitoConfig, TokenValidationParameters> TokenValidatorDict =
            new LazyConcurrentDictionary<CognitoConfig, TokenValidationParameters>();


        public CognitoTokenParser(IOptions<CognitoConfig> cogOptions, ILogger<CognitoTokenParser> logger)
        {
            _cogOptions = cogOptions?.Value ?? throw new ArgumentNullException(nameof(cogOptions));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public JwtSecurityToken ParseAuthToken(string token, bool validate = false)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));
            var handler = new JwtSecurityTokenHandler();


            JwtSecurityToken secToken;
            if (validate)
            {
                try
                {

                    TokenValidationParameters valParams = GetTokenValidationParameters(_cogOptions);

                    var claims = handler.ValidateToken(token, valParams, out var tokenSecure);

                    secToken = tokenSecure as JwtSecurityToken;

                }
                catch (SecurityTokenExpiredException expiredEx)
                {
                    throw new TokenIsExpiredException("Token expired", "Supplied token is expired", expiredEx);
                }
                catch (Exception ex)
                {
                    throw new TokenValidationException("Token is invalid", "Supplied token is invalid", ex);
                }
            }
            else
            {
                try
                {
                    secToken = handler.ReadToken(token) as JwtSecurityToken;
                }
                catch (Exception ex)
                {
                    throw new TokenParsingException("Token is invalid", "Supplied token cannot be parsed", ex);
                }
            }

            return secToken;
        }

        public static TokenValidationParameters GetTokenValidationParameters(CognitoConfig cognitoConfig)
        {
            return TokenValidatorDict.GetOrAdd(cognitoConfig, cogConfig =>
            {
                SigningKeyRetriever signingRetriever = new SigningKeyRetriever(cognitoConfig);
                return signingRetriever.GetTokenValidationParameters();
            });
        }


    }
}
