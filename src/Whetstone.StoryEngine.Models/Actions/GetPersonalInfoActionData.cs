using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Actions
{

    public enum PersonalDataType
    {
        PhoneNumber = 0,
        ShortName = 1,
        GivenName = 2,
        EmailAddress = 3

    }


    [DataContract]
    [JsonObject]
    [MessageObject]
    public class GetPersonalInfoActionData : NodeActionData
    {


        public GetPersonalInfoActionData()
        {


        }

        [YamlIgnore]
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("nodeAction", Order = 0)]
        public override NodeActionEnum NodeAction { get { return NodeActionEnum.AssignValue; } set { } }


        [JsonRequired]
        [DataMember]
        [JsonProperty(PropertyName = "slotName")]
        public string SlotName { get; set; }


        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "personalDataType")]
        public PersonalDataType PersonalDataType { get; set; }






    }


}
