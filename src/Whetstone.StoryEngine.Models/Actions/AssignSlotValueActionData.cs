using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Whetstone.StoryEngine.Models.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using MessagePack;
using System.Threading.Tasks;
using Whetstone.StoryEngine;
using YamlDotNet.Serialization;
using Whetstone.StoryEngine.Models;

namespace Whetstone.StoryEngine.Models.Actions
{

    [DataContract]
    [JsonObject]
    [MessageObject]
    public class AssignSlotValueActionData : NodeActionData
    {
        [YamlIgnore]
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "nodeAction")]
        public override NodeActionEnum NodeAction { get { return NodeActionEnum.AssignValue; } set { } }

        public AssignSlotValueActionData()
        {


        }

        [JsonRequired]
        [DataMember]
        [JsonProperty(PropertyName = "slotName")]
        public string SlotName { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }
}
