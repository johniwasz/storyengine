using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Tracking
{

    /// <summary>
    /// This is used to store user slot choices. 
    /// </summary>
    /// <remarks>
    /// For example, if user makes an open ended slot selection, like a date or free form text, then this records the selection.
    /// </remarks>
    [JsonObject]
    [DataContract]
    [MessagePackObject]
    public class SelectedItem : IStoryCrumb
    {

        public SelectedItem()
        {
            
        }

        [MessagePack.Key(0)]
        [System.ComponentModel.DataAnnotations.Key()]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DataMember]
        public Guid? Id { get; set; }

        [Key(1)]
        [YamlMember]
        [DataMember]
        [JsonProperty("name", Order = 1)]
        public string Name { get; set; }


        [Key(2)]
        [YamlMember]
        [DataMember]
        [JsonProperty("value", Order = 2)]
        public string Value { get; set; }


        public override string ToString()
        {
            return string.Concat("SelectedItem(", Name, "=", Value, ")");
        }

    }
}
