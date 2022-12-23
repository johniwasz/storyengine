using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Actions
{


    /// <summary>
    /// This class is used to capture and store user consent to receive a text (SMS) message. Consent can be greanted and revoked.
    /// </summary>
    /// <remarks></remarks>
    [DataContract]
    [JsonObject]
    [MessageObject]
    public class SmsConfirmationActionData : NodeActionData
    {



        [YamlIgnore]
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "nodeAction")]
        public override NodeActionEnum NodeAction { get { return NodeActionEnum.SmsConfirmation; } set { } }


        /// <summary>
        /// This slot supplies the confirmation name that is stored with the user's phone number confirmation. It is not reqired. If not provided, the confirmation name is pulled from the title's PhoneInfo.
        /// </summary>
        [DataMember]
        [YamlMember(Alias = "confirmationNameSlot")]
        [JsonProperty(PropertyName = "confirmationNameSlot")]
        public string ConfirmationNameSlot { get; set; }



        [DataMember]
        [YamlMember(Alias = "grantConfirmation")]
        [JsonProperty(PropertyName = "grantConfirmation")]
        public bool GrantConfirmation { get; set; }



        [DataMember]
        [YamlMember(Alias = "phoneNumberSlot")]
        [JsonProperty(PropertyName = "phoneNumberSlot")]
        public string PhoneNumberSlot { get; set; }



    }
}
