using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Whetstone.StoryEngine.Models.Tracking;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Actions
{


    public enum InventoryActionType
    {
        /// <summary>
        /// Adds the item to the user's inventory
        /// </summary>
        [EnumMember(Value = "add")]
        Add = 0,

        /// <summary>
        /// Remove the item from inventory
        /// </summary>
        [EnumMember(Value = "remove")]
        Remove = 1,


        /// <summary>
        /// Removes all items by resetting count to 0.
        /// </summary>
        [EnumMember(Value = "clear")]
        Clear = 2


    }

    /// <summary>
    /// Apply a change to the user's inventory
    /// </summary>
    [DataContract]
    [JsonObject]
    [MessageObject]
    public class InventoryActionData : NodeActionData
    {


        public InventoryActionData()
        {
            NodeAction = NodeActionEnum.Inventory;

        }


        [YamlIgnore]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("nodeAction", Order = 0)]
        [DataMember]
        public sealed override NodeActionEnum NodeAction { get; set; }


        [JsonProperty("item", Order = 1, NullValueHandling = NullValueHandling.Ignore)]
        [NotMapped]
        [DataMember]
        public InventoryItemBase Item { get; set; }



        /// <summary>
        /// Default to Add
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("inventoryActionType", Order = 2)]
        [DefaultValue(InventoryActionType.Add)]
        [DataMember]
        [YamlMember]
        public InventoryActionType ActionType { get; set; }

    }


}
