using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using Whetstone.StoryEngine.Models.Tracking;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Conditions
{

    /// <summary>
    /// Determines the type of slot value comparison performed by the SlotValueCondition check.
    /// </summary>
    public enum SlotValueConditionCheckType
    {
        /// <summary>
        /// If the slot has a value is that is not null and not an empty string, then return true.
        /// </summary>
        [EnumMember(Value = "notempty")]
        NotEmpty = 0,
        /// <summary>
        /// If the slot has a value that is null or an empty string, then return true.
        /// </summary>
        [EnumMember(Value = "isempty")]
        IsEmpty = 1,
        /// <summary>
        /// Compare the slot value to the Value property using a case insensitive compare. Return true if equal.
        /// </summary>
        [EnumMember(Value = "equals")]
        Equals = 2
    }


    /// <summary>
    /// Use to evaluate a slot value.
    /// </summary>
    [DataContract]
    public class SlotValueCondition : StoryConditionBase
    {
        [NotMapped]
        [YamlMember]
        [JsonProperty("conditionType", Order = 0)]
        [DataMember]
        public override ConditionType ConditionType
        {
            get { return ConditionType.SlotValue; }

            set { /* do nothing */ }

        }

        public override bool IsStoryCondition(ConditionInfo condInfo)
        {
            bool isConditionMet = false;
            if (condInfo == null)
                throw new ArgumentNullException($"{nameof(condInfo)}");

            if (string.IsNullOrWhiteSpace(SlotName))
                throw new ArgumentException("Slot name is not specified.");


            List<SelectedItem> selItems = new List<SelectedItem>();

            if ((condInfo.Crumbs?.Any()).GetValueOrDefault(false))
            {
                foreach (IStoryCrumb crumb in condInfo.Crumbs)
                {
                    SelectedItem selItem = crumb as SelectedItem;
                    if (selItem != null)
                        selItems.Add(selItem);
                }
            }

            if (selItems.Any())
            {

                SelectedItem foundItem = selItems.FirstOrDefault(x => x.Name.Equals(SlotName, StringComparison.OrdinalIgnoreCase));
                string foundValue = null;

                if (foundItem != null)
                    foundValue = foundItem.Value;



                switch (ValueCheckType)
                {
                    case SlotValueConditionCheckType.Equals:
                        if (string.IsNullOrWhiteSpace(foundValue))
                        {
                            isConditionMet = string.IsNullOrWhiteSpace(Value);
                        }
                        else
                        {
                            isConditionMet = foundValue.Equals(Value, StringComparison.OrdinalIgnoreCase);
                        }
                        break;
                    case SlotValueConditionCheckType.IsEmpty:
                        isConditionMet = string.IsNullOrWhiteSpace(foundValue);
                        break;
                    case SlotValueConditionCheckType.NotEmpty:
                        isConditionMet = !string.IsNullOrWhiteSpace(foundValue);
                        break;
                }
            }




            return isConditionMet;
        }

        /// <summary>
        /// This is the name of the slot value to check.
        /// </summary>
        [YamlMember]
        [JsonProperty("slotName", Order = 1)]
        [DataMember]
        public string SlotName { get; set; }

        /// <summary>
        /// Determine how to evaluate the slot value. It could check for null, not null and equal comparisons.
        /// </summary>
        [YamlMember]
        [JsonProperty("valueCheckType", Order = 2)]
        [DataMember]
        public SlotValueConditionCheckType ValueCheckType { get; set; }

        /// <summary>
        /// Value to compare to the slot value. This applies if the 
        /// </summary>
        [YamlMember]
        [JsonProperty("value", Order = 3, NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        public string Value { get; set; }




    }
}
