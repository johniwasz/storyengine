using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Runtime.Serialization;
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
        [JsonProperty("nodeAction", Order = 0)]
        public sealed override NodeActionEnum NodeAction { get { return NodeActionEnum.NodeVisit; } set { } }

        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("visitAction", Order = 1)]
        [YamlMember(Alias = "visitAction")]
        public UpdateVisitActionEnum? VisitAction { get; set; }

    }
}
