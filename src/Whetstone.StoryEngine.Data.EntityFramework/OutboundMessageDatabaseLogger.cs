
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Models.Story.Text;
using Whetstone.StoryEngine.Repository.Messaging;
using Whetstone.StoryEngine.Repository.Phone;

namespace Whetstone.StoryEngine.Data.EntityFramework
{
    public class OutboundMessageDatabaseLogger : IOutboundMessageLogger
    {

        private readonly IUserContextRetriever _userContextRetriever;

        private readonly IPhoneInfoRetriever _phoneRetriever = null;
        private readonly ILogger<OutboundMessageDatabaseLogger> _logger;
        private readonly ISmsConsentRepository _smsConsentRepo;


        public OutboundMessageDatabaseLogger(IUserContextRetriever userContextRetriever, IPhoneInfoRetriever phoneRetriever, Func<SmsConsentRepositoryType, ISmsConsentRepository> consentFunc, ILogger<OutboundMessageDatabaseLogger> logger)
        {
            _userContextRetriever = userContextRetriever ??
                                    throw new ArgumentNullException($"{nameof(userContextRetriever)}");
            _phoneRetriever = phoneRetriever ?? throw new ArgumentNullException($"{nameof(phoneRetriever)}");

            if (consentFunc == null)
                throw new ArgumentNullException($"{nameof(consentFunc)}");


            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");

            _smsConsentRepo = consentFunc(SmsConsentRepositoryType.Database) ?? throw new ArgumentNullException($"Database implementation of the consent repository not found");
        }

        public async Task SaveOutboundLogMessages(List<OutboundMessageLogEntry> logEntries)
        {
            if (logEntries == null)
                throw new ArgumentNullException($"{nameof(logEntries)}");

            foreach (var logEntry in logEntries)
            {
                using var userContext = await _userContextRetriever.GetUserDataContextAsync();
                Stopwatch messageSaveTime = new Stopwatch();
                messageSaveTime.Start();
                await userContext.AddOutboundMessageLog(logEntry);
                messageSaveTime.Stop();
                _logger.LogInformation($"Duration to save log message {logEntry.Id}: {messageSaveTime.ElapsedMilliseconds}ms");
            }
        }

        public async Task UpdateOutboundMessageBatchAsync(OutboundBatchRecord message)
        {
            if (message == null)
                throw new ArgumentNullException($"{nameof(message)}");

            // ApplyIdentifiers(message);
            _logger.LogDebug($"Updating message batch {message.Id}");

            using IUserDataContext ucx = await _userContextRetriever.GetUserDataContextAsync();
            Stopwatch saveTime = new Stopwatch();
            saveTime.Start();
            ucx.Update<OutboundBatchRecord>(message);
            await ucx.SaveChangesAsync();
            saveTime.Stop();

            _logger.LogInformation($"Saved outbound sms message {message.Id} in {saveTime.ElapsedMilliseconds}ms");
        }

