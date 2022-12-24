using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Tracking
{
    [JsonObject]
    [DataContract]
    [MessagePackObject]
    public class MultiItem : InventoryItemBase
    {


        public MultiItem()
        {
            this.ItemType = InventoryItemType.Multi;
            this.Count = 0;
        }


        public MultiItem(string itemName)
        {
            this.ItemType = InventoryItemType.Multi;
            this.Name = itemName;
            this.Count = 0;

        }

        [Key(1)]
        [YamlMember]
        [DataMember]
        [JsonProperty("name", Order = 1)]
        public sealed override string Name { get; set; }



        /// <summary>
        /// Number of times the item is in the user's inventory.
        /// </summary>
        [Key(2)]
        [YamlIgnore]
        [DataMember]
        [JsonProperty("count", Order = 2)]
        public int Count { get; set; }

        [Key(3)]
        [NotMapped]
        [YamlIgnore]
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("itemType", Order = 0)]
        public sealed override InventoryItemType ItemType { get; set; }


        public override bool Equals(object obj)
        {
            if (obj is MultiItem compareItem)
                Name.Equals(compareItem.Name, StringComparison.OrdinalIgnoreCase);

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Concat("MultiItem(", Name, ", Count ", Count, ")");
        }


    }
}
