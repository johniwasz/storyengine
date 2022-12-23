using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace Whetstone.StoryEngine.Models.Story
{

    [JsonObject(Title = "Chapter")]
    [DataContract]
    public class StoryChapter
    {

        [JsonProperty(PropertyName = "id")]
        [DataMember]
        public long? Id { get; set; }

        //[Required]
        //[DataMember]
        //public int Sequence { get; set; }

        [JsonProperty(PropertyName = "names")]
        [DataMember]
        public List<LocalizedPlainText> Names { get; set; }

        /// <summary>
        /// This is for serialization and deserializaton.
        /// </summary>
        /// <remarks>Do not store to the database in this format.</remarks>    
        [JsonProperty(PropertyName = "nodes")]
        [DataMember] 
        public List<StoryNode> Nodes { get; set; }

    }
}
