using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Tracking;
using Whetstone.StoryEngine.Repository.Messaging;
using Whetstone.StoryEngine.Repository.Phone;


namespace Whetstone.StoryEngine.Repository.Actions
{

    /// <summary>
    /// Stores the phone confirmation
    /// </summary>
    public class SmsConfirmationActionProcessor : INodeActionProcessor
    {
        private ISmsConsentRepository _consentRepo;
        private ITitleReader _titleReader;
        private IPhoneInfoRetriever _phoneRetriever;

        public SmsConfirmationActionProcessor(ISmsConsentRepository consentRep, ITitleReader titleReader, IPhoneInfoRetriever phoneInfoRetriever)
        {
            _consentRepo = consentRep ?? throw new ArgumentNullException(nameof(consentRep));
            _titleReader = titleReader ?? throw new ArgumentNullException(nameof(titleReader));
            _phoneRetriever = phoneInfoRetriever ?? throw new ArgumentNullException(nameof(phoneInfoRetriever));
        }


        public async Task<string> ApplyActionAsync(StoryRequest req, List<IStoryCrumb> crumbs,
            NodeActionData actionData)
        {
            StringBuilder sb = new StringBuilder();

            SmsConfirmationActionData confirmationData = null;

            if (actionData is SmsConfirmationActionData)
                confirmationData = actionData as SmsConfirmationActionData;

            if (confirmationData == null)
                throw new ArgumentNullException(
                    $"{nameof(actionData)}", "Cannot be null and must be of type SmsConfifmrationActionData");

            if(!(req.SessionContext.TitleVersion?.VersionId.HasValue).GetValueOrDefault(false))
                throw new ArgumentException(
                    $"Session request with client request id {req.RequestId} and engine request id {req.EngineRequestId} is missing the titleversionid value from the session context");


            //  List<SelectedItem> selItems = crumbs.GetSelectedItems();

            string consentName = await GetConsentNameAsync(req, confirmationData.ConfirmationNameSlot, crumbs);

            string phoneNumber = GetUserPhoneNumber(req, confirmationData.PhoneNumberSlot, crumbs);


            var phoneInfo = await _phoneRetriever.GetPhoneInfoAsync(phoneNumber);
            if (!(phoneInfo?.Id.HasValue).GetValueOrDefault(false))
                throw new Exception(
                    $"Session request with with client request id {req.RequestId} and engine request id {req.EngineRequestId} cannot resolve to a phone id");

            UserPhoneConsent userPhoneCon = new UserPhoneConsent();
            userPhoneCon.Id = Guid.NewGuid();
            userPhoneCon.SmsConsentDate = req.RequestTime;
            userPhoneCon.TitleClientUserId = req.SessionContext.EngineUserId.Value;

            userPhoneCon.IsSmsConsentGranted = confirmationData.GrantConfirmation;
            userPhoneCon.TitleClientUserId = req.SessionContext.EngineUserId.Value;
            userPhoneCon.TitleVersionId = req.SessionContext.TitleVersion.VersionId.Value;
            userPhoneCon.EngineRequestId = req.EngineRequestId;
            userPhoneCon.PhoneId = phoneInfo.Id.Value;
            userPhoneCon.Name = consentName;

            await _consentRepo.SaveConsentAsync(userPhoneCon);

            sb.AppendLine(
                $"Recorded consent grant {consentName} as {confirmationData.GrantConfirmation} for phone id {phoneInfo.Id.Value}");

            return sb.ToString();
        }

        private string GetUserPhoneNumber(StoryRequest req, string phoneNumberSlot, List<IStoryCrumb> crumbs)
        {
            string retPhoneNumber;
            if(string.IsNullOrWhiteSpace(phoneNumberSlot))
            {
                // If the phone number is not set, then check if the client is an SMS type client. 
                if (req.Client == Client.Sms)
                {
                    // If the Client is of type SMS, then the user id is the phone number of the client making the call
                    retPhoneNumber = req.UserId;
                }
                else
                    throw new Exception($"phoneNumberSlot is not set. User client type is {req.Client} and requires the phone number slot to be set");

            }
            else
            {
                string phoneSlotValue = GetCrumbSelectedItemValue(phoneNumberSlot, crumbs);
                if (string.IsNullOrWhiteSpace(phoneSlotValue))
                    throw new Exception($"Phone slot {phoneNumberSlot} not found");

                retPhoneNumber = phoneSlotValue;
            }


            return retPhoneNumber;

        }

        private async Task<string> GetConsentNameAsync(StoryRequest req, string confirmationNameSlot, List<IStoryCrumb> crumbs)
        {
            string retConsentName;


            if(string.IsNullOrWhiteSpace(confirmationNameSlot))
            {
            

                string titleConsent;

                // if the confirmation name slot name is not specified, then pull it from the title.
                StoryPhoneInfo storyPhone = await _titleReader.GetPhoneInfoAsync( req.SessionContext.TitleVersion);

                titleConsent = storyPhone?.ConsentName;

                if (string.IsNullOrWhiteSpace(titleConsent))
                    throw new Exception("Confirmation name not found in title");


                retConsentName = titleConsent;
            }
            else
            {

                if (string.IsNullOrWhiteSpace(confirmationNameSlot))
                    throw new Exception($"Confirmation name slot must be set if the confirmation name is not found in phone title settings");

                string consentDataName = GetCrumbSelectedItemValue(confirmationNameSlot, crumbs);

                if (string.IsNullOrWhiteSpace(consentDataName))
                    throw new Exception($"Confirmation name {confirmationNameSlot} not found in crumbs");

                retConsentName = consentDataName;
            }


            return retConsentName;
        }

        private string GetCrumbSelectedItemValue(string slotName, List<IStoryCrumb> crumbs)
        {
            string slotValue = null;
            if (crumbs != null)
            {
                List<SelectedItem> selItems = crumbs.GetSelectedItems();

                SelectedItem foundConfirmSlot = selItems.FirstOrDefault(x => x.Name.Equals(slotName, StringComparison.OrdinalIgnoreCase));

                if (foundConfirmSlot != null)
                    slotValue = foundConfirmSlot.Value;
            }

            return slotValue;
        }
    }
}
