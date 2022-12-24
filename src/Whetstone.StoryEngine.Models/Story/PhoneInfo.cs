using System.Runtime.Serialization;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Story
{


    [DataContract]
    public class PhoneInfo
    {


        /// <summary>
        /// This is the phone number use to send any messages.
        /// </summary>
        /// <remarks>
        /// This can be a short code or a long code.
        /// </remarks>
        [DataMember]
        [YamlMember(Alias = "sourcePhone", Order = 0)]
        public string SourcePhone { get; set; }


        /// <summary>
        /// This is the Amazon Pinpoint application Id. 
        /// </summary>
        [DataMember]
        [YamlMember(Alias = "smsService", Order = 1)]
        public SmsSenderType SmsService { get; set; }

    }
}
