using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;

namespace Whetstone.StoryEngine.Models.Story
{
    [JsonObject(Title ="LocResponseSet")]
    [DataContract]
    [Table("LocResponseSet")]
    public class LocalizedResponseSet
    {
        [JsonProperty(PropertyName ="localizedResponses"  )]
        [DataMember]
        public List<LocalizedResponse> LocalizedResponses { get; set; }
    }
}
