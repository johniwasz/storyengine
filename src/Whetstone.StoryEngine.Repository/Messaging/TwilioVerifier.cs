using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Twilio.Security;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Repository.Phone;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Repository.Messaging
{
    public class TwilioVerifier : SecretStoreReaderBase<TwilioCredentials>,  ITwilioVerifier
    {
        private ILogger<TwilioVerifier> _logger;

        private TwilioConfig _twilioSnapshotConfig;

        public TwilioVerifier(IOptions<TwilioConfig> twilioConfigOpts, ISecretStoreReader secureStoreReader, IMemoryCache memCache, ILogger<TwilioVerifier> logger) : base(secureStoreReader, memCache)
        {
            _twilioSnapshotConfig = twilioConfigOpts?.Value 
                                    ?? throw new ArgumentNullException($"{nameof(twilioConfigOpts)}");

            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");
        }


        public async Task<bool> ValidateTwilioMessageAsync(string path, IDictionary<string, string> headerValues, IDictionary<string, string> formVals, string alias)
        {


            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException($"{nameof(path)}");

            if (headerValues == null)
                throw new ArgumentNullException($"{nameof(headerValues)}");

            if (formVals == null)
                throw new ArgumentNullException($"{nameof(formVals)}");



            TwilioCredentials creds = await GetCredentialsAsync(_twilioSnapshotConfig.LiveCredentials);

            if (creds == null)
                throw new Exception($"Could not get Twilio credentials for {_twilioSnapshotConfig.LiveCredentials}");


            string authToken = creds.Token;
            if (string.IsNullOrWhiteSpace(authToken))
                throw new Exception($"Twiliio config {_twilioSnapshotConfig.LiveCredentials} does not contain an authentication token");


            bool isValid;
            string signature = headerValues["X-Twilio-Signature"];
            string hostName;

            if (headerValues.Keys.Contains("X-Original-Host"))
                hostName = headerValues["X-Original-Host"];
            else
                hostName = headerValues["Host"];

            var originalScheme = headerValues["X-Forwarded-Proto"];

            var urlBuilder = new UriBuilder {Path = path, Host = hostName, Scheme = originalScheme};


            if (!string.IsNullOrWhiteSpace(alias))
                urlBuilder.Query = $"alias={alias}";

            string origUrl = urlBuilder.ToString();

            RequestValidator reqValidator = new RequestValidator(authToken);

            isValid = reqValidator.Validate(origUrl, formVals, signature);

            _logger.LogInformation($"Validation of Twitter post to {origUrl} is {isValid}");


            return isValid;
        }



    }
}
