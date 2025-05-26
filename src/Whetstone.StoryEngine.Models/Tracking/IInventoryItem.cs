using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Xml.Serialization;

namespace Whetstone.StoryEngine.Models.Tracking
{
    public enum InventoryItemType
    {
        Unique = 0,
        Multi = 1
    }

    // [JsonConverter(typeof(ItemTypeConverter))]
    [XmlInclude(typeof(UniqueItem))]
    [XmlInclude(typeof(MultiItem))]
    [MessagePack.Union(0, typeof(UniqueItem))]
    [MessagePack.Union(1, typeof(MultiItem))]
    public abstract class InventoryItemBase : IStoryCrumb
    {
        [MessagePack.Key(0)]
        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }

        /// <summary>
        /// Internal name used to identify the item.
        /// </summary>
        [IgnoreMember]
        [JsonProperty(PropertyName = "name")]
        public abstract string Name { get; set; }

        [IgnoreMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "itemType")]
        public abstract InventoryItemType ItemType { get; set; }

    }
}
