using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Conditions;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Story.Cards;
using Whetstone.StoryEngine.Models.Story.Ssml;
using Whetstone.StoryEngine.Models.Story.Text;
using Whetstone.StoryEngine.Models.Tracking;

namespace Whetstone.StoryEngine.Data
{
    public static class StoryExtensions
    {

        /// <summary>
        /// Retrieves a list of choices trimmed by conditions.
        /// </summary>
        /// <param name="originalChoices"></param>
        /// <returns></returns>
        public static async Task<List<Choice>> GetAvailableChoicesAsync(this IEnumerable<Choice> originalChoices, ConditionInfo conditionInfo, ITitleReader reader, TitleVersion titleVersion)
        {
            List<Choice> availableChoices = new List<Choice>();

            if (originalChoices != null)
            {
                foreach (Choice origChoice in originalChoices)
                {

                    if (origChoice.ConditionNames != null && origChoice.ConditionNames.Count > 0)
                    {
                        bool isConditionMet = await origChoice.ConditionNames.IsConditionMetAsync(conditionInfo, reader, titleVersion);

                        if (isConditionMet)
                            availableChoices.Add(origChoice);
                    }
                    else
                    {
                        availableChoices.Add(origChoice);
                    }
                }

            }

            return availableChoices;
        }


        public static async Task<bool> IsConditionMetAsync(this List<string> conditions, ConditionInfo conditionInfo, ITitleReader titleReader, TitleVersion titleVersion)
        {
            bool isConditionMet = true;
            int conditionIndex = 0;

            if (conditionInfo == null)
                return false;

            while (isConditionMet && conditionIndex < conditions.Count)
            {
                string conditionName = conditions[conditionIndex];


                var foundCondition = await titleReader.GetStoryConditionAsync(titleVersion, conditionName);


                if (foundCondition != null)
                {
                    isConditionMet = foundCondition.IsStoryCondition(conditionInfo);
                }

                if (isConditionMet)
                    conditionIndex++;
            }

            return isConditionMet;
        }

        public static List<string> GetSuggestions(this StoryNode node, string locale, ConditionInfo conditionInfo, List<StoryConditionBase> storyConditions)
        {
            List<string> suggestions = new List<string>();

            if ((node.Choices?.Any()).GetValueOrDefault(false))
            {
                suggestions.AddRange(node.Choices.GetSuggestions(locale, conditionInfo, storyConditions));
            }

            if ((node.LocalizedSuggestions?.Any()).GetValueOrDefault(false))
            {
                foreach (var locSuggestion in node.LocalizedSuggestions)
                {
                    var locText = locSuggestion.GetLocalizedPlainText(locale);
                    suggestions.Add(locText);
                }
            }
            return suggestions;
        }


        public static List<string> GetSuggestions(this List<Choice> nodeChoices, string locale, ConditionInfo conditionInfo, List<StoryConditionBase> storyConditions)
        {
            List<string> suggestions = new List<string>();
            if (nodeChoices != null)
            {
                foreach (var choice in nodeChoices)
                {
                    bool isConditionMet;
                    if ((choice.ConditionNames?.Any()).GetValueOrDefault(false))
                        isConditionMet = IsConditionMet(choice.ConditionNames, conditionInfo, storyConditions);
                    else
                        isConditionMet = true;


                    if (isConditionMet)
                    {
                        List<string> foundSuggestions = choice.NodeMapping.GetSuggestions(locale, conditionInfo, storyConditions);

                        if ((foundSuggestions?.Any()).GetValueOrDefault(false))
                            suggestions.AddRange(foundSuggestions);
                    }
                }
            }

            return suggestions;
        }


