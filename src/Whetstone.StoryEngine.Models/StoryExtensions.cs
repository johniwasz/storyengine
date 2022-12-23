using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Whetstone.StoryEngine.Models.Conditions;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Models
{
    public static class StoryExtensions
    {

        public static T GetLocalizeText<T>(this IEnumerable<T> localizedList)
            where T : class, ILocalizedItem
        {
            return GetLocalizeText(localizedList, null);
        }

        //public static StoryModel ToStory(this DataTitle dataStory)
        //{
        //    StoryModel retTitle = null;

        //    if(dataStory !=null)
        //    {
        //        retTitle = new StoryModel();

        //        retTitle.ShortName = dataStory.ShortName;
        //        retTitle.Id = dataStory.Id;
        //        retTitle.UniqueId = dataStory.UniqueId;
        //        retTitle.Title = dataStory.Title;
        //        retTitle.Description = dataStory.Description;

        //        if((dataStory.Versions?.Any()).GetValueOrDefault(false))
        //        {
        //            retTitle.Versions = new List<StoryVersion>();
        //            foreach(DataTitleVersion dataVer in dataStory.Versions)
        //            {
        //                retTitle.Versions.Add(dataVer.ToStoryVersion());
        //            }
        //        }

        //        retTitle.InvocationNames = dataStory.InvocationNames;


        //    }

        //    return retTitle;


        //}
//        public static StoryVersion ToStoryVersion(this DataTitleVersion dataVersion)
//        {
//            StoryVersion storyVersion = new StoryVersion();

//           // storyVersion.Id = dataVersion.Id;
//            storyVersion.UniqueId = dataVersion.UniqueId;
//            storyVersion.Version = dataVersion.Version;
//            storyVersion.PublishDate = dataVersion.PublishDate;
//            storyVersion.Id = dataVersion.Id;

//            if ((dataVersion.Slots?.Any()).GetValueOrDefault(false))
//            {
//                storyVersion.Slots = new List<SlotType>();

//                foreach(DataSlotType dataSlotType in dataVersion.Slots)
//                {
//                    if(dataSlotType!=null)
//                     storyVersion.Slots.Add(dataSlotType.ToSlotType());
//                }
//            }

//            if ((dataVersion.Intents?.Any()).GetValueOrDefault(false))
//            {
//                storyVersion.Intents = new List<Intent>();
               
//                foreach(DataIntent sourceIntent in dataVersion.Intents)
//                {
//                    storyVersion.Intents.Add(sourceIntent.ToIntent(storyVersion.Slots));
//                }
//            }

//            if((dataVersion.InventoryItems?.Any()).GetValueOrDefault(false))
//            {
//                storyVersion.InventoryItems = new List<DataInventoryItem>();
//                // ReSharper disable once AssignNullToNotNullAttribute
//                storyVersion.InventoryItems.AddRange(dataVersion.InventoryItems);
//            }


//            // Add inventory items
//            if((dataVersion.DataInventoryConditions?.Any()).GetValueOrDefault(false))
//            {
////storyVersion.InventoryConditions
//                storyVersion.InventoryConditions = new List<InventoryCondition>();

//                // ReSharper disable once PossibleNullReferenceException
//                foreach (DataInventoryCondition dataCondition in dataVersion.DataInventoryConditions)
//                {

//                    List<DataInventoryItem> foundItems = new List<DataInventoryItem>();
//                    if (dataCondition.InventoryConditionXRefs != null)
//                    {
//                        // find the related inventory items.
//                       foundItems =
//                            dataCondition.InventoryConditionXRefs.Where(x => x.Condition == dataCondition)
//                                .Select(x => x.InventoryItem).ToList();
//                    }
//                    storyVersion.InventoryConditions.Add(dataCondition.ToInventoryCondition(foundItems));

//                }


//            }

//            return storyVersion;
//        }

        public static SlotType ToSlotType(this DataSlotType dataSlotType)
        {
            SlotType retSlotType = null;

            if(dataSlotType!=null)
            {
                retSlotType = new SlotType();
                retSlotType.Name = dataSlotType.Name;
                retSlotType.Id = dataSlotType.Id;
                retSlotType.UniqueId = dataSlotType.UniqueId;
                retSlotType.Values = dataSlotType.Values;

               //if((dataSlotType.Values?.Any()).GetValueOrDefault(false))
               // {
               //     retSlotType.Values = new List<SlotValue>();
               //     foreach(DataSlotValue dataVal in dataSlotType.Values)
               //     {
               //         retSlotType.Values.Add(dataVal.ToSlotValue());
               //     }

               // }
            }

            return retSlotType;
        }

        public static Intent ToIntent(this DataIntent dataIntent, List<SlotType> slotTypes)
        {
            Intent retIntent = null;
            if(dataIntent!=null)
            {
                retIntent = new Intent();
                retIntent.Id = dataIntent.Id;
                retIntent.Name = dataIntent.Name;

                retIntent.UniqueId = dataIntent.UniqueId;

                if((dataIntent.LocalizedIntents?.Any()).GetValueOrDefault(false))
                {
                    retIntent.LocalizedIntents = dataIntent.LocalizedIntents;

                    if((slotTypes?.Any()).GetValueOrDefault(false) && 
                         (dataIntent.SlotTypeMappings?.Any()).GetValueOrDefault(false))
                    {

                        retIntent.SlotMappings = new List<IntentSlotMapping>();

                        foreach(DataIntentSlotMapping dataMapping in dataIntent.SlotTypeMappings)
                        {
                            retIntent.SlotMappings.Add(dataMapping.ToIntentSlotMapping(slotTypes));
                        }
                    } 
                }


            }
            return retIntent;
        }
        

        //public static DataTitleVersion ToDataStoryVersion(this StoryVersion version)
        //{
        //    DataTitleVersion dataVersion = null;

        //    if(version!=null)
        //    {
        //        dataVersion = new DataTitleVersion();

        //        if (version.UniqueId.HasValue)
        //            dataVersion.UniqueId = version.UniqueId.Value;
        //        else
        //            dataVersion.UniqueId = Guid.NewGuid();

        //        dataVersion.Id = version.Id;
        //        dataVersion.Version = version.Version;

        //        dataVersion.PublishDate = version.PublishDate;

        //        if((version.Slots?.Any()).GetValueOrDefault(false))
        //        {

        //            dataVersion.Slots = new List<DataSlotType>();

        //            foreach(SlotType st in version.Slots)
        //            {

        //                dataVersion.Slots.Add(st.ToDataSlotType());
        //            }
        //        }

        //        if((version.Intents?.Any()).GetValueOrDefault(false))
        //        {
        //            dataVersion.Intents = new List<DataIntent>();
        //            foreach(Intent intent in version.Intents)
        //            {

        //                DataIntent dataIntent = intent.ToDataIntent(dataVersion.Slots);
        //                dataVersion.Intents.Add(dataIntent);

        //                if (intent.Name.Equals("ChopTreeIntent"))
        //                    Debug.WriteLine("Here!");

        //                if((intent.SlotMappings?.Any()).GetValueOrDefault(false))
        //                {
        //                    dataIntent.SlotTypeMappings = new List<DataIntentSlotMapping>();
        //                    foreach (IntentSlotMapping slotMapping in intent.SlotMappings)
        //                    {

        //                        DataSlotType foundSlot = dataVersion.Slots.FirstOrDefault(x => slotMapping.SlotType.Name.Equals(x.Name, StringComparison.OrdinalIgnoreCase));

        //                        if (foundSlot != null)
        //                        {
        //                            dataIntent.SlotTypeMappings.Add(slotMapping.ToDataIntentSlotMapping(foundSlot, dataIntent));
        //                        }

        //                    }
        //                }

        //            }

        //        }


        //        if ((version.InventoryItems?.Any()).GetValueOrDefault(false))
        //        {
        //            dataVersion.InventoryItems = version.InventoryItems;
        //        }

        //        if ((version.InventoryConditions?.Any()).GetValueOrDefault(false))
        //        {
        //            dataVersion.DataInventoryConditions = new List<DataInventoryCondition>();

        //            // ReSharper disable once PossibleNullReferenceException
        //            foreach (InventoryCondition invCondition in version.InventoryConditions)
        //            {
                        
        //                dataVersion.DataInventoryConditions.Add(invCondition.ToDataInventoryCondition(dataVersion));


        //            }



        //        }



        //    }



        //    return dataVersion;
        //}

        public static DataIntentSlotMapping ToDataIntentSlotMapping(this IntentSlotMapping mapping, DataSlotType foundSlot, DataIntent intent)
        {
            DataIntentSlotMapping retMapping = new DataIntentSlotMapping();

            retMapping.Alias = mapping.Alias;
            retMapping.SlotType = foundSlot;
            if (foundSlot != null)
                retMapping.SlotTypeId = foundSlot.Id;

            retMapping.Intent = intent;

            if (intent != null)
                retMapping.IntentId = intent.Id;

            return retMapping;


        }


        //public static LocalizedIntent ToLocalizedIntent(this DataLocalizedIntent dataLocIntent)
        //{
        //    LocalizedIntent retIntent = null;

        //    if(dataLocIntent!=null)
        //    {
        //        retIntent = new LocalizedIntent();
            
        //        retIntent.UniqueId = dataLocIntent.UniqueId;
        //        retIntent.Locale = dataLocIntent.Locale;
        //        retIntent.PlainTextPrompt = dataLocIntent.PlainTextPrompt;
              
        //        if((dataLocIntent.Utterances?.Any()).GetValueOrDefault(false))
        //        {
        //            retIntent.Utterances = new List<string>();
        //            retIntent.Utterances.AddRange(dataLocIntent.Utterances);
        //        }
        //    }
        //    return retIntent;
        //}

        //public static SlotValue ToSlotValue(this DataSlotValue dataSlotValue)
        //{
        //    SlotValue val = null;

        //    if(dataSlotValue!=null)
        //    {
        //        val = new SlotValue();
             
        //      //  val.UniqueId = dataSlotValue.UniqueId;
        //        val.Value = dataSlotValue.Value;
        //        if ((dataSlotValue.Synonyms?.Any()).GetValueOrDefault(false))
        //        {
        //            val.Synonyms = new List<string>();
        //            val.Synonyms.AddRange(dataSlotValue.Synonyms);
        //        }
        //    }
        //    return val;
        //}


        public static T GetLocalizeText<T>(this IEnumerable<T> localizedList, string locale) where T : class, ILocalizedItem
        {
            T localizedItem = null;

            if (localizedList != null && localizedList.Any())
            {
                if (string.IsNullOrWhiteSpace(locale))
                {
                    localizedItem = localizedList.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.Locale));

                }
                else
                {

                    localizedItem = localizedList.FirstOrDefault(x =>
                    {


                        if (string.IsNullOrWhiteSpace(x.Locale))
                            return false;
                        else
                        {
                            return x.Locale.Equals(locale, StringComparison.OrdinalIgnoreCase);

                        }
                    });


                    if (localizedItem == null)
                    {
                        localizedItem = localizedList.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.Locale));
                    }
                }

            }
            return localizedItem;
        }


        public static string GetLocalizedPlainText(this IEnumerable<LocalizedPlainText> localizedList, string locale)
        {
            string localizedText = null;

            LocalizedPlainText localizedItem = GetLocalizeText(localizedList, locale);

            localizedText = localizedItem?.Text;

            return localizedText;


        }


        /// <summary>
        /// Given a slot value, it returns the matched value and synonyms. It searches based on the value and the synonym.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static List<string> GetMatchedValues(this SlotType slot, string value)
        {

            List<string> returnVals = new List<string>();

            SlotValue slotVal = slot.Values.FirstOrDefault(x =>
                x.Value.Equals(value, StringComparison.OrdinalIgnoreCase) ||
                (x.Synonyms?.Contains(value, StringComparer.OrdinalIgnoreCase)).GetValueOrDefault(false));

            if (slotVal != null)
            {
                returnVals.Add(slotVal.Value);

                if (slotVal.Synonyms != null && slotVal.Synonyms.Any())
                    returnVals.AddRange(slotVal.Synonyms);
            }

            return returnVals;
        }


        public static IntentSlotMapping ToIntentSlotMapping(this DataIntentSlotMapping dataSlotMapping, List<SlotType> slotTypes)
        {
            IntentSlotMapping retMapping = null;

            if(dataSlotMapping!=null)
            {
                retMapping = new IntentSlotMapping();
                retMapping.Alias = dataSlotMapping.Alias;

                if(dataSlotMapping.SlotType!=null)
                {
                    if((slotTypes?.Any()).GetValueOrDefault(false))
                    {
                        var foundSlot =  slotTypes.FirstOrDefault(x => x.Name.Equals(dataSlotMapping.SlotType.Name, StringComparison.OrdinalIgnoreCase));
                        if (foundSlot != null)
                            retMapping.SlotType = foundSlot;
                    }
                }
                
            }

            return retMapping;
        }


        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>
            (this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct =
                new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                    from accseq in accumulator
                    from item in sequence
                    select accseq.Concat(new[] { item }));
        }


        public static string ToOrdinal(this int value)
        {
            // Start with the most common extension.
            string extension = "th";

            // Examine the last 2 digits.
            int last_digits = value % 100;

            // If the last digits are 11, 12, or 13, use th. Otherwise:
            if (last_digits < 11 || last_digits > 13)
            {
                // Check the last digit.
                switch (last_digits % 10)
                {
                    case 1:
                        extension = "st";
                        break;
                    case 2:
                        extension = "nd";
                        break;
                    case 3:
                        extension = "rd";
                        break;
                }
            }

            return string.Concat(value.ToString(), extension);
        }

    }
}
