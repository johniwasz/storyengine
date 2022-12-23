using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Models.Messaging
{




    /// <summary>
    /// Has information required to record that the request originated from a title. 
    /// </summary>
    [JsonObject("NotificationSourcePhoneMessageAction")]
    public class NotificationSourcePhoneMessageAction : INotificationSource
    {

        public NotificationSourcePhoneMessageAction()
        {


        }


        [JsonProperty(PropertyName = "notificationSourceType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public NotificationSourceTypeEnum NotificationSourceType { get => NotificationSourceTypeEnum.PhoneMessageAction; set { /* do nothing */ } }


        [JsonProperty(PropertyName = "consent")]
        public UserPhoneConsent Consent { get; set; }

    }

}
