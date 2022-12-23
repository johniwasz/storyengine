using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Whetstone.StoryEngine.Models
{

    public enum YesNoMaybeEnum
    {
        
        Yes =0,
        No =1,
        Maybe=2
    }

    [JsonObject]
    [DataContract]
    public class CanFulfillResponse
    {

        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "canFulfill")]
        public YesNoMaybeEnum CanFulfill { get; set; }


        [DataMember]
        [JsonProperty(PropertyName = "slotFulfillment")]
        public Dictionary<string, SlotCanFulFill> SlotFulFillment { get; set; }


        /// <summary>
        /// Report how long it took to fulfill the response in milliseconds.
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "processDuration")]
        public long ProcessDuration { get; set; }



        [DataMember]
        [JsonProperty(PropertyName = "responseConversionError")]
        public string ResponseConversionError { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "engineError")]
        public string EngineErrorText { get; set; }
    }


    [DataContract]
    public class SlotCanFulFill
    {
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "canUnderstand")]
        public YesNoMaybeEnum CanUnderstand { get; set; }

        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "canFulfill")]
        public YesNoMaybeEnum CanFulfill { get; set; }

        [DataMember]
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }


    }
}
