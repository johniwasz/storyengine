using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Models.Messaging;
using Microsoft.Extensions.Logging;
using Twilio.Exceptions;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Microsoft.Extensions.Options;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Repository.Phone;

namespace Whetstone.StoryEngine.Repository.Messaging
{
    public class SmsTwilioSender : SecretStoreReaderBase<TwilioCredentials>, ISmsSender
    {
        private readonly TwilioConfig _twilioConfig;
        private readonly ILogger<SmsTwilioSender> _logger;

        public SmsTwilioSender(IOptions<TwilioConfig> twilioConfig, ISecretStoreReader secureReader, IMemoryCache memCache, ILogger<SmsTwilioSender> logger) : base(secureReader, memCache)
        {
            _twilioConfig = twilioConfig?.Value ?? throw new ArgumentNullException(nameof(twilioConfig),"Twilio configuration settings not found");

            if (string.IsNullOrWhiteSpace(_twilioConfig.LiveCredentials))
                throw new ArgumentException($"LiveCredentials configuration setting not found");

            if (string.IsNullOrWhiteSpace(_twilioConfig.TestCredentials))
                throw new ArgumentException($"TestCredentials configuration setting not found");

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrWhiteSpace(_twilioConfig.StatusCallbackUrl))
                _logger.LogWarning("Status callback URL not set. Twilio will not return status updates");


        }

        public SmsSenderType ProviderName => SmsSenderType.Twilio;


