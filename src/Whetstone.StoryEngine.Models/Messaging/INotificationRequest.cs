using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Converters;
using Whetstone.StoryEngine.Models.Data;

namespace Whetstone.StoryEngine.Models.Messaging
{
    public enum NotificationTypeEnum
    {

        /// <summary>
        /// SendStatus a text message
        /// </summary>
        Sms = 1,
        /// <summary>
        /// SendStatus a Google or Alexa notification
        /// </summary>
        SpeechClientNotification =2,
    }


    public enum NotificationSourceTypeEnum
    {

        /// <summary>
        /// User consent story agreement.
        /// </summary>
        PhoneMessageAction = 1,
        /// <summary>
        /// SendStatus a Google or Alexa notification
        /// </summary>
        Scheduled = 2,
        /// <summary>
        /// This message is in response to an inbound SMS message
        /// </summary>
        InboundSmsResponse = 3
    }


    /// <summary>
    /// Contains the message to send to the destination.
    /// </summary>
    [JsonConverter(typeof(JsonNotificationRequestConverter))]

    public interface INotificationRequest
    {

        Guid? Id { get; set; }

      
        NotificationTypeEnum NotificationType { get; set; }


        INotificationSource Source { get; set; }

    }

    [JsonConverter(typeof(JsonNotificationRecordConverter))]
    public interface IOutboundNotificationRecord
    {

        NotificationTypeEnum NotificationType { get; set; }
    }

    [JsonConverter(typeof(JsonNotificationSourceConverter))]
    public interface INotificationSource
    {

        NotificationSourceTypeEnum NotificationSourceType { get; set; }


    }


    [JsonObject("InboundSmsNotification")]
    public class InboundSmsNotification : INotificationSource
    {

        public InboundSmsNotification()
        {


        }


        [JsonProperty(PropertyName = "notificationSourceType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public NotificationSourceTypeEnum NotificationSourceType { get => NotificationSourceTypeEnum.InboundSmsResponse; set { /* do nothing */ } }



        [JsonProperty(PropertyName = "engineUserId")]
        public Guid? EngineUserId { get; set; }


        [JsonProperty(PropertyName = "consentId")]
        public Guid? ConsentId { get; set; }


    }
}
