using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Newtonsoft.Json;
using System;

namespace Whetstone.StoryEngine.Models.Admin
{
    public class MessageConsentReportRecord
    {
        /// <summary>
        /// True if the message was sent. False if the message was not sent or there was an error while sending the message.
        /// </summary>
        [Index(0)]
        [Name("successstatus")]
        [JsonProperty(PropertyName = "status", Required = Required.Always)]
        public bool Status { get; set; }


        /// <summary>
        /// A unique user id Whetstone Technologies uses to track Alexa Skill and Google Action users.
        /// </summary>
        /// <remarks>The user id can also apply to other platforms, like Bixby.</remarks>
        [Index(1)]
        [Name("userid")]
        [JsonProperty(PropertyName = "userId")]
        public Guid? UserId { get; set; }


        /// <summary>
        /// Phone number provided by the user. This is the phone number the message was sent to.
        /// </summary>
        [Index(2)]
        [Name("phonenumber")]
        [JsonProperty(PropertyName = "phoneNumber")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// UTC date time the message was sent.
        /// </summary>
        [Index(3)]
        [Name("sendtime")]
        [JsonProperty(PropertyName = "sendTime")]
        public DateTime? SendTime { get; set; }


        /// <summary>
        /// This is the value the message provider assigned to the message upon receipt.
        /// </summary>
        /// <remarks>Use this value to track down issues with the message provider.</remarks>
        [Index(4)]
        [Name("providermessageid")]
        [JsonProperty(PropertyName = "providerMessageId")]
        public string ProviderMessageId { get; set; }

        /// <summary>
        /// Text contents of the message.
        /// </summary>
        [Index(5)]
        [Name("code")]
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// An identifier user by Whetstone Technologies to uniquely identify Alexa Skill and Google Action sessions.
        /// </summary>
        [Index(6)]
        [Name("sessionid")]
        [JsonProperty(PropertyName = "sessionId")]
        public Guid? SessionId { get; set; }

        /// <summary>
        /// Date and time the user granted consent to receive an SMS text message.
        /// </summary>
        [Index(7)]
        [Name("smsconsentdate")]
        [JsonProperty(PropertyName = "smsConsentDate")]
        public DateTime? SmsConsentDate { get; set; }
    }


    public class MessageConsentReportRecordMap : ClassMap<MessageConsentReportRecord>
    {
        public MessageConsentReportRecordMap()
        {
            Map(m => m.Status ? "true" : "false").Name("successstatus").Index(0);
            Map(m => m.UserId).Name("userid").Index(1);
            Map(m => m.PhoneNumber).Name("phonenumber").Index(2); //.ConvertUsing(row => JsonConvert.DeserializeObject<Json>(row.GetField("Json")));
            Map(m => m.SendTime.HasValue ? m.SendTime.Value.ToString("yyyy-MM-ddThh:mm:ss.fffffff") : null).Name("sendtime").Index(3);
            Map(m => m.ProviderMessageId).Name("providermessageid").Index(4);
            Map(m => m.Message).Name("code").Index(5);
            Map(m => m.SessionId).Name("sessionid").Index(6);
            Map(m => m.SmsConsentDate.HasValue ? m.SmsConsentDate.Value.ToString("yyyy-MM-ddThh:mm:ss.fffffff") : null).Name("smsconsentdate").Index(7);



        }
    }
}
