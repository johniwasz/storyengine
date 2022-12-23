using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Tracking;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Conditions
{
    [JsonObject]
    [DataContract]
    public class InventoryCondition : StoryConditionBase
    {


        /// <summary>
        /// A list of items required in inventory. This is all or nothing. All items must be found in inventory.
        /// This is used only for YAML serialization and will be removed once the application moves to a JSON model.
        /// </summary>
        [DataMember]
        [YamlMember(Alias = "requiredInventoryItems")]
        [JsonIgnore]
        public List<InventoryItemBase> RequiredStoryInventoryItems { get; set; }


        /// <summary>
        /// This is used for API serialization.
        /// </summary>
        [JsonProperty("inventoryItems", Order = 1)]
        [YamlIgnore]
        [DataMember]
        public List<DataInventoryItem> InventoryItems { get; set; }

        public InventoryCondition()
        {
            this.ConditionType = ConditionType.Inventory;
        }


        public InventoryCondition(string name)
        {
            this.ConditionType = ConditionType.Inventory;
            this.Name = name;
        }

        [YamlIgnore]
        [DataMember]
        [NotMapped]
        [JsonProperty("conditionType", Order =0)]
        public sealed override ConditionType ConditionType { get; set; }

        public override bool IsStoryCondition(ConditionInfo conditionInfo)
        {
            if (conditionInfo == null)
                throw new ArgumentNullException($"{nameof(conditionInfo)}");

            int foundCount = 0;
            int itemTypesCount = RequiredStoryInventoryItems.Count;

            var storyCrumbs = conditionInfo.Crumbs;

            if ((storyCrumbs?.Any()).GetValueOrDefault(false))
            {
                foreach (InventoryItemBase invItem in RequiredStoryInventoryItems)
                {
                    if (storyCrumbs.Contains(invItem))
                        foundCount++;
                }
            }

            bool areAllItemsFound = foundCount == itemTypesCount;


            if (RequiredOutcome.GetValueOrDefault(true))
                return areAllItemsFound;
            else
                return !areAllItemsFound;
        }
    }
}
