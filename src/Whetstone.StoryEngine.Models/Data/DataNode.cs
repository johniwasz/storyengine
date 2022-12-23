using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Story;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Data
{
    [DataContract]
    [JsonObject]
    [DebuggerDisplay("Node Name = {Name}")]
    [Table("StoryNodes")]
    public class DataNode 
    {
        public DataNode()
        {
     
        }


        public DataNode(string coordText)
        {
     
            this.CoordinatesJson = coordText;


        }

        /// <summary>
        /// Node Id
        /// </summary>
        [JsonProperty]
        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [System.ComponentModel.DataAnnotations.Key]
        public long? Id { get; set; }
        
        [JsonProperty]
        [DataMember()]
        public string Name { get; set; }



        [JsonProperty]
        [DataMember()]
        [ForeignKey("StoryNodeId")]
        public List<DataLocalizedResponseSet> ResponseSet { get; set; }


        [JsonProperty]
        [DefaultValue(ResponseBehavior.SelectFirst)]
        [DataMember()]
        [ForeignKey("StoryNodeId")]
        public ResponseBehavior ResponseBehavior { get; set; }

        [JsonProperty]
        [IgnoreDataMember]
        [InverseProperty("Node")]
        [ForeignKey("StoryNodeId")]
        public List<DataChoice> Choices { get; set; }


        /// <summary>
        /// Used to change user state or perform some action on user state.
        /// </summary>
        /// <remarks>This includes things like saving node visits or changing inventory status.</remarks>
        [JsonProperty]
        [DataMember()]
        [ForeignKey("StoryNodeId")]
        public List<NodeActionData> Actions { get; set; }




        // [JsonConverter(typeof(EnumerableConverter))]

        [IgnoreDataMember]
        public IEnumerable<DataNodeVisitConditionXRef> NodeVisitConditionXRefs { get; set; }

        [Column("Coordinates", TypeName = "jsonb")]
        private string CoordinatesJson { get; set; }


        [IgnoreDataMember]
        public long? VersionId { get; set; }


        [ForeignKey("VersionId")]
        [DataMember]
        public DataTitleVersion Version { get; set; }

        [IgnoreDataMember]
        public long? ChapterId { get; set; }

        [DataMember]
        [ForeignKey("ChapterId")]
        public DataChapter Chapter { get; set; }

        [JsonProperty]
        [NotMapped]
        [DataMember]
        public Coordinates Coordinates
        {
            get
            {
                return JsonConvert.DeserializeObject<Coordinates>(CoordinatesJson);
            }
            set
            {
                CoordinatesJson = JsonConvert.SerializeObject(value);
            }
        }
    }
}
