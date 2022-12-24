using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.Alexa;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.AlexaProcessor
{
    public class AlexaIntentExporter : IAlexaIntentExporter
    {

#pragma warning disable CS1998
        public async Task<InteractionModel> GetIntentModelAsync(StoryTitle storyTitle, string locale)
        {
#pragma warning restore CS1998
            ICollection<StoryNode> allNodes = storyTitle.Nodes;

            InteractionModel alexaModel = new InteractionModel
            {
                LanguageModel = new LanguageModel()
            };

            string invokeText = storyTitle.InvocationNames.GetLocalizedPlainText(locale);

            if (!string.IsNullOrWhiteSpace(invokeText))
            {
                alexaModel.LanguageModel.InvocationName = invokeText;
            }
            else
            {
                throw new Exception("No invocation name found. Review title configuration.");
            }

            var mappedIntents = new List<IntentModel>
            {

                // Get the title intents


                new IntentModel("AMAZON.HelpIntent"),

                new IntentModel("AMAZON.StopIntent"),
                new IntentModel("AMAZON.CancelIntent"),

                new IntentModel("AMAZON.StartOverIntent"),

                new IntentModel("AMAZON.NavigateHomeIntent")
            };

            // Fallback intent only available in the US currently (7/1/18)
            if (locale.Equals("en-US", StringComparison.OrdinalIgnoreCase))
                mappedIntents.Add(new IntentModel("AMAZON.FallbackIntent"));


            List<Intent> intentLib = new List<Intent>();

            if ((storyTitle.Intents?.Any()).GetValueOrDefault(false))
            {
                foreach (Intent storyIntent in storyTitle.Intents)
                {
                    AddIfNewIntent(mappedIntents, storyIntent, locale);
                }


                //ApplyNodeIntent(intentLib, locale, mappedIntents, globalConfig.HelpNode);

                //ApplyNodeIntent(intentLib, locale, mappedIntents, globalConfig.WelcomeBackNode);

                //ApplyNodeIntent(intentLib, locale, mappedIntents, globalConfig.LaunchNode);

                //ApplyNodeIntent(intentLib, locale, mappedIntents, globalConfig.ErrorNode);

                //ApplyNodeIntent(intentLib, locale, mappedIntents, globalConfig.EndStoryNode);

                //ApplyNodeIntent(intentLib, locale, mappedIntents, globalConfig.RestartNode);
            }

            if ((allNodes?.Any()).GetValueOrDefault(false))
            {
                foreach (StoryNode node in allNodes)
                {
                    ApplyNodeIntent(intentLib, locale, mappedIntents, node);
                }
            }

            foreach (var intentModel in mappedIntents)
            {

                List<IntentModel> foundIntents = mappedIntents.FindAll(x => x.Name.Equals(intentModel.Name));
                if (foundIntents.Count > 1)
                {

                    Console.WriteLine(string.Format("Found duplicate {0} intent {1} times", intentModel.Name,
                        foundIntents.Count));

                }
            }

            alexaModel.LanguageModel.Intents = mappedIntents;

            if ((storyTitle.Slots?.Any()).GetValueOrDefault(false))
            {
                List<SlotTypeModel> slotTypes = new List<SlotTypeModel>();

                foreach (var slotVar in storyTitle.Slots)
                {
                    SlotTypeModel slotType = new SlotTypeModel();


                    var (SlotTypeName, IsAmazonIntent) = MapToAmazonSlot(slotVar.Name);
                    if (!IsAmazonIntent)
                    {
                        slotType.Name = SlotTypeName;


                        if ((slotVar.Values?.Any()).GetValueOrDefault(false))
                        {
                            slotType.Values = new List<SlotEntry>();
                            foreach (var slotVal in slotVar.Values)
                            {
                                SlotEntry slotEntry = new SlotEntry
                                {
                                    SlotValue = new SlotValueItem()
                                };

                                slotEntry.SlotValue.Value = slotVal.Value;

                                if ((slotVal.Synonyms?.Any()).GetValueOrDefault(false))
                                    slotEntry.SlotValue.Synonyms = slotVal.Synonyms;

                                slotType.Values.Add(slotEntry);

                            }

                        }
                        slotTypes.Add(slotType);
                    }
                }

                alexaModel.LanguageModel.SlotTypes = slotTypes;

                return alexaModel;
            }

            return null;
        }


        private void ApplyNodeIntent(List<Intent> intentLib, string locale, List<IntentModel> mappedIntents, StoryNode node)
        {

            if (node?.Choices != null)
            {

                foreach (Choice nodeChoice in node.Choices)
                {

                    string intentName = nodeChoice.IntentName;
                    try
                    {

                        if (string.IsNullOrWhiteSpace(intentName))
                        {
                            // get the intent by id

                            // check if the title contains the intent
                            var addIntent = intentLib?.FirstOrDefault(x =>
                                x.Name.Equals(nodeChoice.IntentName, StringComparison.OrdinalIgnoreCase));

                            if (addIntent != null)
                            {
                                AddIfNewIntent(mappedIntents, addIntent, locale);
                            }
                            else
                            {
                                AddIfNewIntent(mappedIntents, new Intent(nodeChoice.IntentName), locale);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Error creating Alexa IntentModel for intent {0}", intentName), ex);
                    }
                }
            }
        }


        private static void AddIfNewIntent(List<IntentModel> mappedIntents, Intent libIntent, string locale)
        {

            var (Name, IsAmazonIntent) = MapToAmazonIntent(libIntent.Name);
            Intent saveIntent = null;
            if (IsAmazonIntent)
            {
                saveIntent = new Intent(Name);
            }
            else
                saveIntent = libIntent;


            if (!mappedIntents.Exists(x => x.Name.Equals(saveIntent.Name)))
            {
                IntentModel newIntent = CreateIntentModel(saveIntent, locale);
                mappedIntents.Add(newIntent);
            }
        }


        private static IntentModel CreateIntentModel(Intent intent, string locale)
        {


            IntentModel mappedIntent = new IntentModel();
            try
            {


                mappedIntent.Name = intent.Name;
                LocalizedIntent localIntent = intent.LocalizedIntents.GetLocalizeText(locale);

                mappedIntent.Samples = localIntent?.Utterances;

                if (intent.SlotMappingsByName != null && intent.SlotMappingsByName.Count > 0)
                {
                    mappedIntent.Slots = new List<SlotModel>();
                    foreach (var slot in intent.SlotMappingsByName)
                    {
                        SlotModel slotMod = new SlotModel
                        {
                            Name = slot.Key
                        };

                        var (SlotTypeName, IsAmazonIntent) = MapToAmazonSlot(slot.Value);
                        slotMod.Type = SlotTypeName;
                        mappedIntent.Slots.Add(slotMod);
                    }
                }
            }
            catch (Exception ex)
            {

                throw new Exception(string.Format("Error creating Alexa IntentModel for intent {0}", intent.Name), ex);

            }

            return mappedIntent;
        }


        private static (string SlotTypeName, bool IsAmazonIntent) MapToAmazonSlot(string slotTypeName)
        {

            if (!string.IsNullOrWhiteSpace(slotTypeName))
            {
                if (slotTypeName.Equals(WhetstoneIntents.US_CITY_INTENT, StringComparison.OrdinalIgnoreCase))
                    return ("AMAZON.US_CITY", true);
                else if (slotTypeName.Equals(WhetstoneIntents.US_PHONENUMBER_INTENT, StringComparison.OrdinalIgnoreCase))
                    return ("AMAZON.PhoneNumber", true);
                else if (slotTypeName.Equals(WhetstoneIntents.FOUR_DIGIT_NUMBER_INTENT, StringComparison.OrdinalIgnoreCase))
                    return ("AMAZON.FOUR_DIGIT_NUMBER", true);
                else if (slotTypeName.Equals(WhetstoneIntents.NUMBER_INTENT, StringComparison.OrdinalIgnoreCase))
                    return ("AMAZON.NUMBER", true);

            }


            return (slotTypeName, false);
        }

        private static (string Name, bool IsAmazonIntent) MapToAmazonIntent(string intentName)
        {

            string amazonIntent = IntentNameMap.GetAmazonIntent(intentName);

            if (!string.IsNullOrWhiteSpace(amazonIntent))
            {
                return (amazonIntent, true);
            }

            return (intentName, false);

        }
    }
}
