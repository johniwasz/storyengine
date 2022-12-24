using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Serialization;

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
