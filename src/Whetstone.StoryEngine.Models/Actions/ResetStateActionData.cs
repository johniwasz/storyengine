using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Actions
{

    /// <summary>
    /// The goal of this action to clear all story crumbs.
    /// </summary>
    [DataContract]
    [JsonObject]
    [MessageObject]
    public class ResetStateActionData : NodeActionData
    {

        [YamlIgnore]
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("nodeAction", Order = 0)]
        public override NodeActionEnum NodeAction { get { return NodeActionEnum.ResetState; } set { } }

    }
}
