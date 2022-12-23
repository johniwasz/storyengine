using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Tracking;

namespace Whetstone.StoryEngine.Repository.Actions
{
    public class ResetStateActionProcessor : INodeActionProcessor
    {


#pragma warning disable CS1998
        public async Task<string> ApplyActionAsync(StoryRequest req, List<IStoryCrumb> crumbs, NodeActionData actionData)
#pragma warning restore CS1998
        {


            crumbs.Clear();

            return "ResetStateAction: State is reset";
        }
    }
}
