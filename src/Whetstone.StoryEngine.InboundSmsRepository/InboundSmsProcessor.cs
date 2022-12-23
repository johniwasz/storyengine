using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.Repository.Messaging;
using Whetstone.StoryEngine.Repository.Phone;

namespace Whetstone.StoryEngine.InboundSmsRepository
{
    internal enum ConsentStatus
    {
        Granted = 1,
        Revoked = 2,
        Unknown = 3
    }

    public class InboundSmsProcessor : IInboundSmsProcessor
    {
        internal static readonly string GRANTCONSENTINTENT = "GrantConsentIntent";
        private readonly Microsoft.Extensions.Logging.ILogger _logger;


        private readonly IAppMappingReader _appReader;
        private readonly IStoryRequestProcessor _requestProcessor;
        private readonly ISmsConsentRepository _consentRepo;
        private readonly IPhoneInfoRetriever _phoneRep;
        private readonly ITitleReader _titleReader;
        private readonly IStoryUserRepository _userRepo;
        private readonly ISmsSender _smsSender;
        private readonly ITwilioVerifier _twilioVerifier;
        private readonly ISessionLogger _sessionLogger;
        private readonly INotificationDispatcher _notifDispatcher;


        public InboundSmsProcessor(IAppMappingReader appReader, IStoryRequestProcessor requestProcessor, ISmsConsentRepository consentRepo,
            IPhoneInfoRetriever phoneRep, ITitleReader titleReader, ISmsSender smsSender, ITwilioVerifier twilioVerifier, ISessionLogger sessionLogger,
            Func<UserRepositoryType, IStoryUserRepository> userRepFunc,
            Func<NotificationsDispatchTypeEnum, INotificationDispatcher> sendFunction,
            ILogger<InboundSmsProcessor> logger)
        {
            _appReader = appReader ?? throw new ArgumentNullException(nameof(appReader));
            _requestProcessor = requestProcessor ?? throw new ArgumentNullException(nameof(requestProcessor));
            _consentRepo = consentRepo ?? throw new ArgumentNullException(nameof(consentRepo));
            _phoneRep = phoneRep ?? throw new ArgumentNullException(nameof(phoneRep));
            _titleReader = titleReader ?? throw new ArgumentNullException(nameof(titleReader));
            _smsSender = smsSender ?? throw new ArgumentNullException(nameof(smsSender));
            _twilioVerifier = twilioVerifier ?? throw new ArgumentNullException(nameof(twilioVerifier));
            _sessionLogger = sessionLogger ?? throw new ArgumentNullException(nameof(sessionLogger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (userRepFunc==null)
                throw new ArgumentNullException(nameof(userRepFunc));

            _userRepo = userRepFunc(UserRepositoryType.DynamoDB) ??
                        throw new ArgumentException($"Could not get the DynamoDB user repository type");

            if(sendFunction == null)
                throw new ArgumentNullException(nameof(sendFunction));

            _notifDispatcher = sendFunction(NotificationsDispatchTypeEnum.StepFunction) ??
                    throw new ArgumentException($"Could not get the StepFunction notification dispatcher");

        }


        public async Task<INotificationRequest> ProcessInboundSmsMessageAsync(InboundSmsMessage msg)
        {

            if (msg == null)
                throw new ArgumentNullException($"{nameof(msg)}");

            INotificationRequest notifRequest = null;

            try
            {
                SmsSenderType senderType = GetSmsSenderType(msg);
                NameValueCollection bodyCol = HttpUtility.ParseQueryString(msg.Body);

                IDictionary<string, string> bodyValues = bodyCol.ToDictionary();


                if (senderType == SmsSenderType.Twilio)
                {
                    // Validate the message
                    bool isValid = await _twilioVerifier.ValidateTwilioMessageAsync(msg.Path, msg.Headers, bodyValues, msg.Alias);
                    if (!isValid)
                    {
                        throw new Exception("Twilio request could not be verified.");
                    }
                }

                StoryRequest storyReq = await ToStoryRequestAsync(bodyValues, msg.Alias);
                // Get the story user id
                var consentResult = await GetConsentAsync(storyReq);
                ConsentStatus conStatus = consentResult.Item1;

                // If the consent is blocked, then treat the incoming request as a ping request,
                // unless the user is granting consent again.
                if (conStatus == ConsentStatus.Revoked && !storyReq.Intent.Equals(GRANTCONSENTINTENT))
                    storyReq.IsPingRequest = true;

                StoryResponse storyResp = await _requestProcessor.ProcessStoryRequestAsync(storyReq);

                // before sending the response, verify if the use has granted consent.
                Guid? consentId;
                consentResult = await GetConsentAsync(storyReq, false);
                consentId = consentResult.Item2;


                if (!((storyReq.IsPingRequest).GetValueOrDefault(false)))
                {
                    if (senderType == SmsSenderType.Twilio)
                    {
                        if (storyReq.RequestType != StoryRequestType.Stop)
                            notifRequest = await DispatchMessageAsync(storyReq, storyResp, senderType, consentId);
                    }
                    else
                        notifRequest= await DispatchMessageAsync( storyReq, storyResp, senderType, consentId);


                    await _sessionLogger.LogRequestAsync(storyReq, storyResp);
                }



            }
            catch (Exception ex)
            {

                string inboundMsgText = JsonConvert.SerializeObject(msg);

                _logger.LogError(ErrorEvents.InboundSmsProcessingError, ex, $"Error processing message: {inboundMsgText}");

                // Must rethrow so that SQS puts the message back on the queue in the event of a failure.
                throw;
            }

            return notifRequest;
        }



        private SmsSenderType GetSmsSenderType(InboundSmsMessage msg)
        {
            var senderType = msg.SmsType.GetValueOrDefault(_smsSender.ProviderName);

            return senderType;
        }


        private async Task<StoryRequest> ToStoryRequestAsync(IDictionary<string, string> bodyCol, string alias)
        {

            string rawText = bodyCol["Body"];
            string sourceNumber = bodyCol["From"];
            string destinationNumber = bodyCol["To"];
            string smsSid = bodyCol["SmsMessageSid"];
            string accountSid = bodyCol["AccountSid"];


            TitleVersion titleVer = await _appReader.GetTitleAsync(Client.Sms, destinationNumber, alias);

            if (titleVer == null)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append($"No SMS title found for client {destinationNumber}");
                if (!string.IsNullOrWhiteSpace(alias))
                    builder.Append($" and alias {alias}");

                throw new Exception(builder.ToString());
            }

            StoryRequest req = new StoryRequest
            {
                RequestTime = DateTime.UtcNow,
                ApplicationId = destinationNumber,

                RequestId = smsSid,
                SessionId = accountSid,
                UserId = sourceNumber,
                EngineRequestId = Guid.NewGuid(),
                Client = Client.Sms,
                InputType = UserInputType.Keyboard
            };

            var (IntentName, RequestType) = GetIntent(rawText);
            req.Intent = IntentName;
            req.RequestType = RequestType;

            req.IsNewSession = true;
            // TO DO get the local from the phone number
            req.Locale = "en-US";
            req.RawText = rawText;


            req.SessionContext = new EngineSessionContext
            {
                TitleVersion = titleVer,
                
                EngineSessionId = Guid.NewGuid()
            };

            var foundUser = await _userRepo.GetUserAsync(req);
            req.SessionContext.EngineUserId = foundUser.Id;

            return req;
        }


        private (string IntentName, StoryRequestType RequestType) GetIntent(string text)
        {
            string textVal = text ?? "";
            StoryRequestType reqType = StoryRequestType.Launch;

            string intent;
            if (textVal.Equals("YES", StringComparison.OrdinalIgnoreCase) ||
textVal.Equals("RESUME", StringComparison.OrdinalIgnoreCase) ||
textVal.Equals("UNSTOP", StringComparison.OrdinalIgnoreCase) ||
textVal.Equals("START", StringComparison.OrdinalIgnoreCase))
            {
                intent = GRANTCONSENTINTENT;
            }
            else if (textVal.Equals("NO", StringComparison.OrdinalIgnoreCase))
            {
                intent = "NoIntent";
            }
            else if (textVal.Equals("STOP", StringComparison.OrdinalIgnoreCase) ||
                     textVal.Equals("STOPALL", StringComparison.OrdinalIgnoreCase) ||
                     textVal.Equals("UNSUBSCRIBE", StringComparison.OrdinalIgnoreCase) ||
                     textVal.Equals("CANCEL", StringComparison.OrdinalIgnoreCase) ||
                     textVal.Equals("END", StringComparison.OrdinalIgnoreCase) ||
                     textVal.Equals("QUIT", StringComparison.OrdinalIgnoreCase))
            {
                reqType = StoryRequestType.Stop;
                intent = "StopIntent";
            }
            else if (textVal.Equals("HELP", StringComparison.OrdinalIgnoreCase))
            {
                reqType = StoryRequestType.Help;
                intent = "HelpIntent";
            }

            else if (textVal.Equals("RESUME", StringComparison.OrdinalIgnoreCase) ||
                     textVal.Equals("UNSTOP", StringComparison.OrdinalIgnoreCase))
            {
                intent = "ResumeIntent";
            }
            else
            {
                intent = "FallbackIntent";
            }


            return (intent, reqType);

        }


        private async Task<INotificationRequest> DispatchMessageAsync( StoryRequest storyReq, StoryResponse storyResp, SmsSenderType senderType, Guid? consentId)
        {
            InboundSmsNotification notificationSource = new InboundSmsNotification
            {
                // TODO -- validate a consent
                //notificationSource.TitleUserId = storyReq.SessionContext.EngineUserId.Value;
                //notificationSource.EngineRequestId = storyReq.EngineRequestId;
                //notificationSource.TitleVersionId = storyReq.SessionContext.TitleVersion.VersionId.GetValueOrDefault();
                //notificationSource.ConsentDate = DateTime.UtcNow;
                //notificationSource.ConsentName = "whetstonesms";

                EngineUserId = storyReq.EngineRequestId,
                ConsentId = consentId
            };

            SmsNotificationRequest notificationRequest = new SmsNotificationRequest
            {
                Id = Guid.NewGuid(),
                SmsProvider = senderType,
                SourceNumber = storyReq.ApplicationId,
                Source = notificationSource,
                TextMessages = storyResp.LocalizedResponse.GeneratedTextResponse
            };


            //if (!message.TitleUserId.HasValue)
            //    throw new ArgumentException($"{nameof(message)} TitleUserId property cannot be null");

            //if (message.Id == default(Guid))
            //    throw new ArgumentException($"{nameof(message)} Id property must be a valid value");

            //if (message.SmsToNumberId == default(Guid))
            //    throw new ArgumentException($"{nameof(message)} SmsToNumberId property must be a valid value");

            //if (message.ConsentId == default(Guid))
            //    throw new ArgumentException($"{nameof(message)} ConsentId property must be a valid value");

            DataPhone destPhone = await _phoneRep.GetPhoneInfoAsync(storyReq.UserId);
            notificationRequest.DestinationNumberId = destPhone.Id;


            await _notifDispatcher.DispatchNotificationAsync(notificationRequest);

            return notificationRequest;

        }

        private async Task<(ConsentStatus, Guid? )> GetConsentAsync(StoryRequest storyReq, bool includePreconditionConsents = true)
        {
            if (storyReq == null)
            {
                throw new ArgumentNullException(nameof(storyReq));
            }

            _ = (storyReq.SessionContext?.EngineUserId).GetValueOrDefault(Guid.Empty);

            ConsentStatus isConsentGranted;

            TitleVersion titleVer = storyReq.SessionContext?.TitleVersion;
            string phoneNumber = storyReq.UserId;

            StoryPhoneInfo phoneInfo = await _titleReader.GetPhoneInfoAsync(titleVer);
            if (phoneInfo == null)
            {
                throw new Exception($"Mapped title {titleVer.ShortName} and {titleVer.Version} with deployment id {titleVer.DeploymentId} does not have phone information defined. PhoneInfo is required for a title that supports an SMS endpoint.");
            }

            if (string.IsNullOrWhiteSpace(phoneInfo.ConsentName))
            {
                throw new Exception($"Mapped title {titleVer.ShortName} and {titleVer.Version} with deployment id {titleVer.DeploymentId} does not have a consent name configured in phone info. A consent name is required for a title that supports an SMS endpoint.");
            }


            // If the required consents don't have to be checked,
            // then assume they passed for the next step.
            bool reqConsent = true;

            var phoneData = await _phoneRep.GetPhoneInfoAsync(phoneNumber);
        
            Guid? consentId = null;

            // This should be done when calling into the title for the first time. 
            // The precondition check ensures consent was granted from another 
            if (phoneInfo.RequiredConsents != null && includePreconditionConsents)
            {

                int reqConsentIndex = 0;

                while (reqConsent && reqConsentIndex < phoneInfo.RequiredConsents.Count)
                {
                    string requiredConsent = phoneInfo.RequiredConsents[reqConsentIndex];
                    UserPhoneConsent reqConsentResult =
                        await _consentRepo.GetConsentAsync(requiredConsent, phoneData.Id.Value);

                    reqConsent = (reqConsentResult?.IsSmsConsentGranted).GetValueOrDefault(false);

                    if (reqConsent)
                    {
                        reqConsentIndex++;
                        consentId = reqConsentResult?.Id;
                    }

                }
            }


            // If required consents were found and granted or if they did not need to be 
            // checked, then continue.
            if (reqConsent)
            {
                UserPhoneConsent phoneConsent = await _consentRepo.GetConsentAsync(phoneInfo.ConsentName, phoneData.Id.Value);

                if (phoneConsent == null)
                {
                    // User has not yet rendered consent. 
                    isConsentGranted = ConsentStatus.Unknown;
                }
                else
                {
                    if ((phoneConsent?.IsSmsConsentGranted).GetValueOrDefault(false))
                    {
                        consentId = phoneConsent.Id;
                        isConsentGranted = ConsentStatus.Granted;
                    }
                    else
                        isConsentGranted = ConsentStatus.Revoked;
                }
            }
            else
                isConsentGranted = ConsentStatus.Revoked;

            return (isConsentGranted, consentId);
        }

    }
}
