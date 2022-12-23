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
    public class InventoryActionProcessor : INodeActionProcessor
    {

        private readonly ILogger<InventoryActionProcessor> _dataLogger;



        public InventoryActionProcessor(ILogger<InventoryActionProcessor> logger)
        {
            _dataLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

#pragma warning disable CS1998
        public async Task<string> ApplyActionAsync(StoryRequest req, List<IStoryCrumb> crumbs, NodeActionData actionData)
#pragma warning restore CS1998
        {

            if (req == null)
                throw new ArgumentNullException(nameof(req));


            if (actionData == null)
                throw new ArgumentNullException(nameof(actionData));

            InventoryActionData invData = (InventoryActionData)actionData;


            string appliedAction = null;

            InventoryActionType invAction = invData.ActionType;

            switch (invAction)
            {
                case InventoryActionType.Add:
                    appliedAction = AddItem(invData.Item, crumbs);
                    break;
                case InventoryActionType.Remove:
                    appliedAction = RemoveItem(invData.Item, crumbs);
                    break;
                case InventoryActionType.Clear:
                    appliedAction = ClearItem(invData.Item, crumbs);
                    break;
                    ;
                default:
                    // assume add.
                    appliedAction = AddItem(invData.Item, crumbs);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(appliedAction))
                appliedAction = string.Concat("InventoryAction: ", appliedAction);

            return appliedAction;
        }


        private string AddItem(InventoryItemBase item, List<IStoryCrumb> crumbs)
        {
            StringBuilder addItemBuilder = new StringBuilder();


            if (crumbs == null)
                crumbs = new List<IStoryCrumb>();


            if (item is UniqueItem)
            {
                // check if the crumbs already contain the item and if not, then add it.
                if (!crumbs.Contains(item))
                {
                    crumbs.Add(item);
                    {
                        string message = $"Added unique item {item.Name} to inventory";
                        addItemBuilder.AppendLine(message);
                        _dataLogger.LogInformation(message);
                    }
                }
                else
                {
                    string message = $"Attempted to add unique item {item.Name} to inventory. User already has item.";
                    addItemBuilder.AppendLine(message);
                    _dataLogger.LogInformation(message);
                }
            }
            else if (item is MultiItem)
            {
                MultiItem multiItem = crumbs.FirstOrDefault(x =>
                {
                    if (x is MultiItem)
                    {
                        MultiItem multi = x as MultiItem;

                        return multi.Name.Equals(item.Name);
                    }

                    return false;

                }) as MultiItem;

                if (multiItem != null)
                {

                    multiItem.Count = multiItem.Count + 1;
                    int curCount = multiItem.Count;
                    string message = $"Incremented count of inventory item {item.Name} to {curCount}";
                    addItemBuilder.AppendLine(message);
                    _dataLogger.LogInformation(message);
                }
                else
                {
                    crumbs.Add(item);
                    string message = $"Added single {item.Name} to inventory";
                    addItemBuilder.AppendLine(message);
                    _dataLogger.LogInformation(message);
                }
            }

            return addItemBuilder.ToString();
        }

        private string RemoveItem(InventoryItemBase item, List<IStoryCrumb> crumbs)
        {
            string itemName = item.Name;

            crumbs.RemoveAll(x =>
            {
                bool isFound = false;
                if (x is InventoryItemBase)
                {
                    var itemBase = x as InventoryItemBase;
                    if (itemBase.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase))
                        isFound = true;
                }

                return isFound;
            });

            return null;
        }


        public string ClearItem(InventoryItemBase item, List<IStoryCrumb> crumbs)
        {

            StringBuilder clearItemBuilder = new StringBuilder();


            if (crumbs == null)
                crumbs = new List<IStoryCrumb>();


            if (item is UniqueItem)
            {
                // check if the crumbs already contain the item and if not, then add it.
                if (crumbs.Contains(item))
                {
                    crumbs.Remove(item);

                    string message = $"Cleared unique item {item.Name} from inventory";
                    clearItemBuilder.AppendLine(message);
                    _dataLogger.LogInformation(message);
                }
                else
                {

                    string message = $"Attempted to clear unique item {item.Name} from inventory. User does not have item.";
                    clearItemBuilder.AppendLine(message);
                    _dataLogger.LogInformation(message);
                }
            }
            else if (item is MultiItem)
            {
                MultiItem multiItem = crumbs.FirstOrDefault(x =>
                {
                    if (x is MultiItem)
                    {
                        MultiItem multi = x as MultiItem;

                        return multi.Name.Equals(item.Name);
                    }

                    return false;

                }) as MultiItem;

                if (multiItem != null)
                {

                    int priorCount = multiItem.Count;
                    multiItem.Count = 0;

                    string message = $"Set count of item {item.Name} in inventory to {multiItem.Count}. User had {priorCount} item(s).";
                    clearItemBuilder.AppendLine(message);
                    _dataLogger.LogInformation(message);
                }
                else
                {
                    MultiItem multi = new MultiItem();

                    multi.Count = 0;
                    multi.Name = item.Name;
                    crumbs.Add(multi);

                    string message = $"Setting count of item {item.Name} in user's inventory to 0. Item was not in inventory.";
                    clearItemBuilder.AppendLine(message);
                    _dataLogger.LogInformation(message);
                }
            }

            return clearItemBuilder.ToString();
        }


    }
}
