using System.Collections.Generic;
using System.Runtime.Serialization;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Story
{
    public class StoryPhoneInfo
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


        [DataMember]
        [YamlMember(Alias = "consentName", Order = 2)]
        public string ConsentName { get; set; }



        [DataMember]
        [YamlMember(Alias = "requiredConsents", Order = 3)]
        public List<string> RequiredConsents { get; set; }



    }
}
