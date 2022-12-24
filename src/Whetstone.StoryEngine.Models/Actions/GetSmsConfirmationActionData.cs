using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Actions
{
    public class GetSmsConfirmationActionData : NodeActionData
    {


        [YamlIgnore]
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "nodeAction")]
        public override NodeActionEnum NodeAction { get { return NodeActionEnum.SmsConfirmation; } set { } }


    }
}
