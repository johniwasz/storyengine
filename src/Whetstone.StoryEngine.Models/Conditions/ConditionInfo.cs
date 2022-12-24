using System.Collections.Generic;
using Whetstone.StoryEngine.Models.Tracking;

namespace Whetstone.StoryEngine.Models.Conditions
{

    /// <summary>
    /// Used to evaluate story conditions.
    /// </summary>
    /// <remarks>
    /// Story conditions expect this data to be passed in order to evaluate whether a condition is true or not.
    /// </remarks>
    public class ConditionInfo
    {

        public ConditionInfo(List<IStoryCrumb> crumbs, List<IStoryCrumb> permCrumbs, Client userClient)
        {
            this.Crumbs = crumbs;
            this.UserClient = userClient;
            this.PermanentCrumbs = permCrumbs;

        }

        public ConditionInfo(List<IStoryCrumb> crumbs, Client userClient)
        {
            this.Crumbs = crumbs;
            this.UserClient = userClient;
            this.PermanentCrumbs = null;

        }

        public ConditionInfo()
        {
        }

        public Client UserClient { get; set; }


        public List<IStoryCrumb> Crumbs { get; set; }


        public List<IStoryCrumb> PermanentCrumbs { get; set; }
    }
}
