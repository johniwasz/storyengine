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

    [MessagePackObject]
    [DataContract]
    public class UniqueItem : InventoryItemBase
    {

        public UniqueItem()
        {
            this.ItemType = InventoryItemType.Unique;

        }

        public UniqueItem(string itemName)
        {
            this.Name = itemName;
            this.ItemType = InventoryItemType.Unique;

        }


        [Key(1)]
        [YamlMember]
        [DataMember]
        [JsonProperty("name", Order = 1)]
        public override string Name { get; set; }

        [Key(2)]
        [YamlIgnore]
        [NotMapped]
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("itemType", Order = 0)]
        public override InventoryItemType ItemType { get; set; }


        public override bool Equals(object obj)
        {
            if (obj is UniqueItem compareItem)
                return this.Name.Equals(compareItem.Name, StringComparison.OrdinalIgnoreCase);


            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        public override string ToString()
        {
            return string.Concat("UniqueItem(", Name, ")");
        }


    }
}
