using System.Collections.Generic;
using System.Diagnostics;
using YamlDotNet.Serialization;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Whetstone.StoryEngine.Models.Actions;
using System.ComponentModel;
using Whetstone.StoryEngine.Models.Integration;

namespace Whetstone.StoryEngine.Models.Story
{



   public enum ResponseBehavior
    {
        SelectFirst,
        Random
    }


    public enum AuditBehavior
    {
        /// <summary>
        /// This is the default behavior
        /// </summary>
        RecordAll =0,
        /// <summary>
        /// Records the response the engine sends. Does not record the user's input.
        /// </summary>
        RecordEngineResponseOnly =1,
        /// <summary>
        /// Do not save any audit data for the current node.
        /// </summary>
        RecordNone=2

    }

    [JsonObject]
    [DataContract]
    [DebuggerDisplay("Node name: {Name}, Id: {Id}")]
    public class StoryNode : IStoryEntity
    {


        public StoryNode()
        {

        }

        /// <summary>
        /// Node Id
        /// </summary>

        [DataMember]
        public long? Id { get; set; }

        [YamlMember(Order = 0)]
        [DataMember()]
        public string Name { get; set; }


        [YamlMember(Order = 1)]
        [DataMember]
        public List<DataRetrievalAction> DataRetrievalActions { get; set; }


        [YamlMember(Order = 2)]
        [DataMember()]
        public List<LocalizedResponseSet> ResponseSet { get; set; }


        [YamlMember(Order = 3)]
        [DataMember()]
        public ResponseBehavior ResponseBehavior { get; set; }


        [JsonProperty("auditBehavior", IsReference = false)]
        [YamlMember(Order = 4)]
        [DataMember()]
        public AuditBehavior? AuditBehavior { get; set; }

        [YamlMember(Order = 5)]
        [DataMember()]
        [InverseProperty("Node")]
        public List<Choice> Choices { get; set; }

        [YamlIgnore]
        [DataMember]
        public string TitleId { get; set; }

        [IgnoreDataMember]
        [ForeignKey("TitleId")]
        public StoryTitle Title { get; set; }

       

        /// <summary>
        /// Used to change user state or perform some action on user state.
        /// </summary>
        /// <remarks>This includes things like saving node visits or changing inventory status.</remarks>
        [YamlMember(Order = 6)]     
        [DataMember()]       
        public List<NodeActionData> Actions { get; set; }


        [JsonProperty("localizedSuggestions", IsReference = false)]
        [YamlMember(Order = 7)]
        [DataMember]
        public List<List<LocalizedPlainText>> LocalizedSuggestions { get; set; }

        [JsonProperty("coordinates", IsReference = false)]
        [YamlMember(Order = 8)]
        [DataMember]
        public Coordinates Coordinates { get; set; }

    }

}
