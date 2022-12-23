using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Data;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Story
{

    [JsonObject(Title = "standardNode")]
    [DataContract]
    public class StandardNode
    {
        
        [JsonProperty(PropertyName ="id")]
        [DataMember]
        public long? Id { get; set; }

        [JsonProperty(PropertyName = "nodeType")]
        [DataMember]
        public StoryNodeType NodeType { get; set; }

        [JsonProperty(PropertyName = "node")]
        [DataMember]
        public DataNode Node { get; set; }

    }
}