        public static List<string> GetSuggestions(this NodeMappingBase nodeBase, string locale, ConditionInfo conditionInfo, List<StoryConditionBase> storyConditions)
        {
            List<string> suggestTexts = new List<string>();

            if (nodeBase != null)
            {

                bool isConditionMet;
                if ((nodeBase.Conditions?.Any()).GetValueOrDefault(false))
                    isConditionMet = IsConditionMet(nodeBase.Conditions, conditionInfo, storyConditions);
                else
                    isConditionMet = true;

                if (nodeBase.GetType() == typeof(SlotMap) && isConditionMet)
                {
                    SlotMap map = (SlotMap)nodeBase;

                    foreach (var mapping in map.Mappings)
                    {
                        var listResult = GetSuggestions(mapping, locale, conditionInfo, storyConditions);
                        suggestTexts.AddRange(listResult);
                    }

                }
                else if (nodeBase.GetType() == typeof(MultiNodeMapping) && isConditionMet)
                {
                    MultiNodeMapping multiNodeMap = (MultiNodeMapping)nodeBase;

                    string locText = multiNodeMap.LocalizedSuggestionText?.GetLocalizedPlainText(locale);
                    if (!string.IsNullOrWhiteSpace(locText))
                        suggestTexts.Add(locText);
                }
                else if (nodeBase.GetType() == typeof(ConditionalNodeMapping))
                {
                    ConditionalNodeMapping condMap = (ConditionalNodeMapping)nodeBase;

                    if (isConditionMet)
                        suggestTexts.AddRange(GetSuggestions(condMap.TrueConditionResult, locale, conditionInfo, storyConditions));
                    else
                        suggestTexts.AddRange(GetSuggestions(condMap.FalseConditionResult, locale, conditionInfo, storyConditions));

                }
                else if (nodeBase.GetType() == typeof(SlotNodeMapping) && isConditionMet)
                {
                    SlotNodeMapping slotNodeMap = (SlotNodeMapping)nodeBase;

                    suggestTexts.AddRange(GetSuggestions(slotNodeMap.NodeMap, locale, conditionInfo, storyConditions));
                }
                else if (nodeBase.GetType() == typeof(SingleNodeMapping) && isConditionMet)
                {
                    SingleNodeMapping singleMap = (SingleNodeMapping)nodeBase;
                    string locText = singleMap.LocalizedSuggestionText?.GetLocalizedPlainText(locale);
                    if (!string.IsNullOrWhiteSpace(locText))
                        suggestTexts.Add(locText);
                }
            }

            return suggestTexts;

        }

        public static bool IsConditionMet(this List<string> conditions, ConditionInfo conditionInfo, List<StoryConditionBase> storyConditions)
        {
            bool isConditionMet = true;
            int conditionIndex = 0;

            if (conditionInfo == null)
                return false;

            while (isConditionMet && conditionIndex < conditions.Count)
            {
                string conditionName = conditions[conditionIndex];


                var foundCondition = storyConditions.FirstOrDefault(x => x.Name.Equals(conditionName, StringComparison.OrdinalIgnoreCase));


                if (foundCondition != null)
                {
                    isConditionMet = foundCondition.IsStoryCondition(conditionInfo);
                }

                if (isConditionMet)
                    conditionIndex++;
            }

            return isConditionMet;
        }


        public static ConditionInfo GetConditionInfo(this DataTitleClientUser curUser)
        {
            List<IStoryCrumb> crumbs = curUser.TitleState;
            List<IStoryCrumb> permCrumbs = curUser.PermanentTitleState;

            ConditionInfo conditionInfo = new ConditionInfo(crumbs, permCrumbs, curUser.Client);

            return conditionInfo;
        }






        public static async Task<StoryResponse> ToStoryResponseAsync(this StoryNode node, string userLocale, ConditionInfo conditionInfo, ITitleReader titleReader, TitleVersion titleVersion, bool continueSession, ILogger logger)
        {

            return await node.ToStoryResponseAsync(userLocale, conditionInfo, titleReader, titleVersion, continueSession, true, logger);
        }

