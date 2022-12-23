using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Whetstone.StoryEngine.Security
{


    public enum UserConfimationCodeStatus
    {
        /// <summary>
        /// Returned if the user name is valid and the confirmation code is accepted
        /// </summary>
        [EnumMember(Value = "confirmed")]
        Confirmed,

        /// <summary>
        /// Returned if the user name is valid, but the confirmation code is expired
        /// </summary>
        [EnumMember(Value = "expired")]
        Expired,

        /// <summary>
        /// Returned if the either the user name or the confirmation code is invalid
        /// </summary>
        [EnumMember(Value = "invalid")]
        Invalid
    }



    public class ConfirmNewUserResponse
    {

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "status")]
        public UserConfimationCodeStatus Status { get; set; }
    }
}
