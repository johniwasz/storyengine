using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Messaging.Sms
{
    public class InboundSmsMessage
    {

        [JsonRequired]
        [JsonProperty(PropertyName = "body")]
        public string Body { get; set; }

        [JsonProperty(PropertyName = "alias")]
        public string Alias { get; set; }

        [JsonProperty(PropertyName = "path")]
        public string Path { get; set; }

        [JsonProperty(PropertyName = "headers")]
        public Dictionary<string, string> Headers { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "smsType", NullValueHandling = NullValueHandling.Ignore)]
        public SmsSenderType? SmsType { get; set; }

    }

}
