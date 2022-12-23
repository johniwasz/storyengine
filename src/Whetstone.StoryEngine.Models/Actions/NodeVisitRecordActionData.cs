using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Whetstone.StoryEngine.Models.Tracking;
using Whetstone.StoryEngine;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Actions
{

    public enum UpdateVisitActionEnum
    {
        Add,
        Remove

    }

    /// <summary>
    /// Indicates the user has visited a node.
    /// </summary>
    [DataContract]
    [JsonObject]
    [MessageObject]
    public class NodeVisitRecordActionData : NodeActionData
    {

        public NodeVisitRecordActionData()
        {

            NodeAction = NodeActionEnum.NodeVisit;
        }

        [YamlIgnore]
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("nodeAction", Order =0)]
        public sealed override NodeActionEnum NodeAction { get { return NodeActionEnum.NodeVisit; } set { } }

        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("visitAction", Order = 1)]
        [YamlMember(Alias = "visitAction")]
        public UpdateVisitActionEnum? VisitAction { get; set; }

    }
}
