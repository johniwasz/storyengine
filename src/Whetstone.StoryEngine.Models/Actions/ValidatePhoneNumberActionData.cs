using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Actions
{

    [DataContract]
    [JsonObject]
    [MessageObject]
    public class ValidatePhoneNumberActionData : NodeActionData
    {

        public ValidatePhoneNumberActionData()
        {


        }

        [YamlIgnore]
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "nodeAction")]
        public override NodeActionEnum NodeAction { get { return NodeActionEnum.ValidatePhoneNumber; } set { } }


        /// <summary>
        /// Indicate which slot contains the phone number. This is the number the message will be sent to.
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "phoneNumberSlot")]
        public string PhoneNumberSlot { get; set; }

        /// <summary>
        /// The phone type. For example (mobile, landline, etc)
        /// </summary>
        [DataMember]
        [YamlMember(Alias = "phoneTypeSlot")]
        [JsonProperty(PropertyName = "phoneTypeSlot")]
        public string PhoneTypeSlot { get; set; }

        /// <summary>
        /// Returns true if the phone number can receive SMS text messages. False if it cannot.
        /// </summary>
        [DataMember]
        [YamlMember(Alias = "supportsSmsSlot")]
        [JsonProperty(PropertyName = "supportsSmsSlot")]
        public string SupportsSmsSlot { get; set; }


        /// <summary>
        /// Returns true if the phone number is not a valid format. False if phone number format is invalid. This will happen if the number has too many or too few digits.
        /// </summary>
        [DataMember]
        [YamlMember(Alias = "isValidFormatSlot")]
        [JsonProperty(PropertyName = "isValidFormatSlot")]
        public string IsValidFormatSlot { get; set; }

    }
}
