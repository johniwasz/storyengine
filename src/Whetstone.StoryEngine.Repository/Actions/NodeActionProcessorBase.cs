using System;
using System.Collections.Generic;
using System.Linq;
using Whetstone.StoryEngine.Models.Tracking;

namespace Whetstone.StoryEngine.Repository.Actions
{
    public class NodeActionProcessorBase
    {
        protected SelectedItem GetSelectedItem(string itemName, List<IStoryCrumb> crumbs)
        {

            SelectedItem foundItem = null;
            bool isFound = false;

            if ((crumbs?.Any()).GetValueOrDefault(false))
            {

                int crumbIndex = 0;
                while (crumbIndex < crumbs.Count && !isFound)
                {
                    IStoryCrumb curCrumb = crumbs[crumbIndex];
                    if (curCrumb is SelectedItem)
                    {
                        SelectedItem selItem = (SelectedItem)curCrumb;
                        if ((selItem?.Name?.Equals(itemName, StringComparison.OrdinalIgnoreCase)).GetValueOrDefault(false))
                        {
                            foundItem = selItem;
                            isFound = true;

                        }
                    }

                    if (!isFound)
                        crumbIndex++;
                }
            }

            return foundItem;
        }


    }
}
