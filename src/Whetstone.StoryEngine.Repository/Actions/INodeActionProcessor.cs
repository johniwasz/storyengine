using System.Collections.Generic;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Tracking;

namespace Whetstone.StoryEngine.Repository.Actions
{
    public interface INodeActionProcessor
    {
        //long? Id { get; set; }
        //bool? IsPermanent { get; set; }
        //NodeActionType NodeAction { get; set; }
        //string ParentNodeName { get; set; }

        Task<string> ApplyActionAsync(StoryRequest req, List<IStoryCrumb> crumbs, NodeActionData actionData);
    }
}