        public static async Task<StoryResponse> ToStoryResponseAsync(this StoryNode node, string userLocale, ConditionInfo conditionInfo, ITitleReader titleReader, TitleVersion titleVersion, ILogger logger)
        {

            return await node.ToStoryResponseAsync(userLocale, conditionInfo, titleReader, titleVersion, false, true, logger);
        }


        public static async Task<StoryResponse> ToStoryResponseAsync(this StoryNode node, string userLocale, ConditionInfo conditionInfo, ITitleReader titleReader, TitleVersion titleVersion, bool continueSession, bool isRequestValid, ILogger logger)
        {
            StoryResponse result = new StoryResponse
            {
                TitleId = node.TitleId,
                TitleVersion = titleVersion.Version,
                LocalizedResponse = await node.GetResponseAsync(userLocale, conditionInfo, titleReader, titleVersion, logger),

                Choices = node.Choices,
                NodeName = node.Name,
                Actions = node.Actions,
                ForceContinueSession = continueSession,
                IsRequestValid = isRequestValid,
                AuditBehavior = node.AuditBehavior
            };

            var conditions = await titleReader.GetStoryConditionsAsync(titleVersion);
            result.Suggestions = node.GetSuggestions(userLocale, conditionInfo, conditions);
            return result;


        }

        /// <summary>
        /// Returns the built response of localize text. 
        /// </summary>
        /// <param name="locResponse"></param>
        /// <returns></returns>
        public static async Task<List<TextFragmentBase>> GetTextResponseAsync(this LocalizedResponse locResponse, ConditionInfo conditionInfo, ITitleReader titleReader, TitleVersion titleVersion, ILogger logger)
        {

            List<TextFragmentBase> locText = null;

            if (locResponse.TextFragments != null)
            {
                locText = await GetTextFragmentResponseAsync(locResponse.TextFragments, conditionInfo, titleReader, titleVersion, logger);
            }

            List<SelectedItem> selItems = new List<SelectedItem>();

            if ((conditionInfo.Crumbs?.Any()).GetValueOrDefault(false))
                selItems.AddRange(conditionInfo.Crumbs.GetSelectedItems());

            if ((locResponse.SpeechResponses?.Any()).GetValueOrDefault(false))
            {
                await ApplySpeechResponseMacros(locResponse.SpeechResponses, selItems, logger);
            }


            if ((locResponse.RepromptSpeechResponses?.Any()).GetValueOrDefault(false))
            {
                await ApplySpeechResponseMacros(locResponse.RepromptSpeechResponses, selItems, logger);
            }

            return locText;
        }

        private static async Task ApplySpeechResponseMacros(List<ClientSpeechFragments> clientFrags, List<SelectedItem> selItems, ILogger logger)
        {
            if ((clientFrags?.Any()).GetValueOrDefault(false))
            {
                foreach (var speechResp in clientFrags)
                {
                    if ((speechResp?.SpeechFragments?.Any()).GetValueOrDefault(false))
                    {
                        foreach (SpeechFragment speechRespFrag in speechResp.SpeechFragments)
                        {
                            if (speechRespFrag is PlainTextSpeechFragment plainFrag)
                            {
                                plainFrag.Text = await MacroProcessing.ProcessTextFragmentMacrosAsync(plainFrag.Text, selItems, logger);
                            }
                            else if (speechRespFrag is SsmlSpeechFragment ssmlFrag)
                            {
                                ssmlFrag.Ssml = await MacroProcessing.ProcessTextFragmentMacrosAsync(ssmlFrag.Ssml, selItems, logger);
                            }

                        }
                    }
                }
            }
        }

