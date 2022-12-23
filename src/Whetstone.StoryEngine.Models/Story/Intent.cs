using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using System.Diagnostics;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Integration;

namespace Whetstone.StoryEngine.Models.Story
{

   // [JsonConverter(typeof(DollarIdPreservingConverter<Intent>))]
 
    public static class WhetstoneIntents
    {
        public static readonly string US_PHONENUMBER_INTENT = "WHETSTONE.US_PHONENUMBER";

        public static readonly string US_CITY_INTENT = "WHETSTONE.US_CITY";

        public static readonly string FOUR_DIGIT_NUMBER_INTENT = "WHETSTONE.FOUR_DIGIT_NUMBER";

        public static readonly string NUMBER_INTENT = "WHETSTONE.NUMBER";
    }

    [DebuggerDisplay("Intent Name = {Name}")]
    [DataContract]
    [MessageObject]
    [JsonObject("Encapsulates the intent")]
    public class Intent : IStoryItem
    {


        public Intent()
        {

        }

        public Intent(string name)
        {
            this.Name = name;

        }

        [DataMember]
        [Required]
        [YamlMember(Alias = "name", Order = 0)]
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [DataMember]
        [YamlMember(Alias = "id", Order = 1)]
        [JsonProperty(PropertyName ="sysId")]
        public long? Id { get; set; }


        /// <summary>
        /// If true, then the intent can be used to support Alexa's nameless invocation interface.
        /// </summary>
        [JsonProperty("supportsNamelessInvocation", Order = 2)]
        [YamlMember(Alias = "supportsNamelessInvocation")]
        [DataMember]
        public bool? SupportsNamelessInvocation { get; set; }


        [JsonProperty("canInvokeSlotValidator")]
        [YamlMember(Alias = "canInvokeSlotValidator", Order = 3)]
        [DataMember]
        public DataRetrievalAction CanInvokeSlotValidator { get; set; }

        [DataMember]
        [YamlIgnore]
        //[JsonIgnore()]
        [JsonProperty("uniqueId")]
        public Guid? UniqueId { get; set; }

        /// <summary>
        /// Plain text name of the intent. It must be unique.
        /// </summary>



        [DataMember]
        [YamlMember(Alias = "localizedIntents")]
        [JsonProperty("localizedIntents")]
        public List<LocalizedIntent> LocalizedIntents { get; set; }

        /// <summary>
        /// An intent can be associated with a title or a global reusable intent.
        /// </summary>
        [NotMapped]
        [YamlIgnore]
        [JsonIgnore]
        [IgnoreDataMember]
        public string TitleId { get; set; }


        /// <summary>
        /// This is left over from the YAML file. 
        /// </summary>
      //  [Obsolete("Use SlotMappings. This relationship is outdated in the newer relational model.")]
        [YamlMember(Alias = "slotMappings")]
        [JsonIgnore]
        [DataMember]
        public Dictionary<string, string> SlotMappingsByName { get; set; }

        [DataMember]
        [YamlIgnore]
        [JsonProperty("slotMappings")]
        public List<IntentSlotMapping> SlotMappings { get; set; }


        /// <summary>
        /// Used to change user state or perform some action on user state.
        /// </summary>
        /// <remarks>This includes things like saving node visits or changing inventory status.</remarks>
        [YamlMember(Alias ="actions")]
        [JsonProperty("actions")]
        [DataMember()]        
        public List<NodeActionData> Actions { get; set; }

    }


}
