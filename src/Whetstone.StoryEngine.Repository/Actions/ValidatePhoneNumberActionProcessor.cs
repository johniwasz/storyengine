using Microsoft.Extensions.Logging;
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
using Whetstone.StoryEngine.Repository.Phone;

namespace Whetstone.StoryEngine.Repository.Actions
{
    public class ValidatePhoneNumberActionProcessor : PhoneActionProcessorBase, INodeActionProcessor
    {
        private readonly IPhoneInfoRetriever _phoneTypeRetriever;

        private readonly ITitleReader _titleReader;

        public ValidatePhoneNumberActionProcessor(IPhoneInfoRetriever phoneTypeRetriever, ITitleReader titleReader)
        {
            _phoneTypeRetriever = phoneTypeRetriever ??
                                  throw new ArgumentNullException(nameof(phoneTypeRetriever));

            _titleReader = titleReader ?? throw new ArgumentNullException(nameof(titleReader));
        }



        public async Task<string> ApplyActionAsync(StoryRequest req, List<IStoryCrumb> crumbs,
            NodeActionData actionData)
        {
            StringBuilder phoneActionBuilder = new StringBuilder();

            if (req == null)
                throw new ArgumentNullException(nameof(req));

            if(req.SessionContext?.TitleVersion == null)
                throw new ArgumentNullException(nameof(req), "SessionContext.TitleVersion cannot be null");

            if (crumbs == null)
            {
                throw new ArgumentNullException(nameof(crumbs));
            }

            if (actionData == null)
                throw new ArgumentNullException(nameof(actionData));

            ValidatePhoneNumberActionData phoneData = actionData as ValidatePhoneNumberActionData;


            bool isPrivacyLogEnabled = await _titleReader.IsPrivacyLoggingEnabledAsync(req.SessionContext.TitleVersion);

            if (phoneData == null)
                throw new ArgumentException(
                    $"{nameof(actionData)} is not a supported type. ValidatePhoneNumberActionData expected");


            if (string.IsNullOrWhiteSpace(phoneData.PhoneNumberSlot))
                throw new ArgumentException("Phone message action could not be executed. Phone number slot name not provided.");


            if (string.IsNullOrWhiteSpace(phoneData.SupportsSmsSlot) &&
                string.IsNullOrWhiteSpace(phoneData.PhoneTypeSlot) &&
                string.IsNullOrWhiteSpace(phoneData.IsValidFormatSlot))
                throw new ArgumentException("Node data must have one or more of the following slots set: IsValidFormatSlot, SupportsSmsSlot or PhoneTypeSlot");
   

            List<SelectedItem> selItems = crumbs.GetSelectedItems();

            SelectedItem phoneItem = selItems.FirstOrDefault(x =>
                x.Name.Equals(phoneData.PhoneNumberSlot, StringComparison.OrdinalIgnoreCase));

            if (phoneItem == null)
                throw new Exception($"Phone slot {phoneData.PhoneNumberSlot} not found.");



            string phoneNumber = phoneItem.Value;

            PhoneTypeEnum curPhoneType = PhoneTypeEnum.Invalid;
            bool supportsSms = false;
            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {

                string phoneLogValue = isPrivacyLogEnabled ? phoneNumber : "(redacted)";

                phoneActionBuilder.AppendLine($"Invoking phone validator with formatted phone number {phoneLogValue}");

            
                var formatResult = PhoneUtility.ValidateFormat(phoneNumber, req.Locale);
                bool isFormatValid = formatResult.IsValid;
                SetSlotValue(crumbs, selItems, phoneData.IsValidFormatSlot, isFormatValid ? "true" : "false",
                    phoneActionBuilder, false);

                if (isFormatValid)
                {
                    if (isPrivacyLogEnabled)
                        phoneActionBuilder.AppendLine($"Phone number {phoneLogValue} passes the formatting check");
                    else
                        phoneActionBuilder.AppendLine($"Phone number {phoneLogValue} passes the formatting check");


                    if (!string.IsNullOrWhiteSpace(phoneData.PhoneTypeSlot) ||
                        !string.IsNullOrWhiteSpace(phoneData.SupportsSmsSlot))
                    {
                        DataPhone phoneInfo =
                            await _phoneTypeRetriever.GetPhoneInfoAsync(formatResult.FormattedNumber);

                        curPhoneType = phoneInfo.Type;
                        supportsSms = phoneInfo.CanGetSmsMessage;

                        phoneActionBuilder.AppendLine(supportsSms
                            ? $"Phone number {phoneLogValue} supports SMS text"
                            : $"Phone number {phoneLogValue} does not support SMS text");

                        if(!isPrivacyLogEnabled)
                            phoneActionBuilder.AppendLine($"Phone number {phoneLogValue} is type {curPhoneType}");

                    }
                    else
                    {
                        curPhoneType = PhoneTypeEnum.Unknown;
                    }

                }
                else
                {
                    curPhoneType = PhoneTypeEnum.InvalidFormat;

                }

            }
            else
            {
                phoneActionBuilder.AppendLine(
                    "Phone number is blank or missing. Setting phone type to Invalid and SupportsSms to false");
            }


            SetSlotValue(crumbs, selItems, phoneData.PhoneTypeSlot, curPhoneType.ToString(), phoneActionBuilder, isPrivacyLogEnabled);


            SetSlotValue(crumbs, selItems, phoneData.SupportsSmsSlot, supportsSms ? "true" : "false",
                phoneActionBuilder, false);

            return phoneActionBuilder.ToString();
        }

        private void SetSlotValue(List<IStoryCrumb> crumbs, List<SelectedItem> selectedItem, string slotName, string slotValue, StringBuilder logText, bool isPrivacyLogEnabled)
        {
            if (!string.IsNullOrWhiteSpace(slotName))
            {
                var supportsSmsSlot = selectedItem.FirstOrDefault(x => x.Name.Equals(slotName));

                string logSlotValue;

                if (isPrivacyLogEnabled)
                    logSlotValue = "(redacted)";
                else
                    logSlotValue = slotValue;

                if (supportsSmsSlot == null)
                {
                    SelectedItem selItem = new SelectedItem();
                    selItem.Name = slotName;
                    selItem.Value = slotValue;
                    crumbs.Add(selItem);
                    logText.AppendLine($"Adding new slot {slotName} with value {logSlotValue}");
                }
                else
                {
                    logText.AppendLine($"Setting slot {slotName} to {logSlotValue}");
                    supportsSmsSlot.Value = slotValue;
                }
            }
        }

    }
}
