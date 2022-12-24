using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Tracking;

namespace Whetstone.StoryEngine.Repository.Actions
{
    public class AssignSlotValueActionProcessor : NodeActionProcessorBase, INodeActionProcessor
    {


        private readonly ILogger _dataLogger;

        private readonly ITitleReader _titleReader;


        public AssignSlotValueActionProcessor(ILogger<AssignSlotValueActionProcessor> logger, ITitleReader titleReader)
        {
            _dataLogger = logger ?? throw new ArgumentNullException(nameof(logger));
            _titleReader = titleReader ?? throw new ArgumentNullException(nameof(titleReader));



        }




        public async Task<string> ApplyActionAsync(StoryRequest req, List<IStoryCrumb> crumbs, NodeActionData actionData)
        {
            if (req == null)
                throw new ArgumentNullException(nameof(req));

            if (req.SessionContext?.TitleVersion == null)
                throw new ArgumentNullException(nameof(req), "SessionContext.TitleVersion cannot be null");

            if (actionData == null)
                throw new ArgumentNullException(nameof(actionData));


            bool isPrivacyLoggingEnabled = await _titleReader.IsPrivacyLoggingEnabledAsync(req.SessionContext.TitleVersion);


            AssignSlotValueActionData assignSlotData = (AssignSlotValueActionData)actionData;


            string actionResultText;

            if (string.IsNullOrWhiteSpace(assignSlotData.SlotName))
                throw new ArgumentNullException(nameof(assignSlotData), "SlotName cannot be null or empty");

            SelectedItem foundItem = GetSelectedItem(assignSlotData.SlotName, crumbs);

            string valueText = assignSlotData.Value;

            valueText = ReplaceSlotValues(valueText, crumbs);


            if (foundItem != null)
            {

                actionResultText = $"Replacing selected item {assignSlotData.SlotName}";

                string priorVal = foundItem.Value;

                foundItem.Value = valueText;

                if (isPrivacyLoggingEnabled)
                {
                    _dataLogger.LogInformation(actionResultText);
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(priorVal))
                    {
                        _dataLogger.LogInformation("Assigned selected name {@foundItem}. Prior value was {priorVal}", foundItem, priorVal);
                    }
                    else
                    {
                        _dataLogger.LogInformation("Assigned selected name {@foundItem}", foundItem);
                    }
                }

            }
            else
            {
                if (crumbs == null)
                    crumbs = new List<IStoryCrumb>();


                SelectedItem selItem = new SelectedItem();
                selItem.Name = assignSlotData.SlotName;
                selItem.Value = valueText;
                crumbs.Add(selItem);

                actionResultText = $"Adding selected item {assignSlotData.SlotName}";

                if (isPrivacyLoggingEnabled)
                {
                    _dataLogger.LogInformation(actionResultText);
                }
                else
                    _dataLogger.LogInformation("Added selected item {@selItem}", selItem);
            }

            return actionResultText;

        }

        private string ReplaceSlotValues(string valueText, List<IStoryCrumb> crumbs)
        {
            MatchCollection matches = Regex.Matches(valueText, @"{([^{}]*)}");
            string returnText = valueText;

            List<SelectedItem> selItems = new List<SelectedItem>();

            foreach (IStoryCrumb crumb in crumbs)
            {
                if (crumb is SelectedItem)
                    selItems.Add((SelectedItem)crumb);
            }

            foreach (Match match in matches)
            {
                string originalMatch = match.Value;
                string formattedMatch = originalMatch.Substring(1, originalMatch.Length - 2);
                formattedMatch = formattedMatch.Trim();

                SelectedItem foundItem = selItems.FirstOrDefault(x => x.Name.Equals(formattedMatch, StringComparison.OrdinalIgnoreCase));
                if (foundItem != null)
                    returnText = returnText.Replace(originalMatch, foundItem.Value);
            }


            return returnText;

        }
    }
}