        private static async Task<List<TextFragmentBase>> GetTextFragmentResponseAsync(List<TextFragmentBase> textFrags, ConditionInfo conditionInfo, ITitleReader titleReader, TitleVersion titleVersion, ILogger logger)
        {
            List<TextFragmentBase> retValues = new List<TextFragmentBase>();


            if (textFrags != null)
            {
                foreach (TextFragmentBase textBase in textFrags)
                {

                    if (textBase is SimpleTextFragment simpleFrag)
                    {
                        List<SelectedItem> selectedItems = conditionInfo.Crumbs.GetSelectedItems();

                        string processedText = await MacroProcessing.ProcessTextFragmentMacrosAsync(simpleFrag.Text, selectedItems, logger);

                        retValues.Add(new SimpleTextFragment(processedText));

                    }

                    if (textBase is AudioTextFragment audioFrag)
                    {
                        retValues.Add(audioFrag);
                    }

                    if (textBase is ConditionalTextFragment condFrag)
                    {
                        bool isMet = await condFrag.Conditions.IsConditionMetAsync(conditionInfo, titleReader, titleVersion);

                        if (isMet)
                        {


                            retValues.AddRange(await GetTextFragmentResponseAsync(condFrag.TrueResultFragments, conditionInfo,
                                titleReader, titleVersion, logger));
                        }
                        else
                        {
                            retValues.AddRange(await GetTextFragmentResponseAsync(condFrag.FalseResultFragments, conditionInfo,
                                titleReader, titleVersion, logger));
                        }


                    }
                }


            }

            return retValues;
        }

