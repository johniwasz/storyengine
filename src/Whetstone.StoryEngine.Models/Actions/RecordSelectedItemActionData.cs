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

    [DataContract]
    [JsonObject]
    [MessageObject]
    public class RecordSelectedItemActionData : NodeActionData
    {



        public RecordSelectedItemActionData()
        {
            NodeAction = NodeActionEnum.SelectedItem;

        }

        [YamlIgnore]
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("nodeAction", Order = 0)]
        public override NodeActionEnum NodeAction { get; set; }
        
        /// <summary>
        /// Indicate which slot names to record.
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "slotNames", NullValueHandling = NullValueHandling.Ignore, Order = 1)]
        [YamlMember(Alias ="slots")]
        public List<string> SlotNames { get; set; }

    }
}
