using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.Serialization;

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
