using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Serialization;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models
{

 //   [JsonConverter(typeof(DollarIdPreservingConverter<SlotType>))]
    [DebuggerDisplay("SlotTypeName = {Name}")]
    [DataContract]
    public class SlotType : IStoryItem
    {
        [JsonProperty("sysId")]
        [DataMember]
        public long? Id { get; set; }

     
        [DataMember]
        public Guid? UniqueId { get; set; }

        [Required]
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<SlotValue> Values { get; set; }


    }


    [DebuggerDisplay("SlotValue = {Value}")]
    [JsonObject(IsReference = false)]
    [DataContract]
    public class SlotValue 
    {

        [Required]
        [DataMember]
        public string Value { get; set; }


  
        [DataMember]
        public List<string> Synonyms { get; set; }


     
    }
}
