using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Tracking;

namespace Whetstone.StoryEngine.Repository.Actions
{
    public class RemoveSelectedItemActionProcessor : INodeActionProcessor
    {


        private readonly ILogger<RemoveSelectedItemActionProcessor> _dataLogger;


        public RemoveSelectedItemActionProcessor(ILogger<RemoveSelectedItemActionProcessor> logger)
        {
            _dataLogger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");

        }


#pragma warning disable CS1998
        public async Task<string> ApplyActionAsync(StoryRequest req, List<IStoryCrumb> crumbs, NodeActionData actionData)
#pragma warning restore CS1998
        {
            StringBuilder actionBuilder = new StringBuilder();

            if (crumbs == null)
                crumbs = new List<IStoryCrumb>();


            RemoveSelectedItemActionData removeData = (RemoveSelectedItemActionData)actionData;


            // If no slots names are requested and/or there are no slots in the request, then there's no 
            // point in continuing.
            if ((removeData.SlotNames?.Any()).GetValueOrDefault(false) && (req.Slots?.Any()).GetValueOrDefault(false))
            {

                actionBuilder.Append("RemoveSelectedItemAction: ");

                foreach (string slotName in removeData.SlotNames)
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

                        if (crumbRecord != null)
                        {
                            crumbs.Remove(crumbRecord);
                            string message = $"Removed selected item {slotName}";
                            actionBuilder.AppendLine(message);
                            _dataLogger.LogInformation(message);
                        }
                        else
                        {
                            string message = $"Selected item {slotName} to remove not found";
                            actionBuilder.AppendLine(message);
                            _dataLogger.LogInformation(message);
                        }

                    }

                }
            }

            return actionBuilder.ToString();
        }
    }
}
