using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;
using Whetstone.StoryEngine.Models.Story.Text;

namespace Whetstone.StoryEngine.Models.Messaging.Sms
{
    public class SmsNotificationRequest : INotificationRequest
    {


        public SmsNotificationRequest ()
        {


        }


        [JsonRequired]
        [JsonProperty(PropertyName = "id")]
        public Guid? Id { get; set; }



        [JsonProperty(PropertyName = "notificationType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public NotificationTypeEnum NotificationType
        {
            get { return NotificationTypeEnum.Sms; }
            set { /* do nothing */ }
        }

        [JsonRequired]
        [JsonProperty(PropertyName = "textMessages")]
        public List<TextFragmentBase> TextMessages { get; set; }


        /// <summary>
        /// Drives sender-specific behavior.
        /// </summary>
        [JsonProperty(PropertyName = "tags")]
        public Dictionary<string, string> Tags { get; set; }


        [JsonProperty(PropertyName = "sourceNumber")]
        public string SourceNumber { get; set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "destinationNumberId")]
        public Guid? DestinationNumberId { get; set; }


        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "smsProvider")]
        public SmsSenderType SmsProvider { get; set; }


        [JsonProperty(PropertyName = "source")]
        public INotificationSource Source { get; set; }
    }
}