        public async Task<OutboundMessageLogEntry> SendSmsMessageAsync( SmsMessageRequest messageRequest)
        {
            _logger.LogInformation("Begin Twilio SendStatus Sms Message");

            if (messageRequest == null)
                throw new ArgumentNullException(nameof(messageRequest));


            if (string.IsNullOrWhiteSpace(messageRequest.DestinationNumber))
                throw new ArgumentNullException(nameof(messageRequest), "DestinationNumber cannot be empty or null");

            if (string.IsNullOrWhiteSpace(messageRequest.SourceNumber))
                throw new ArgumentNullException(nameof(messageRequest), "SourceNumber cannot be empty or null");

            if (string.IsNullOrWhiteSpace(messageRequest.Message))
                throw new ArgumentNullException(nameof(messageRequest), "Message cannot be empty or null");

            TwilioCredentials creds = await GetCredentialsAsync(_twilioConfig.LiveCredentials);

            if (creds == null)
                throw new ArgumentException($"Twilio credentials {_twilioConfig.LiveCredentials} not found"); 

 


            TwilioRestClient twilioRestClient = new TwilioRestClient(creds.AccountSid, creds.Token);

            Twilio.Types.PhoneNumber toPhone = new Twilio.Types.PhoneNumber(messageRequest.DestinationNumber);

            CreateMessageOptions msgCreateOptions = new CreateMessageOptions(toPhone)
            {
                Body = messageRequest.Message,
                From = messageRequest.SourceNumber
            };

            string statusCallbackUrl = _twilioConfig.StatusCallbackUrl;

            if (!string.IsNullOrWhiteSpace(statusCallbackUrl))
            {
                try
                {
                    msgCreateOptions.StatusCallback = new Uri(statusCallbackUrl);
                }
                catch(Exception ex)
                {
                    string errMsg = $"Error setting callback URL for Twilio message with setting {statusCallbackUrl}";
                    _logger.LogError(ex, errMsg);
                    throw new Exception(errMsg, ex);
                }

            }

            OutboundMessageLogEntry messageSendResult;

            try
            {
                MessageResource messageResponse = await MessageResource.CreateAsync(msgCreateOptions, twilioRestClient);

                messageSendResult = new OutboundMessageLogEntry
                {
                    LogTime = DateTime.UtcNow,
                    HttpStatusCode = (messageResponse.Status != MessageResource.StatusEnum.Failed &&
                                        messageResponse.Status != MessageResource.StatusEnum.Undelivered ?
                                        200 : 500),
                    ExtendedStatus = messageResponse.ErrorMessage,
                    ServiceMessageId = messageResponse.Sid
                };


                StringBuilder errorInfoText = new StringBuilder();

                if (messageResponse.Sid != null)
                {
                    errorInfoText.AppendLine($"Twilio message Sid: {messageResponse.Sid}");
                

                }
                errorInfoText.AppendLine($"Status: {messageResponse.Status}");

                if (messageResponse.ErrorCode != null || messageResponse.ErrorMessage != null)
                {
                    StringBuilder extendedStatus = new StringBuilder();

                    if (messageResponse.ErrorCode != null)
                    {
                        string errorCodeMessage = $"ErrorCode: {messageResponse.ErrorCode}";
                        errorInfoText.AppendLine(errorCodeMessage);
                        extendedStatus.Append(errorCodeMessage);
                    }

                    if (messageResponse.ErrorMessage != null)
                    {
                        string errorMessage = $"ErrorMessage: {messageResponse.ErrorMessage}";
                        errorInfoText.AppendLine(errorMessage);
                        extendedStatus.Append($" {errorMessage}");
                    }
                    messageSendResult.HttpStatusCode = 500;
                    messageSendResult.ExtendedStatus = extendedStatus.ToString();

                }

               

                // For now consider the send a success if it doesn't indicate an error
                if (messageSendResult.HttpStatusCode == 500)
                {
                    _logger.LogError(errorInfoText.ToString());
                    messageSendResult.SendStatus = MessageSendStatus.Error;                    
                    messageSendResult.IsException = true;
                }
                else
                {
                    _logger.LogInformation(errorInfoText.ToString());
                    messageSendResult.HttpStatusCode = 200;
                    messageSendResult.IsException = false;

                    StringBuilder twilioUpdateText = new StringBuilder();

                    twilioUpdateText.AppendLine($"Twilio status: {messageResponse.Status}");
                    twilioUpdateText.AppendLine($"Message resource: {messageResponse.Uri}");


                    if (!string.IsNullOrWhiteSpace(messageResponse.MessagingServiceSid))
                    {
                        twilioUpdateText.AppendLine($"Messaging service SID: {messageResponse.MessagingServiceSid}");
                    }

                    if (!string.IsNullOrWhiteSpace(messageResponse.NumSegments))
                    {
                        // Only log NumSegments if the count is not the single segment which is expected.
                        if (!messageResponse.NumSegments.Equals("1"))
                            twilioUpdateText.AppendLine($"NumSegments: {messageResponse.NumSegments}");
                    }


                    if (!string.IsNullOrWhiteSpace(messageResponse.NumMedia))
                    {
                        // Only log if the number of media segments is more than what's expected.
                        if(!messageResponse.NumMedia.Equals("0"))
                            twilioUpdateText.AppendLine($"NumMedia: {messageResponse.NumMedia}");
                    }

                    if (messageResponse.DateCreated != null)
                        twilioUpdateText.AppendLine($"DateCreated: {messageResponse.DateCreated}");


                    twilioUpdateText.AppendLine($"Direction: {messageResponse.Direction.ToString()}");

                    if (messageResponse.SubresourceUris!=null)
                    {
                        // If the sub resources count is 1, then it's a duplicate of the Url returned and logged
                        // by the Message resource entry. No need to store additional information unless
                        // there is more than one sub resource.
                        if (messageResponse.SubresourceUris.Count > 1)
                        {
                            twilioUpdateText.AppendLine("Subresources:");
                            foreach (string key in messageResponse.SubresourceUris.Keys)
                            {
                                twilioUpdateText.AppendLine($"  {key}:{messageResponse.SubresourceUris[key]}");
                            }
                        }
                    }


                    messageSendResult.ExtendedStatus = twilioUpdateText.ToString();
                    // Once the message is sent to Twilio, it is queue. 
                    // A call back returning from Twilio will confirm that it was sent, 
                    // and then confirm that it was delivered.
                    messageSendResult.SendStatus = MessageSendStatus.SentToDispatcher;
                }
            }
            catch( ApiException ex )
            {
                _logger.LogError("Received Twilio ApiException", ex);
                messageSendResult = new OutboundMessageLogEntry
                {
                    LogTime = DateTime.UtcNow,
                    HttpStatusCode = ex.Status,
                    SendStatus = MessageSendStatus.Error,
                    IsException = true,
                    ExtendedStatus = ex.Message
                };

            }
            catch (ApiConnectionException ex )
            {
                _logger.LogError("Received Twilio ApiConnectionException", ex);
                messageSendResult = new OutboundMessageLogEntry
                {
                    LogTime = DateTime.UtcNow,
                    HttpStatusCode = 500,
                    SendStatus = MessageSendStatus.Error,
                    IsException = true,
                    ExtendedStatus = ex.Message
                };
            }

            return messageSendResult;
        }
    }
}
