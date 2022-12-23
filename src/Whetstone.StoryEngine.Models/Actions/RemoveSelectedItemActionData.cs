using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Whetstone.StoryEngine.Models.Tracking;
using Whetstone.StoryEngine;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Actions
{


    /// <summary>
    /// Removes a user's selected slot item(s).
    /// </summary>
    /// <remarks>
    /// This is useful if a user has selected two slot items (city, healthcondition) and one of the items needs to be removed to execute a search.
    /// </remarks>
    [DataContract]
    [JsonObject]
    [MessageObject]
    public class RemoveSelectedItemActionData : NodeActionData
    {


        public RemoveSelectedItemActionData()
        {

            NodeAction = NodeActionEnum.RemoveSelectedItem;

        }


        [YamlMember(Alias = "slotNames")]
        [JsonProperty(PropertyName ="slotNames", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> SlotNames { get; set; }

        [YamlIgnore]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("nodeAction", Order = 0)]
        public override NodeActionEnum NodeAction { get; set; }

     
    }
}
