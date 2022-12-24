using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Whetstone.StoryEngine.Models.Story;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Actions
{

    [DataContract]
    [JsonObject]
    [MessageObject]
    public class PhoneMessageActionData : NodeActionData
    {
        public PhoneMessageActionData()
        {


        }

        [YamlIgnore]
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "nodeAction")]
        public override NodeActionEnum NodeAction { get { return NodeActionEnum.PhoneMessage; } set { } }


        /// <summary>
        /// Indicate which slot contains the phone number. This is the number the message will be sent to.
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "phoneNumberSlot")]
        public string PhoneNumberSlot { get; set; }

        /// <summary>
        /// Indicates phoneInfo data that overrides values set on the title.  This way we can support multiple source phone numbers
        /// in the 
        /// </summary>
        [DataMember]
        [YamlMember(Alias = "phoneInfo")]
        [JsonProperty(PropertyName = "phoneInfo")]
        public PhoneInfo PhoneInfo { get; set; }

        /// <summary>
        /// Each message is a text message that can have tags and macros that can be interpreted and replaced
        /// by a pre-processor. The message can also specify slot values that are merged in at runtime.
        /// </summary>
        [DataMember]
        [YamlMember(Alias = "phoneMessages")]
        [JsonProperty(PropertyName = "phoneMessages")]
        public List<PhoneMessage> Messages { get; set; }


        /// <summary>
        /// This slot supplies the confirmation name that is stored with the user's phone number confirmation.
        /// </summary>
        [DataMember]
        [YamlMember(Alias = "confirmationNameSlot")]
        [JsonProperty(PropertyName = "confirmationNameSlot")]
        public string ConfirmationNameSlot { get; set; }


    }
}
