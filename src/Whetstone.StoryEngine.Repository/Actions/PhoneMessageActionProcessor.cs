using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Tracking;
using Whetstone.StoryEngine.Models.Conditions;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Story.Text;
using Whetstone.StoryEngine.Repository.Messaging;
using Whetstone.StoryEngine.Repository.Phone;

namespace Whetstone.StoryEngine.Repository.Actions
{
    public class PhoneMessageActionProcessor : PhoneActionProcessorBase, INodeActionProcessor
    {

        private readonly ILogger<PhoneMessageActionProcessor> _logger;
        private readonly Func<NotificationsDispatchTypeEnum, INotificationDispatcher> _dispatcherFunc;
        private readonly ITitleReader _titleReader;
        private readonly IPhoneInfoRetriever _phoneTypeRetriever;
        private readonly ISmsConsentRepository _consentRep;
     

        public PhoneMessageActionProcessor(ITitleReader titleReader,
                                                Func<NotificationsDispatchTypeEnum, INotificationDispatcher> dispatcherFunc,
                                                IPhoneInfoRetriever phoneTypeRetriever,
                                                ISmsConsentRepository consentRepo,
                                                ILogger<PhoneMessageActionProcessor> logger)
        {
            _titleReader = titleReader ?? throw new ArgumentNullException(nameof(titleReader));
           
            _phoneTypeRetriever = phoneTypeRetriever ?? throw new ArgumentNullException(nameof(phoneTypeRetriever));
            _dispatcherFunc= dispatcherFunc ?? throw new ArgumentNullException(nameof(dispatcherFunc));
            _consentRep = consentRepo ??
                          throw new ArgumentNullException(nameof(consentRepo));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<string> ApplyActionAsync(StoryRequest req, List<IStoryCrumb> crumbs, NodeActionData actionData)
        {
            StringBuilder phoneActionBuilder = new StringBuilder();

            if (req == null)
                throw new ArgumentNullException(nameof(req));

            if(req.SessionContext?.TitleVersion == null)
                throw new ArgumentNullException(nameof(req), "SessionContext.TitleVersion cannot be null");

            if (crumbs == null)
            {
                throw new ArgumentException(nameof(crumbs));
            }

            PhoneMessageActionData phoneData = (PhoneMessageActionData)actionData;

            // Check for title info in the action.  If it's not there, then try to grab it from the title.  Either way, it needs to
            // be available in one or the other for us to continue.
            PhoneInfo phoneTitleInfo = phoneData.PhoneInfo;

            if (phoneTitleInfo == null)
            {
                StoryPhoneInfo storyPhone = await _titleReader.GetPhoneInfoAsync( req.SessionContext.TitleVersion);

                if (storyPhone == null)
                    throw new Exception($"Story title {req.SessionContext.TitleVersion.ShortName} version {req.SessionContext.TitleVersion.Version} does not include phone info for sending messages");

                phoneTitleInfo = new PhoneInfo
                {
                    SmsService = storyPhone.SmsService, SourcePhone = storyPhone.SourcePhone
                };

            }

            if (phoneTitleInfo == null)
                throw new Exception($"No phone information found in phone action data or title {req.SessionContext.TitleVersion.ShortName} and version {req.SessionContext.TitleVersion.Version}");

            List<SelectedItem> selItems = crumbs.GetSelectedItems();

            string phoneNumber = null;

            if (string.IsNullOrWhiteSpace(phoneData.PhoneNumberSlot))
                throw new Exception("Phone message action could not be executed. Phone number slot name not provided.");
            
          
            SelectedItem phoneItem = selItems.FirstOrDefault(x => x.Name.Equals(phoneData.PhoneNumberSlot, StringComparison.OrdinalIgnoreCase));

            if (phoneItem == null)
                throw new Exception($"Phone slot {phoneData.PhoneNumberSlot} not found.");

            if (string.IsNullOrWhiteSpace(phoneItem.Value))
                throw new Exception($"Phone slot {phoneData.PhoneNumberSlot} found, but phone number value is missing.");

            phoneNumber = phoneItem.Value;
            

            // Grab Application, user, session and request ids from the incoming request

            if (string.IsNullOrWhiteSpace(req.ApplicationId))
                throw new Exception("applicationId is missing from StoryRequest");

            if (string.IsNullOrWhiteSpace(req.UserId))
                throw new Exception("userId is missing from StoryRequest");

            if (string.IsNullOrWhiteSpace(req.SessionId))
                throw new Exception("sessionId is missing from StoryRequest");

            if (string.IsNullOrWhiteSpace(req.RequestId))
                throw new Exception("requestId is missing from StoryRequest");


            if (string.IsNullOrWhiteSpace(phoneData.ConfirmationNameSlot))
                throw new Exception("Confirmation slot cannot be null or empty");

            SelectedItem confirmSlot = selItems.FirstOrDefault(x => x.Name.Equals(phoneData.ConfirmationNameSlot, StringComparison.OrdinalIgnoreCase));

            if (confirmSlot == null)
                throw new Exception($"required confirmationNameSlot {phoneData.ConfirmationNameSlot} not found");

            string confirmationName = confirmSlot.Value;

            // This is a critical check. If the confirmation name value is not present,
            // then there is no record of the user granting consent to receive a text message.
            if (string.IsNullOrWhiteSpace(confirmationName))
                throw new Exception($"required confirmationNameSlot {phoneData.ConfirmationNameSlot} does not have a value indicating consent was not provided.");

            // Format the phone number
            var (FormattedNumber, IsValid) = PhoneUtility.ValidateFormat(phoneNumber, req.Locale);

            if ((phoneData.Messages?.Any()).GetValueOrDefault(false))
            {
                ConditionInfo conInfo = new ConditionInfo(crumbs, req.Client);

                await BuildAndSendMessageObject(req, phoneActionBuilder, phoneTitleInfo, phoneData, selItems, FormattedNumber, confirmationName, conInfo);
            }


            return phoneActionBuilder.ToString();
        }

        private async Task BuildAndSendMessageObject(StoryRequest req, StringBuilder phoneActionBuilder, PhoneInfo phoneTitleInfo, PhoneMessageActionData phoneData, List<SelectedItem> selItems, string formattedNumber, string consentName, ConditionInfo conInfo)
        {


            DataPhone destPhone = await _phoneTypeRetriever.GetPhoneInfoAsync( formattedNumber);

            if(!destPhone.Id.HasValue)
                destPhone.Id = Guid.NewGuid();
            

            // If we got here, then the user has granted consent. Record it.
            UserPhoneConsent phoneConsent = new UserPhoneConsent
            {
                Id = Guid.NewGuid(),
                TitleClientUserId = req.SessionContext.EngineUserId.GetValueOrDefault(),
                TitleVersionId = req.SessionContext.TitleVersion.VersionId.GetValueOrDefault(),
                IsSmsConsentGranted = true,
                EngineRequestId = req.EngineRequestId,
                Name = consentName,
                SmsConsentDate = req.RequestTime,
                PhoneId = destPhone.Id.Value
            };

            NotificationSourcePhoneMessageAction notificationSource = new NotificationSourcePhoneMessageAction
            {
                Consent = phoneConsent
            };

            SmsNotificationRequest notificationRequest = new SmsNotificationRequest
            {
                Id = Guid.NewGuid(),
                SmsProvider = phoneData.PhoneInfo.SmsService,
                SourceNumber = phoneTitleInfo.SourcePhone,
                Source = notificationSource,
                DestinationNumberId = destPhone.Id
            };

            PhoneTypeEnum curPhoneType = destPhone.Type;

            bool supportsSms = PhoneUtility.PhoneSupportsSms(curPhoneType);

            bool isPrivacyEnabled = await _titleReader.IsPrivacyLoggingEnabledAsync(req.SessionContext.TitleVersion);

            if (supportsSms)
            {
                phoneActionBuilder.Append("PhoneMessageAction: ");

                foreach (PhoneMessage phoneMessage in phoneData.Messages)
                {
                    bool isConditionMet;
                    if ((phoneMessage.ConditionNames?.Count).GetValueOrDefault(0) > 0)
                    {

                        isConditionMet = await phoneMessage.ConditionNames.IsConditionMetAsync(conInfo,
                            _titleReader, req.SessionContext.TitleVersion);
                    }
                    else
                    {
                        isConditionMet = true;
                    }

                    if (isConditionMet)
                    {

                        string sendText =
                            await MacroProcessing.ProcessTextFragmentMacrosAsync(phoneMessage.Message, selItems,
                                _logger);

                        if(notificationRequest.TextMessages==null)
                            notificationRequest.TextMessages = new List<TextFragmentBase>();

                        if(!string.IsNullOrWhiteSpace(sendText))
                            notificationRequest.TextMessages.Add(new SimpleTextFragment(sendText));                   

                        if ((phoneMessage.Tags?.Any()).GetValueOrDefault(false))
                        {
                            if(notificationRequest.Tags == null)
                                notificationRequest.Tags = new Dictionary<string, string>();

                            foreach (var  sourceKey in phoneMessage.Tags.Keys)
                            {
                                notificationRequest.Tags.Add(sourceKey, phoneMessage.Tags[sourceKey]);
                            }
                        }


                        if(isPrivacyEnabled)
                            phoneActionBuilder.AppendLine(
                              $"Queueing message to destination number {notificationRequest.DestinationNumberId} from {notificationRequest.SourceNumber}: (sendtext redacted)");
                        else
                            phoneActionBuilder.AppendLine(
                                $"Queueing message to destination number {notificationRequest.DestinationNumberId} from {notificationRequest.SourceNumber}: {sendText}");
                    }
                    else
                    {
                        if(isPrivacyEnabled)
                            phoneActionBuilder.AppendLine(
                                $"Conditions not met. Message is not being sent to phone number {notificationRequest.DestinationNumberId} from {notificationRequest.SourceNumber}: (sendtext redacted)");
                        else
                            phoneActionBuilder.AppendLine(
                                $"Conditions not met. Message is not being sent to phone number {notificationRequest.DestinationNumberId} from {notificationRequest.SourceNumber}: {phoneMessage.Message}");
                    }

                }

                // Save the phone info record to dynamodb now that the user has granted consent.
                // This will cause it to replicate over to the database
                await _phoneTypeRetriever.SaveDatabasePhoneInfoAsync(destPhone);

                // Then save the consent record to dynamodb. The sequence is important. The phone record will be replicated first,
                // followed by the consent record which depends on the phone record.
                await _consentRep.SaveConsentAsync(phoneConsent);

                INotificationDispatcher dispatcher = _dispatcherFunc(NotificationsDispatchTypeEnum.StepFunction);

                await dispatcher.DispatchNotificationAsync(notificationRequest);

            }
            else
            {
                string message = isPrivacyEnabled
                    ? $"PhoneMessageAction: Destination Phone Number id: {notificationRequest.DestinationNumberId} does not support SMS Messaging"
                    : $"PhoneMessageAction: Destination Phone Number id: {notificationRequest.DestinationNumberId} is phone type: {curPhoneType} and does not support SMS Messaging";

                _logger.LogInformation(message);
                phoneActionBuilder.Append(message);
            }
        }



    }
}
