using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Tracking
{

    /// <summary>
    /// This denotes something that the user does which is tracked such as an inventory item or a visit to a node.
    /// </summary>
    [JsonConverter(typeof(JsonStoryCrumbConverter))]
    public interface IStoryCrumb
    {
       

    }
}