        public async Task<OutboundBatchRecord> ProcessNotificationRequestAsync(SmsNotificationRequest smsRequest)
        {


            if (smsRequest == null)
                throw new ArgumentNullException($"{nameof(smsRequest)}");

            Guid destinationNumberId;

            if (smsRequest.DestinationNumberId.HasValue)
                destinationNumberId = smsRequest.DestinationNumberId.Value;
            else
                throw new ArgumentException($"Property DestinationNumber on {nameof(smsRequest)} cannot be null or empty");


            DataPhone destPhone = await _phoneRetriever.GetPhoneInfoAsync(destinationNumberId);


            if (destPhone == null)
                throw new Exception($"Phone number not found for destination phone id {destinationNumberId}");

            if (!destPhone.Id.HasValue)
                throw new Exception($"Destination phone does not have a valid identifier");


            string destinationNumber = destPhone.PhoneNumber;


            _logger.LogInformation($"Processing log request SMS request for destination phone number id {destinationNumberId}");

            OutboundBatchRecord outboundBatchRecord = new OutboundBatchRecord
            {
                Id = Guid.NewGuid(),
                AllSent = false,
                SmsProvider = smsRequest.SmsProvider,
                SendAttemptsCount = 0
            };
            //outboundRecord.EngineRequestId = smsRequest.

            bool canGetSms = PhoneUtility.PhoneSupportsSms(destPhone.Type);
            outboundBatchRecord.SentToPhone = destPhone;
            outboundBatchRecord.SmsToNumberId = destPhone.Id.Value;


            if (!string.IsNullOrWhiteSpace(smsRequest.SourceNumber))
            {
                // Attempt to get the source phone number from the database

                DataPhone sourcePhone = await GetPhoneFromDatabaseAsync(smsRequest.SourceNumber);

                if (sourcePhone == null)
                {
                    sourcePhone = await _phoneRetriever.GetPhoneInfoAsync(smsRequest.SourceNumber);
                    if (sourcePhone.CreateDate == default)
                    {
                        sourcePhone.CreateDate = DateTime.UtcNow;
                    }

                    if (!sourcePhone.Id.HasValue)
                    {
                        sourcePhone.Id = Guid.NewGuid();
                    }

                    // Save phone to database
                    await SavePhoneAsync(sourcePhone);

                }

                outboundBatchRecord.SentFromPhone = sourcePhone;
                outboundBatchRecord.SmsFromNumberId = sourcePhone.Id;

            }

            outboundBatchRecord.Messages = new List<OutboundMessagePayload>();
            foreach (TextFragmentBase baseFrag in smsRequest.TextMessages)
            {

                if (baseFrag is SimpleTextFragment simpleFrag)
                {
                    OutboundMessagePayload outboundPayload = new OutboundMessagePayload
                    {
                        Id = Guid.NewGuid(),
                        OutboundBatchRecordId = outboundBatchRecord.Id,
                        Message = simpleFrag.Text,

                        Results = new List<OutboundMessageLogEntry>()
                    };


                    // apply the tags to every request.
                    if ((smsRequest.Tags?.Any()).GetValueOrDefault(false))
                    {
                        outboundPayload.Tags = new Dictionary<string, string>();
                        foreach (var tagName in smsRequest.Tags.Keys)
                        {
                            outboundPayload.Tags.Add(tagName, smsRequest.Tags[tagName]);
                        }
                    }


                    OutboundMessageLogEntry result = new OutboundMessageLogEntry
                    {
                        // Id = Guid.NewGuid(),
                        OutboundMessageId = outboundPayload.Id,
                        SendStatus = MessageSendStatus.Requested
                    };

                    if (canGetSms)
                    {
                        result.IsException = false;
                        result.SendStatus = MessageSendStatus.Requested;
                        result.ExtendedStatus = "Message received by workflow";
                    }
                    else
                    {
                        result.IsException = true;
                        result.SendStatus = MessageSendStatus.Error;
                        result.ExtendedStatus =
                            $"Requested destination phone number {destinationNumber} cannot receive SMS messages";
                    }

                    result.LogTime = DateTime.UtcNow;
                    outboundPayload.Status = result.SendStatus;
                    outboundPayload.Results.Add(result);
                    outboundBatchRecord.Messages.Add(outboundPayload);
                }
            }

            INotificationSource notificationSource = smsRequest.Source;
            if (notificationSource is NotificationSourcePhoneMessageAction)
            {
                NotificationSourcePhoneMessageAction notificationPhoneMessageAction = notificationSource as NotificationSourcePhoneMessageAction;

                // Get the consent grant from the notification source.

                UserPhoneConsent userConsent = notificationPhoneMessageAction.Consent;

                // TODO -- makes sure the mock saves the consent to the database.
                // Save the consent record to the database.
                await _smsConsentRepo.SaveConsentAsync(userConsent);

                // Since the request originated from a title action, a user initiated it. 
                // Assign the user id to the outbound record.
                outboundBatchRecord.TitleUserId = userConsent.TitleClientUserId;

                // The phone message notification source includes the consent.
                outboundBatchRecord.ConsentId = userConsent.Id;

                // apply the engine request id. 
                outboundBatchRecord.EngineRequestId = notificationPhoneMessageAction.Consent.EngineRequestId;
            }
            else if (notificationSource is InboundSmsNotification)
            {
                InboundSmsNotification smsNotification = notificationSource as InboundSmsNotification;

                // Since the request originated from a title action, a user initiated it. 
                // Assign the user id to the outbound record.
                outboundBatchRecord.TitleUserId = smsNotification.EngineUserId;

                // The phone message notification source includes the consent.
                outboundBatchRecord.ConsentId = smsNotification.ConsentId;

            }

            try
            {
                await SaveProcessedOutboundSmsMessageAsync(outboundBatchRecord);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error attempting to save outbound record for message sent to phone id {destinationNumberId}");
                throw;
            }

            return outboundBatchRecord;
        }

        private async Task SavePhoneAsync(DataPhone sourcePhone)
        {
            try
            {
                using var userContext = await _userContextRetriever.GetUserDataContextAsync();
                await userContext.UpsertPhoneInfoAsync(sourcePhone);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding send phone number to database with {sourcePhone.PhoneNumber}");
                throw;
            }

        }

        private async Task<DataPhone> GetPhoneFromDatabaseAsync(string phoneNumber)
        {
            DataPhone foundPhone = null;
            try
            {
                using var ucx = await _userContextRetriever.GetUserDataContextAsync();
                foundPhone = await ucx.PhoneNumbers.FirstOrDefaultAsync(x => x.PhoneNumber.Equals(phoneNumber));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting send phone number from record from database with {phoneNumber}");
                throw;
            }


            return foundPhone;
        }

        private async Task SaveProcessedOutboundSmsMessageAsync(OutboundBatchRecord message)
        {

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (!message.TitleUserId.HasValue)
            {
                throw new ArgumentNullException(nameof(message), "TitleUserId property cannot be null");
            }

            if (message.Id == default)
            {
                throw new ArgumentNullException(nameof(message), "Id property must be a valid value");
            }

            if (message.SmsToNumberId == default)
            {
                throw new ArgumentNullException(nameof(message), "SmsToNumberId property must be a valid value");
            }

            if (message.ConsentId == default)
            {
                throw new ArgumentNullException(nameof(message), "ConsentId property must be a valid value");
            }

            using IUserDataContext ucx = await _userContextRetriever.GetUserDataContextAsync();


            // The phone numbers are already saved. Detach them.
            ucx.OutboundMessageBatch.AddOrUpdate<OutboundBatchRecord>(message);

            if (message.SentToPhone != null)
                ucx.Entry(message.SentToPhone).State = EntityState.Unchanged;

            if (message.SentFromPhone != null)
                ucx.Entry(message.SentFromPhone).State = EntityState.Unchanged;

            Stopwatch databaseSaveTime = new Stopwatch();
            databaseSaveTime.Start();
            await ucx.SaveChangesAsync();
            databaseSaveTime.Stop();

            _logger.LogInformation($"Saved outbound sms message {message.Id} in {databaseSaveTime.ElapsedMilliseconds}ms");

            message.IsSaved = true;
        }

    }
}
