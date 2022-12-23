using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;

namespace Whetstone.StoryEngine.Models
{

  //  [JsonConverter(typeof(DollarIdPreservingConverter<IntentSlotMapping>))] 
    [DebuggerDisplay("Intent Alias = {Alias}")]
    [DataContract]
    public class IntentSlotMapping 
    {

        [DataMember]
        [Required]
        public string Alias { get; set; }

        [DataMember]
        public SlotType SlotType { get; set; }
    }
}