        public static async Task<LocalizedEngineResponse> GetResponseAsync(this StoryNode node, string userLocale, ConditionInfo conditionInfo, ITitleReader titleReader, TitleVersion titleVersion, ILogger logger)
        {
            LocalizedEngineResponse returnResponse = null;
            LocalizedResponseSet selectedResponses = null;


            // Get available response sets.
            if ((node.ResponseSet?.Any()).GetValueOrDefault(false))
            {

                bool isPrivacyLoggingEnabled = await titleReader.IsPrivacyLoggingEnabledAsync(titleVersion);

                List<LocalizedResponseSet> userResponseSets = new List<LocalizedResponseSet>();

                string logNodeName = isPrivacyLoggingEnabled ? "(redacted)" : node.Name;

                foreach (var respSet in node.ResponseSet)
                {

                    if (respSet == null)
                    {

                        logger.LogWarning($"Node {logNodeName} has unexpected formatting. Found empty response set. Review node format");
                    }
                    else
                    {

                        LocalizedResponseSet locSet = new LocalizedResponseSet
                        {
                            LocalizedResponses = new List<LocalizedResponse>()
                        };

                        if (respSet.LocalizedResponses == null)
                        {
                            logger.LogWarning($"Node {logNodeName} has unexpected formatting. Found localized responses. Review node format");

                        }
                        else
                        {
                            // Find localized responses that match the user's locale
                            var locResponses = respSet.LocalizedResponses.Where(x => !string.IsNullOrWhiteSpace(x.Locale) &&
                                                            x.Locale.Equals(userLocale, StringComparison.OrdinalIgnoreCase)).ToList();


                            // If matching localized responses were found, then use those.
                            if (locResponses.Any())
                            {
                                locSet.LocalizedResponses.AddRange(locResponses);
                            }
                            else
                            {
                                // if a matching localized response was not found, then use the defaults.

                                locResponses = respSet.LocalizedResponses.Where(x => string.IsNullOrWhiteSpace(x.Locale)).ToList();
                                locSet.LocalizedResponses.AddRange(locResponses);

                            }

                            userResponseSets.Add(locSet);
                        }
                    }
                }

                switch (node.ResponseBehavior)
                {
                    case ResponseBehavior.SelectFirst:
                        selectedResponses = userResponseSets.FirstOrDefault();

                        break;
                    case ResponseBehavior.Random:
                        int index = GetRandomIndex(userResponseSets.Count);
                        selectedResponses = userResponseSets[index];
                        break;
                }


                LocalizedResponse storyResponse = selectedResponses?.LocalizedResponses.FirstOrDefault();
                returnResponse = new LocalizedEngineResponse();

                // Remove any responses that do not apply to the selected client.

                if ((storyResponse.SpeechResponses?.Any()).GetValueOrDefault(false))
                {
                    // Is there a speech response for the client
                    var clientResp = storyResponse.SpeechResponses.FirstOrDefault(x => x.SpeechClient.GetValueOrDefault(Client.Unknown) == conditionInfo.UserClient);

                    if (clientResp == null)
                    {
                        // If there is no client-specific speech response, then select the default.
                        clientResp = storyResponse.SpeechResponses.FirstOrDefault(x => x.SpeechClient.GetValueOrDefault(Client.Unknown) == Client.Unknown);

                        if (clientResp == null)
                            logger.LogWarning($"No speech response available for client {conditionInfo.UserClient} in node {logNodeName}. Please add a response for client {conditionInfo.UserClient}");
                    }

                    returnResponse.SpeechResponses = clientResp.SpeechFragments;
                }



                // Return only the reprompt response for the selected client.
                if ((storyResponse.RepromptSpeechResponses?.Any()).GetValueOrDefault(false))
                {
                    // Is there a speech response for the client
                    var clientResp = storyResponse.RepromptSpeechResponses.FirstOrDefault(x => x.SpeechClient.GetValueOrDefault(Client.Unknown) == conditionInfo.UserClient);

                    if (clientResp == null)
                    {
                        // If there is no client-specific speech response, then select the default.
                        clientResp = storyResponse.RepromptSpeechResponses.FirstOrDefault(x => x.SpeechClient.GetValueOrDefault(Client.Unknown) == Client.Unknown);

                        if (clientResp == null)
                            logger.LogWarning($"No speech reprompt response available for client {conditionInfo.UserClient} in node {logNodeName}. Please add a reprompt response for client {conditionInfo.UserClient}");


                    }


                    returnResponse.RepromptSpeechResponses = clientResp.SpeechFragments;
                }

                // The card response depends on the the generated text response. Make sure it's available

                returnResponse.GeneratedTextResponse =
                    await storyResponse.GetTextResponseAsync(conditionInfo, titleReader, titleVersion, logger);

                if (!string.IsNullOrWhiteSpace(storyResponse.RepromptTextResponse))
                {
                    returnResponse.RepromptTextResponses = new List<string>
                    {
                        storyResponse.RepromptTextResponse
                    };
                }


                // If we have a card response and the card responses list has values, then we should pull the appropriate
                // card response from there. Otherwise the response is using the old style and will already have the appropriate
                // values set.
                if (storyResponse.SendCardResponse.GetValueOrDefault(false))
                {

                    CardEngineResponse cardResp = null;
                    CardResponse storyCardResponse = null;


                    if ((storyResponse.CardResponses?.Any()).GetValueOrDefault(false))
                    {
                        // Is there a speech response for the client
                        storyCardResponse = storyResponse.CardResponses.FirstOrDefault(x => x.SpeechClient.GetValueOrDefault(Client.Unknown) == conditionInfo.UserClient);

                        if (storyCardResponse == null)
                        {
                            // If there is no client-specific card response, then select the default.
                            storyCardResponse = storyResponse.CardResponses.FirstOrDefault(x => x.SpeechClient.GetValueOrDefault(Client.Unknown) == Client.Unknown);
                        }
                    }

                    cardResp = new Models.Story.Cards.CardEngineResponse();
                    if (storyCardResponse == null)
                    {
                        // Build the card response.                       
                        cardResp.CardTitle = storyResponse.CardTitle;
                        cardResp.LargeImageFile = storyResponse.LargeImageFile;
                        cardResp.SmallImageFile = storyResponse.SmallImageFile;
                        cardResp.Text = returnResponse.GeneratedTextResponse.CleanTextList();
                    }
                    else
                    {
                        // Apply the defined card response.
                        cardResp.CardTitle = storyCardResponse.CardTitle;
                        cardResp.LargeImageFile = storyCardResponse.LargeImageFile;
                        cardResp.SmallImageFile = storyCardResponse.SmallImageFile;

                        var fragResponse = await GetTextFragmentResponseAsync(storyCardResponse.TextFragments,
                            conditionInfo, titleReader, titleVersion, logger);

                        cardResp.Text = fragResponse.CleanTextList();

                        cardResp.Buttons = storyCardResponse.Buttons;
                    }
                    returnResponse.CardResponse = cardResp;
                }



            }

            return returnResponse;
        }




