
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Twilio.Security;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Repository.Phone;

namespace Whetstone.StoryEngine.Repository.Messaging
{
    public class TwilioStatusCallbackHandler : SecretStoreReaderBase<TwilioCredentials>, ITwilioStatusCallbackHandler
    {
        private readonly ILogger<TwilioStatusCallbackHandler> _logger;

        private readonly TwilioConfig _twilioConfig;

        public TwilioStatusCallbackHandler(IOptions<TwilioConfig> twilioConfig,
            ISecretStoreReader secureStoreReader, IMemoryCache memCache, ILogger<TwilioStatusCallbackHandler> logger) : base(secureStoreReader, memCache)
        {
            if (twilioConfig == null)
                throw new ArgumentNullException("twilioConfig cannot be null");

            if (twilioConfig.Value == null)
                throw new ArgumentException("twilioConfig.Value cannot be null");

            if (string.IsNullOrWhiteSpace(twilioConfig.Value.LiveCredentials))
                throw new ArgumentException($"Twilio config LiveCredentials is required");

            _twilioConfig = twilioConfig.Value;

            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");

        }


        public async Task ProcessTwilioStatusCallbackAsync(TwilioStatusUpdateMessage statusUpdateMessage)
        {
            if (statusUpdateMessage == null)
                throw new ArgumentNullException($"{nameof(statusUpdateMessage)}");

            if (statusUpdateMessage.OriginalUrl == null)
                throw new ArgumentException("OriginalUrl of the statusUpdateMessage cannot be null");

            if ((statusUpdateMessage.MessageBody?.Keys?.Any()).GetValueOrDefault(false))
                throw new ArgumentException("MessageBody of the statusUpdateMessage cannot be null or empty");

            if(string.IsNullOrWhiteSpace(statusUpdateMessage.ValidationToken))
                throw new ArgumentException("ValidationToken of the statusUpdateMessage cannot be null or empty");

            await Task.Run(async () =>
            {
                TwilioCredentials creds = await GetCredentialsAsync(_twilioConfig.LiveCredentials);

                if (creds == null)
                    throw new Exception($"Credentials for {_twilioConfig.LiveCredentials} not found");


                RequestValidator reqValidator = new RequestValidator(creds.Token);


                bool isValidMessage = reqValidator.Validate(statusUpdateMessage.OriginalUrl.ToString(),
                                        statusUpdateMessage.MessageBody,
                                        statusUpdateMessage.ValidationToken);


                if (!isValidMessage)
                    throw new Exception($"Twilio status callback message failed validation for message  id:{statusUpdateMessage.QueueMessageId}");


                _logger.LogDebug($"Message queue id:{statusUpdateMessage.QueueMessageId} Twilio authentication verified");
            });
        }
    }
}
