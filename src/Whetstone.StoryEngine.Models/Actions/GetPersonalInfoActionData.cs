using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Whetstone.StoryEngine.Models.Tracking;
using Whetstone.StoryEngine;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Actions
{

    public enum PersonalDataType
    {
        PhoneNumber = 0,
        ShortName =1 ,
        GivenName =2 ,
        EmailAddress =3 

    }


    [DataContract]
    [JsonObject]
    [MessageObject]
    public class GetPersonalInfoActionData : NodeActionData
    {


        public GetPersonalInfoActionData()
        {


        }

        [YamlIgnore]
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("nodeAction", Order = 0)]
        public override NodeActionEnum NodeAction { get { return NodeActionEnum.AssignValue; } set { }  } 


        [JsonRequired]
        [DataMember]
        [JsonProperty(PropertyName = "slotName")]
        public string SlotName { get; set; }


        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "personalDataType")]
        public PersonalDataType PersonalDataType { get; set; }






    }


}