        public static async Task<string> GetChoiceNodeNameAsync(this Choice selectedChoice, string intentName, ConditionInfo conditionInfo, Dictionary<string, string> slots, ITitleReader titleReader, TitleVersion titleVersion)
        {
            NodeMappingBase nodeMapper = selectedChoice.NodeMapping;

            return await GetNodeMapping(nodeMapper, intentName, conditionInfo, slots, titleReader, titleVersion);
        }

        private static async Task<string> GetNodeMapping(NodeMappingBase nodeMapper, string intentName, ConditionInfo conditionInfo, Dictionary<string, string> slots, ITitleReader titleReader, TitleVersion titleVersion)

        {
            if (nodeMapper == null)
                return null;

            if (nodeMapper is SingleNodeMapping)
            {
                SingleNodeMapping singleMapping = nodeMapper as SingleNodeMapping;

                if ((singleMapping.Conditions?.Any()).GetValueOrDefault(false))
                {
                    if (!(await singleMapping.Conditions.IsConditionMetAsync(conditionInfo, titleReader, titleVersion)))
                        return null;

                }

                return singleMapping.NodeName;
            }

            if (nodeMapper is MultiNodeMapping)
            {
                MultiNodeMapping multiNodeMapping = nodeMapper as MultiNodeMapping;

                if ((multiNodeMapping.Conditions?.Any()).GetValueOrDefault(false))
                {
                    if (!(await multiNodeMapping.Conditions.IsConditionMetAsync(conditionInfo, titleReader, titleVersion)))
                        return null;

                }

                List<string> multiNames = multiNodeMapping.NodeNames;

                if (multiNames == null || multiNames.Count == 0)
                    return null;


                int nodeIndex = GetRandomIndex(multiNames.Count - 1);

                return multiNames[nodeIndex];
            }

            if (nodeMapper is ConditionalNodeMapping)
            {
                ConditionalNodeMapping conditionalMapper = nodeMapper as ConditionalNodeMapping;


                List<string> conditions = conditionalMapper.Conditions;

                NodeMappingBase mappingBaseResult = null;
                if (await conditions.IsConditionMetAsync(conditionInfo, titleReader, titleVersion))
                {
                    mappingBaseResult = conditionalMapper.TrueConditionResult;
                }
                else
                {
                    mappingBaseResult = conditionalMapper.FalseConditionResult;
                }

                if (mappingBaseResult != null)
                    return await GetNodeMapping(mappingBaseResult, intentName, conditionInfo, slots, titleReader, titleVersion);
            }

            if (nodeMapper is SlotMap)
            {
                // If no slots are passed, there's no point in proceeding.
                if (slots == null || slots.Count == 0)
                    return null;

                SlotMap map = nodeMapper as SlotMap;

                StoryTitle title = await titleReader.GetByIdAsync(titleVersion);

                List<SlotNodeMapping> slotMappings = map.Mappings;

                //  Dictionary<string, List<string>> flattenedSlotsValues = new Dictionary<string, List<string>>();
                Intent parentIntent = title.Intents.FirstOrDefault(x =>
                    x.Name.Equals(intentName, StringComparison.OrdinalIgnoreCase));


                // Get the acutal slot value from the slot mapping
                Dictionary<string, string> resolvedSlotValues = new Dictionary<string, string>();

                foreach (string passedSlotKey in slots.Keys)
                {
                    string passedSlotValue = slots[passedSlotKey];

                    if (!string.IsNullOrWhiteSpace(passedSlotValue))
                    {
                        // only evaluate if the provided slot mapping is one that the choice node is looking for


                        var foundSlotMapping = parentIntent.SlotMappingsByName.FirstOrDefault(x =>
                            x.Key.Equals(passedSlotKey, StringComparison.OrdinalIgnoreCase) ||
                            x.Value.Equals(passedSlotKey, StringComparison.OrdinalIgnoreCase));

                        string matchedKey = foundSlotMapping.Key;

                        if (!string.IsNullOrWhiteSpace(matchedKey))
                        {
                            // Get the slot type
                            string typeName = parentIntent.SlotMappingsByName[matchedKey];

                            SlotType mappedType =
                                title.Slots.FirstOrDefault(x =>
                                    x.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));

                            if (mappedType != null)
                            {
                                // Get the parent slot value from the slot type

                                SlotValue slotVal = mappedType.Values.FirstOrDefault(x =>
                                    x.Value.Equals(passedSlotValue, StringComparison.OrdinalIgnoreCase));

                                if (slotVal != null)
                                    resolvedSlotValues.Add(matchedKey, slotVal.Value);
                                else
                                {
                                    // Look in the synonyms

                                    var foundItem = mappedType.Values.FirstOrDefault(x => x.Synonyms != null &&
                                                                                          x.Synonyms.Any(y =>
                                                                                              y.Equals(passedSlotValue,
                                                                                                  StringComparison
                                                                                                      .OrdinalIgnoreCase)));

                                    if (foundItem != null)
                                        resolvedSlotValues.Add(matchedKey, foundItem.Value);
                                }
                            }
                        }
                    }
                }

                string foundNode = null;
                int index = 0;


                while (foundNode == null && index < slotMappings.Count)
                {

                    SlotNodeMapping nodeMapping = slotMappings[index];
                    bool isConditionMet = true;


                    if ((nodeMapping?.Conditions?.Any()).GetValueOrDefault(false))
                    {
                        isConditionMet = await nodeMapping.Conditions.IsConditionMetAsync(conditionInfo, titleReader, titleVersion);
                    }

                    if (isConditionMet)
                    {
                        // Are the required slots supplied?
                        if (nodeMapping != null)
                        {

                            // all of the required values in the slot keys must match in order for the node map to be considered valid.
                            bool isFound = true;
                            int slotValIndex = 0;
                            List<string> reqSlotKeys = nodeMapping.RequiredSlotValues.Keys.ToList();

                            while (isFound && slotValIndex < reqSlotKeys.Count)
                            {

                                var slotKey = reqSlotKeys[slotValIndex];
                                List<string> permittedSlotVals = nodeMapping.RequiredSlotValues[slotKey];

                                if (resolvedSlotValues.ContainsKey(slotKey))
                                {
                                    string slotVal = resolvedSlotValues[slotKey];

                                    if (!permittedSlotVals.Any(x =>
                                        x.Equals(slotVal, StringComparison.OrdinalIgnoreCase)))
                                        isFound = false;
                                }
                                else
                                    isFound = false;


                                if (isFound)
                                    slotValIndex++;

                            }


                            if (isFound)
                            {
                                foundNode = await GetNodeMapping(nodeMapping.NodeMap, intentName, conditionInfo, slots,
                                    titleReader, titleVersion);

                                if (foundNode == null)
                                {
                                    // The node was found, but conditions prevent it from being used. Exit here.
                                    // force an exit
                                    index = slotMappings.Count;
                                }
                            }
                            else
                            {
                                index++;
                            }


                        }

                    }

                }

                return foundNode;


            }


            return null;
        }




        private static int GetRandomIndex(int upperIndex)
        {
            int pickedIndex = StaticRandom.Next(0, upperIndex + 1);

            return pickedIndex;
        }

    }
}
