using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.Security.Amazon
{
    public class SigningKeyRetriever
    {


        private readonly string _metadataAddress;

        private readonly string _issuer;

        private readonly string _userPoolClientId;

        public SigningKeyRetriever(CognitoConfig cogOptions)
        {
            if (cogOptions == null)
                throw new ArgumentNullException($"{nameof(cogOptions)}");

            string region = string.IsNullOrWhiteSpace(cogOptions.UserPoolRegion)
                ? Bootstrapping.CurrentRegion.SystemName
                : cogOptions.UserPoolRegion;

            if (string.IsNullOrWhiteSpace(region))
                throw new ArgumentNullException($"{nameof(cogOptions)}",
                    $"UserPoolRegion is empty and ${nameof(Bootstrapping.CurrentRegion.SystemName)} is null");


            if (string.IsNullOrWhiteSpace(cogOptions.UserPoolId))
                throw new ArgumentNullException($"{nameof(cogOptions)}", "UserPoolId cannot be null or empty");


            if (string.IsNullOrWhiteSpace(cogOptions.UserPoolClientId))
                throw new ArgumentNullException($"{nameof(cogOptions)}", "UserPoolClientId cannot be null or empty");

            _issuer = $"https://cognito-idp.{region}.amazonaws.com/{cogOptions.UserPoolId}";


            _userPoolClientId = cogOptions.UserPoolClientId;

            _metadataAddress = $"{_issuer}/.well-known/jwks.json";
        }



        internal IEnumerable<SecurityKey> GetSigningKeys()
        {
            IEnumerable<SecurityKey> secKeys = null;

            try
            {

                var json = new WebClient().DownloadString(_metadataAddress);
                secKeys = JsonConvert.DeserializeObject<JsonWebKeySet>(json).Keys;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting signing keys from {_metadataAddress} {ex}");
                throw;
            }


            return secKeys;
        }



        public TokenValidationParameters GetTokenValidationParameters()
        {

            TokenValidationParameters valParams = new TokenValidationParameters
            {

                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _userPoolClientId,
                IssuerSigningKeys = GetSigningKeys()
            };

            return valParams;
        }


    }
}
