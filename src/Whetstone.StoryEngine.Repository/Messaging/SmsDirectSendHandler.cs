
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Story;
using System.Threading;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.Repository.Messaging;
using Microsoft.Extensions.Options;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Repository.Phone;

namespace Whetstone.StoryEngine.OutboutSmsSender
{


    /// <summary>
    /// Used to send the message directly from the StoryEngine.
    /// </summary>
    public class SmsDirectSendHandler: ISmsHandler
    {
        private readonly ILogger<SmsDirectSendHandler> _logger;


        private Func<SmsSenderType, ISmsSender> _smsFunc;

        private const int DEFAULT_throttleRetryMax = 3;
        private const int DEFAULT_messageDelay = 1500;
        private MessagingConfig _messagingConfig;
        private IPhoneInfoRetriever _phoneRetriever;

        public SmsDirectSendHandler(IOptions<MessagingConfig> messageConfig, Func<SmsSenderType, ISmsSender> smsFunc, IPhoneInfoRetriever phoneRetriever, ILogger<SmsDirectSendHandler> logger)
        {
            if (messageConfig == null)
                throw new ArgumentNullException("messageConfig cannot be null");

            if (messageConfig.Value == null)
                throw new ArgumentNullException("messageConfig.Value cannot be null");

            _phoneRetriever = phoneRetriever ?? throw new ArgumentNullException($"{nameof(phoneRetriever)} cannot be null or empty");

            _smsFunc = smsFunc ?? throw new ArgumentNullException($"{nameof(smsFunc)} cannot be null");

            _messagingConfig = messageConfig.Value;

            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");
        }





        public async Task<OutboundBatchRecord> SendOutboundSmsMessagesAsync(OutboundBatchRecord message)
        {

            if (message == null)
                throw new ArgumentNullException("message cannot be null");



            SmsSenderType senderType = message.SmsProvider.GetValueOrDefault(SmsSenderType.Unassigned);

            ISmsSender smsSender = _smsFunc(senderType);


            message.SendAttemptsCount++;
            message.SmsProvider = smsSender.ProviderName;

            _logger.LogDebug($"Loading Sms Sender SmsProvider: {smsSender.ProviderName}");

            int sentCount = 0;

            if ((message.Messages?.Any()).GetValueOrDefault(false))
            {

                bool continueSending = true;
                int messageIndex = 0;
                int throttleCount = 0;


                while (continueSending)
                {
                    _logger.LogDebug($"Getting message index {messageIndex}");

                    var messagePayload = message.Messages[messageIndex];


                    if (!HasMessageBeenSuccessfullySent(messagePayload))
                    {
                        try
                        {

                            DataPhone toPhoneInfo = message.SentToPhone;
                            if (toPhoneInfo == null)
                            {

                                if (message.SmsToNumberId.HasValue)
                                {
                                    toPhoneInfo = await _phoneRetriever.GetPhoneInfoAsync(message.SmsToNumberId.Value);
                                }
                                else
                                {
                                    throw new Exception("Destination phone number is not provided");
                                }
                            }

                            SmsMessageRequest msgReq = new SmsMessageRequest
                            {
                                Message = messagePayload.Message,
                                DestinationNumber = toPhoneInfo.PhoneNumber,
                                Tags = messagePayload.Tags
                            };

                            if (message.SentFromPhone != null)
                            {
                                msgReq.SourceNumber= message.SentFromPhone.PhoneNumber;
                            }


                            Stopwatch outMessageTotalTime = new Stopwatch();
                            outMessageTotalTime.Start();
                            OutboundMessageLogEntry messageSendResult = await smsSender.SendSmsMessageAsync( msgReq);
                            outMessageTotalTime.Stop();
                            messageSendResult.ProviderSendDuration = outMessageTotalTime.ElapsedMilliseconds;
                            messageSendResult.OutboundMessageId = message.Id;


                            // Apply the message id returned by the SMS provider to the message payload.
                            // This bubbles the SMS provider-specific SMS message identifier from the log entry
                            // to the individual text message the send request applies to
                            if (!string.IsNullOrEmpty(messageSendResult.ServiceMessageId))
                            {
                                messagePayload.ProviderMessageId = messageSendResult.ServiceMessageId;
                            }

                            switch (messageSendResult.SendStatus)
                            {
                                case MessageSendStatus.SentToDispatcher:
                                    throttleCount = 0;
                                    _logger.LogInformation($"Message payload in message {message.Id} sent to dispatcher: {messagePayload.Message}");
                                    sentCount++;
                               
                                    // Move to the next message on next iteration. This is the only condition that should increment.
                                    messageIndex++;
                                    continueSending = messageIndex < message.Messages.Count;

                                    break;
                                case MessageSendStatus.Throttled:

                                    throttleCount++;
                                    messageSendResult.IsException = true;
                                    if (throttleCount >= _messagingConfig.ThrottleRetryLimit)
                                    {
                                        // Do not count this as a retry. Let it go back into the workflow and wait without a retry penalty
                                        message.SendAttemptsCount--;
                                        continueSending = false;
                                        _logger.LogError($"Message {message.Id} was throttled {throttleCount} time(s) - throttle limit reached: {messagePayload.Message}");
                                    }
                                    else
                                    {
                                        _logger.LogError($"Message {message.Id} was throttled {throttleCount} time(s): {messagePayload.Message}");
                                    }
                                    break;
                                case MessageSendStatus.Error:
                        
                                    _logger.LogInformation($"Unexpected response {messageSendResult.ExtendedStatus} for message {message.Id} on message: {messagePayload.Message}");
                                    throttleCount = 0;
                                    continueSending = false;
                                    messageSendResult.IsException = true;
                                    break;
                            }

                            if (messagePayload.Results == null)
                                messagePayload.Results = new List<OutboundMessageLogEntry>();

                            messagePayload.Status = messageSendResult.SendStatus;
                            messagePayload.Results.Add(messageSendResult);
                        }
                        catch (Exception ex)
                        {
                            throttleCount = 0;
                            continueSending = false;
                            // Record the exception
                            LogSendError(message, messagePayload, ex);
     
                        }

                    }
                    else
                    {
                        messageIndex++;
                        sentCount++;
                        _logger.LogInformation($"Already successfully sent message to {message.SmsToNumberId} from {message.SmsFromNumberId}: {messagePayload.Message}");
                    }


                }

                // If there were no errors and a message was throttled, then don't increment the retry count.
                //if (!hasUnexpectedException && isThrottled)
                //    message.SendAttemptsCount--;

                message.AllSent = sentCount == message.Messages.Count;

            }
            else
            {
                message.AllSent = true;
            }




            return message;
        }

