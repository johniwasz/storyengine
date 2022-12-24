using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Tracking;

namespace Whetstone.StoryEngine.Repository.Actions
{
    public class RecordSelectedItemActionProcessor : INodeActionProcessor
    {
        private ILogger<RecordSelectedItemActionProcessor> _dataLogger;
        private ITitleReader _titleReader;


        public RecordSelectedItemActionProcessor(ITitleReader titleReader, ILogger<RecordSelectedItemActionProcessor> logger)
        {
            _dataLogger = logger ?? throw new ArgumentNullException(nameof(logger));
            _titleReader = titleReader ?? throw new ArgumentNullException(nameof(titleReader));
        }


        public async Task<string> ApplyActionAsync(StoryRequest req, List<IStoryCrumb> crumbs, NodeActionData actionData)

        {
            if (req == null)
                throw new ArgumentNullException(nameof(req));


            if (actionData == null)
                throw new ArgumentNullException(nameof(actionData));

            RecordSelectedItemActionData selItemData = (RecordSelectedItemActionData)actionData;

            var slotNames = selItemData.SlotNames;


            if (crumbs == null)
                crumbs = new List<IStoryCrumb>();
            StringBuilder applyActionBuilder = new StringBuilder();

            bool isPrivacyEnabled = await _titleReader.IsPrivacyLoggingEnabledAsync(req.SessionContext.TitleVersion);

            // If no slots names are requested and/or there are no slots in the request, then there's no 
            // point in continuing.
            if ((slotNames?.Any()).GetValueOrDefault(false) && (req.Slots?.Any()).GetValueOrDefault(false))
            {
                applyActionBuilder.Append("RecordSelectedItemAction: ");

                foreach (string slotName in slotNames)
                {

                    KeyValuePair<string, string> foundSlot = req.Slots.FirstOrDefault(x => x.Key.Equals(slotName));
                    string slotValue = foundSlot.Value;

                    if (req.Slots.TryGetValue(slotName, out slotValue))
                    {

                        SelectedItem crumbRecord = crumbs.FirstOrDefault(x =>
                        {
                            if (x is SelectedItem)
                            {
                                SelectedItem item = x as SelectedItem;
                                return item.Name.Equals(slotName, StringComparison.OrdinalIgnoreCase);
                            }

                            return false;
                        }) as SelectedItem;

                        if (crumbRecord == null)
                        {
                            crumbRecord = new SelectedItem();
                            crumbRecord.Name = slotName;
                            crumbRecord.Value = slotValue;
                            crumbs.Add(crumbRecord);

                            if (isPrivacyEnabled)
                                _dataLogger.LogInformation($"Adding selection from slot {crumbRecord.Name}");
                            else
                                _dataLogger.LogInformation($"Adding selection from {crumbRecord}", crumbRecord);

                        }
                        else
                        {
                            var oldCrumbRecord = crumbRecord;

                            string origValue = crumbRecord.Value;
                            crumbRecord.Value = slotValue;

                            if (isPrivacyEnabled)
                                _dataLogger.LogInformation($"Replacing value in slot {crumbRecord.Name}");
                            else
                                _dataLogger.LogInformation($"Updating old slot {oldCrumbRecord} to {crumbRecord}", oldCrumbRecord, crumbRecord);
                        }
                    }

                }
            }


            return applyActionBuilder.ToString();

        }
    }
}