        private void LogSendError(OutboundBatchRecord message, OutboundMessagePayload messagePayload, Exception ex)
        {
            OutboundMessageLogEntry unhandledResult = new OutboundMessageLogEntry
            {
                 LogTime = DateTime.UtcNow,
                IsException = true,
                ExtendedStatus = ex.ToString()
            };

            if (messagePayload.Results == null)
                messagePayload.Results = new List<OutboundMessageLogEntry>();

            messagePayload.Results.Add(unhandledResult);

            string errMsg = $"Error sending message for user id {message.TitleUserId}, request id {message.EngineRequestId} and messageid {message.Id} to {message.SmsToNumberId} from {message.SmsFromNumberId}: {messagePayload.Message}";
            _logger.LogError(ex, errMsg);
        }

        

        private  bool HasMessageBeenSuccessfullySent(OutboundMessagePayload payload)
        {
            bool messageSuccessfullySent = false;

            if ((payload?.Results?.Any()).GetValueOrDefault(false))
            {
                var foundVal = payload.Results.FirstOrDefault(x => x.SendStatus == MessageSendStatus.SentToDispatcher);
                messageSuccessfullySent = foundVal != null;
            }

            return messageSuccessfullySent;
        }


        private bool WasLastMessageSendSuccessful(OutboundMessagePayload payload)
        {

            bool wasLastMessageSent = false;
            

            if ((payload.Results?.Any()).GetValueOrDefault(false))
            {

                OutboundMessageLogEntry result = payload.Results.Last();

                // The message status could be queued at the delivery provider or delivered.
                // Either case shows that the message has been sent to the delivery provider.
                // If it Twilio, then it will show as queued.

                wasLastMessageSent = result == null ? false :
                    (result.SendStatus == MessageSendStatus.SentToDispatcher);
            }


            return wasLastMessageSent;
        }

    }
}
