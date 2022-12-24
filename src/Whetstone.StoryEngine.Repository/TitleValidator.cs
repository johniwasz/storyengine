using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Story.Cards;
using Whetstone.StoryEngine.Models.Story.Ssml;
using Whetstone.StoryEngine.Models.Tracking;

namespace Whetstone.StoryEngine.Repository
{
    [ExcludeFromCodeCoverage]
    public class TitleValidator : ITitleValidator
    {

        private ITitleReader _titleReader;
        private IFileRepository _fileRep;

        private readonly List<Client> RequiredClients = new List<Client> { Client.GoogleHome, Client.Alexa };

        public TitleValidator(ITitleReader titleReader, IFileRepository fileRep)
        {
            _titleReader = titleReader;
            _fileRep = fileRep;
        }


        public async Task<List<NodeMapItem>> GetNodeRouteAsync(TitleVersion titleVer, string sourceNode, string destNode)
        {
            List<NodeMapItem> nodeMap = await GetNodeMapAsync(titleVer);

            NodeMapItem startItem = nodeMap.FirstOrDefault(x => x.Name.Equals(sourceNode, StringComparison.OrdinalIgnoreCase));

            Tuple<Queue<string>, bool> pathQueue = FindPath(sourceNode, destNode, -1, nodeMap);

            foreach (string pathText in pathQueue.Item1)
                Debug.WriteLine(pathText);

            return null;

        }

        private Tuple<Queue<string>,bool> FindPath(string startNode, string destNode, int level, List<NodeMapItem> nodeMap)
        {
            level++;
            Queue<string> pathQueue = new Queue<string>();
            pathQueue.Enqueue(startNode);

            Debug.WriteLine("Level {0}, Node {1}", level, startNode);
            var curItem = nodeMap.FirstOrDefault(x => x.Name.Equals(startNode, StringComparison.OrdinalIgnoreCase));
            bool isFound = false;
            if ((curItem.Children?.Any()).GetValueOrDefault(false))
            {
                try
                {
                    int childIndex = 0;
                   
                    while (childIndex < curItem.Children.Count && !isFound)
                    {
                        NodeMapItem childItem = curItem.Children[childIndex];

                        if (childItem.Name.Equals(destNode, StringComparison.OrdinalIgnoreCase))
                        {
                            isFound = true;
                        }
                        else
                        {
                            Tuple<Queue<string>, bool> result = FindPath(childItem.Name, destNode, level, nodeMap);

                            isFound = result.Item2;
                            if (isFound)
                            {
                                IEnumerable<string> foundPath = result.Item1;
                                foreach (string pathFound in foundPath)
                                    pathQueue.Enqueue(pathFound);
                            }

                        }

                        if (!isFound)
                            childIndex++;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);

                }
            }
            else
                Debug.WriteLine("No children?");


            return new Tuple<Queue<string>, bool>(pathQueue, isFound);
        }

        public async Task<List<NodeMapItem>> GetNodeMapAsync(TitleVersion titleVer)
        {
            ICollection<StoryNode> colNodes = await _titleReader.GetNodesByTitleAsync(titleVer);
            List<StoryNode> nodes = new List<StoryNode>();

            nodes.AddRange(colNodes);

            List<NodeMapItem> nodeMap = new List<NodeMapItem>();

            if((nodes?.Any()).GetValueOrDefault(false))
            {
                StoryNode curNode;
                //StoryNode curNode = nodes[0];
                //var parentNodeMap = new NodeMapItem(curNode);
                //nodeMap.Add(parentNodeMap);
                //MapChoiceNodes(nodes, nodeMap, curNode, parentNodeMap);

                for (int nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++)
                {
                    curNode = nodes[nodeIndex];
                    MapChoiceNodes(nodes, nodeMap, curNode);
                }
            }
            
            return nodeMap;
        }

        private void MapChoiceNodes(List<StoryNode> nodes, List<NodeMapItem> nodeMap, StoryNode curNode)
        {

            NodeMapItem parentNodeMap = nodeMap.FirstOrDefault(x => x.Name.Equals(curNode.Name, StringComparison.OrdinalIgnoreCase));
            if(parentNodeMap==null)
            {
                parentNodeMap = new NodeMapItem(curNode);
                nodeMap.Add(parentNodeMap);
            }

            if ((curNode.Choices?.Any()).GetValueOrDefault(false))
            {
                foreach (Choice curChoice in curNode.Choices)
                {
                    //if (!curChoice.IntentName.Equals("GoBackIntent"))
                    //{
                        if (curChoice?.NodeMapping != null)
                        {
                            List<string> nodeNames = GetNodesFromMapping(curChoice.NodeMapping);
                            if ((nodeNames?.Any()).GetValueOrDefault(false))
                            {

                                IEnumerable<string> uniqueNames = nodeNames.Distinct();
                                foreach (string nodeName in uniqueNames)
                                {


                                    NodeMapItem nodeItem = nodeMap.FirstOrDefault(x => x.Name.Equals(nodeName, StringComparison.OrdinalIgnoreCase));

                                    if (nodeItem == null)
                                    {
                                        // add item
                                        StoryNode foundNode = nodes.FirstOrDefault(x => x.Name.Equals(nodeName, StringComparison.OrdinalIgnoreCase));

                                        nodeItem = parentNodeMap.AddChild(foundNode, curChoice);

                                        nodeMap.Add(nodeItem);
                                    }
                                    else
                                    {
                                        StoryNode foundNode = nodes.FirstOrDefault(x => x.Name.Equals(nodeItem.Name, StringComparison.OrdinalIgnoreCase));

                                        parentNodeMap.AddChild(foundNode, curChoice);
                                    }

                                }
                            }
//                        }
                    }
                }

            }
        }

        private List<string> GetNodesFromMapping(NodeMappingBase mapping)
        {
            List<string> nodeNames = new List<string>();

            if(mapping != null)
            {
                //            [MessagePack.Union(0, typeof(SingleNodeMapping))]
                //[MessagePack.Union(1, typeof(MultiNodeMapping))]
                //[MessagePack.Union(2, typeof(ConditionalNodeMapping))]
                //[MessagePack.Union(3, typeof(SlotNodeMapping))]
                //[MessagePack.Union(4, typeof(SlotMap))]



                if(mapping is SingleNodeMapping)
                {
                    SingleNodeMapping singleMapping = (SingleNodeMapping)mapping;
                    if(!string.IsNullOrWhiteSpace(singleMapping.NodeName))
                        nodeNames.Add(singleMapping.NodeName);
                }
                else if(mapping is MultiNodeMapping)
                {
                    MultiNodeMapping multiMapping = (MultiNodeMapping)mapping;
                    if((multiMapping.NodeNames?.Any()).GetValueOrDefault(false))
                    {
                        foreach(string nodeName in multiMapping.NodeNames)
                        {
                            if (!string.IsNullOrWhiteSpace(nodeName))
                                nodeNames.Add(nodeName);
                        }

                    }
                }
                else if(mapping is SlotMap)
                {
                    SlotMap slotMaps = (SlotMap)mapping;

                   if((slotMaps?.Mappings?.Any()).GetValueOrDefault(false))
                    {
                        foreach(SlotNodeMapping slotMapping in slotMaps.Mappings)
                        {
                            List<string> slotNodeNames = GetNodesFromMapping(slotMapping.NodeMap);
                            if((slotNodeNames?.Any()).GetValueOrDefault(false))
                            {
                                foreach (string nodeName in slotNodeNames)
                                {
                                    if (!string.IsNullOrWhiteSpace(nodeName))
                                        nodeNames.Add(nodeName);
                                }

                            }

                        }

                    }


                }
                else if (mapping is SlotNodeMapping)
                {
                    SlotNodeMapping nodeMap = (SlotNodeMapping)mapping;


                   List<string> nodeNameMappings =  GetNodesFromMapping(nodeMap.NodeMap);

                    if((nodeNameMappings?.Any()).GetValueOrDefault(false))
                    {
                        foreach(string nodeName in nodeNameMappings)
                        {
                            if (!string.IsNullOrWhiteSpace(nodeName))
                                nodeNames.Add(nodeName);
                        }
                    }
                }
                else if(mapping is ConditionalNodeMapping)
                {
                    ConditionalNodeMapping condMapping = (ConditionalNodeMapping)mapping;

                    if(condMapping.FalseConditionResult!=null)
                    {
                        List<string> falseNames = GetNodesFromMapping(condMapping.FalseConditionResult);


                        if ((falseNames?.Any()).GetValueOrDefault(false))
                        {
                            foreach (string nodeName in falseNames)
                            {
                                if (!string.IsNullOrWhiteSpace(nodeName))
                                    nodeNames.Add(nodeName);
                            }
                        }
                    }

                    if(condMapping.TrueConditionResult!=null)
                    {

                        List<string> trueNames = GetNodesFromMapping(condMapping.TrueConditionResult);


                        if ((trueNames?.Any()).GetValueOrDefault(false))
                        {
                            foreach (string nodeName in trueNames)
                            {
                                if (!string.IsNullOrWhiteSpace(nodeName))
                                    nodeNames.Add(nodeName);
                            }
                        }
                    }

                }
            }



            return nodeNames;
        }

        public async Task<StoryValidationResult> ValidateTitleAsync(TitleVersion titleVersion)
        {
            StoryValidationResult valResult = new StoryValidationResult();
            valResult.NodeIssues = new List<NodeValdiationResult>();
           ICollection<StoryNode> nodes = await _titleReader.GetNodesByTitleAsync( titleVersion);


            List<Intent> intents = await _titleReader.GetIntentsAsync(titleVersion);

            List<SlotType> slots = await _titleReader.GetSlotTypes(titleVersion);

            List<Models.Conditions.StoryConditionBase>  conditions = await _titleReader.GetStoryConditionsAsync(titleVersion);


            List<string> audioFiles = await _fileRep.GetAudioFileListAsync(titleVersion);

            List<FileCounter> audioFileCount = new List<FileCounter>();

            foreach(string audioFile in audioFiles)
            {
                audioFileCount.Add(new FileCounter(audioFile));
            }


            if(nodes!=null)
                foreach(StoryNode node in nodes)
                {
                    List<string> nodeValResult = await ValidateNodeAsync(titleVersion, node, nodes, audioFileCount, intents, slots, conditions);

                    if((nodeValResult?.Any()).GetValueOrDefault(false))
                    {
                        NodeValdiationResult nodeResult = new NodeValdiationResult();
                        nodeResult.NodeName = node.Name;
                        nodeResult.Messages = nodeValResult;

                        valResult.NodeIssues.Add(nodeResult);
                    }


                }

            valResult.UnusedAudioFiles = new List<string>();
            foreach (var fileCounter in audioFileCount)
            {
                if(fileCounter.UseCount ==0)
                {
                   
                    valResult.UnusedAudioFiles.Add(fileCounter.FileName);
                    Debug.WriteLine(fileCounter.FileName);
                }

            }

            

            return valResult;
        }

        public async Task<StoryValidationResult> ValidateTitleAsync( StoryTitle title)
        {

            StoryValidationResult valResult = new StoryValidationResult();
            valResult.NodeIssues = new List<NodeValdiationResult>();
            ICollection<StoryNode> nodes = title.Nodes;


            List<Intent> intents = title.Intents;

            List<SlotType> slots = title.Slots;

            List<Models.Conditions.StoryConditionBase> conditions = title.Conditions;

            TitleVersion titleVer = new TitleVersion(title.Id, title.Version);

            List<string> audioFiles = await _fileRep.GetAudioFileListAsync( titleVer);

            List<FileCounter> audioFileCount = new List<FileCounter>();

            foreach (string audioFile in audioFiles)
            {
                audioFileCount.Add(new FileCounter(audioFile));
            }


            if (nodes != null)
                foreach (StoryNode node in nodes)
                {
                    List<string> nodeValResult = await ValidateNodeAsync(titleVer, node, nodes, audioFileCount, intents, slots, conditions);

                    if ((nodeValResult?.Any()).GetValueOrDefault(false))
                    {
                        NodeValdiationResult nodeResult = new NodeValdiationResult();
                        nodeResult.NodeName = node.Name;
                        nodeResult.Messages = nodeValResult;

                        valResult.NodeIssues.Add(nodeResult);
                    }


                }

            valResult.UnusedAudioFiles = new List<string>();
            foreach (var fileCounter in audioFileCount)
            {
                if (fileCounter.UseCount == 0)
                {

                    valResult.UnusedAudioFiles.Add(fileCounter.FileName);
                    Debug.WriteLine(fileCounter.FileName);
                }

            }



            return valResult;
        }



        private async  Task<List<string>> ValidateNodeAsync(TitleVersion titleVersion, StoryNode node, 
            ICollection<StoryNode> allNodes, 
            List<FileCounter> audioFileCount, 
            List<Intent> intents, 
            List<SlotType> slots, 
            List<Models.Conditions.StoryConditionBase> conditions)
        {

            List<string> valResult = new List<string>();

            if((node.ResponseSet?.Any()).GetValueOrDefault(false)==false)
            {
                valResult.Add("Missing ResponseSet");
            }
            else
            {
                int respSetCount = node.ResponseSet.Count;

                for (int responseSetIndex = 0; responseSetIndex < node.ResponseSet.Count; responseSetIndex++)
                {
                    LocalizedResponseSet respSet = node.ResponseSet[responseSetIndex];
                    
                  
                   
                    if((respSet.LocalizedResponses?.Any()).GetValueOrDefault(false)== false)
                    {
                        valResult.Add($"ResponseSet index {responseSetIndex} is missing localized responses");
                    }
                    else
                    {
                       for (int locIndex =0; locIndex < respSet.LocalizedResponses.Count; locIndex++)
                        {
                            LocalizedResponse locResp = respSet.LocalizedResponses[locIndex];


                            if(locResp.SendCardResponse.GetValueOrDefault(false))
                            {
                                List<string> cardValIssues = await ValidateCardNodeAsync(titleVersion, responseSetIndex, locIndex, locResp);
                                if( (cardValIssues?.Any()).GetValueOrDefault(false))
                                {
                                    valResult.AddRange(cardValIssues);
                                }
                            }

                            


                            if(!(locResp.RepromptTextResponse?.Any()).GetValueOrDefault(false) && !(locResp.RepromptSpeechResponses?.Any()).GetValueOrDefault(false))
                            {
                                valResult.Add($"ResponseSet index {responseSetIndex} and localized response index {locIndex} has no reprompts");
                            }
                            else
                            {
                                if((locResp.RepromptSpeechResponses?.Any()).GetValueOrDefault(false))
                                {

                                    

                                    List<string> valIssues = await ValidateClientSpeechResponsesAsync(titleVersion, responseSetIndex, locIndex, locResp.RepromptSpeechResponses, audioFileCount, conditions);
                                    if ((valIssues?.Any()).GetValueOrDefault(false))
                                    {
                                        foreach (string valIssue in valIssues)
                                        {
                                            valResult.Add($"Client reprompt speech response issue: {valIssue}");
                                        }
                                    }

                                }

                            }


                            // If there are no responses on the current node, then report an invalid condition.
                            if (!(locResp.SpeechResponses?.Any()).GetValueOrDefault(false) && !(locResp.TextFragments?.Any()).GetValueOrDefault(false))
                            {
                                valResult.Add(string.Format($"ResponseSet index {responseSetIndex} and localized response index {locIndex} has no responses"));
                            }
                            else
                            {

                                bool hasDefaultResponse = false;
                                // Determine if there is a default speech or text response
                                if ((locResp.SpeechResponses?.Any()).GetValueOrDefault(false))
                                {
                                    foreach (var speechResponse in locResp.SpeechResponses)
                                    {
                                        if (!speechResponse.SpeechClient.HasValue)
                                        {
                                            if ((speechResponse.SpeechFragments?.Any()).GetValueOrDefault(false))
                                            {
                                                hasDefaultResponse = true;
                                            }
                                        }
                                    }
                                }


                                if (!hasDefaultResponse)
                                {
                                    if ((locResp.TextFragments?.Any()).GetValueOrDefault(false))
                                        hasDefaultResponse = true;

                                }

                                if(!hasDefaultResponse)
                                {
                                    valResult.Add($"ResponseSet index {responseSetIndex} and localized response index {locIndex} has no default client response");

                                }


                                List<string> valIssues = await ValidateClientSpeechResponsesAsync(titleVersion, responseSetIndex, locIndex, locResp.SpeechResponses, audioFileCount, conditions);
                                if ((valIssues?.Any()).GetValueOrDefault(false))
                                {
                                    foreach(string valIssue in valIssues)
                                    {
                                        valResult.Add($"Client speech response issue: {valIssue}");
                                    }
                                }
                            }

                        }
                    }
                }
            }

            if((node.Actions?.Any()).GetValueOrDefault(false))
            {

                for(int actionIndex = 0; actionIndex < node.Actions.Count; actionIndex++)
                {
                    NodeActionData baseAction = node.Actions[actionIndex];

                    if(baseAction==null)
                    {
                        valResult.Add(string.Format("Node action index {0} is null", actionIndex));
                    }
                    else
                    {
                        //[MessagePack.Union(0, typeof(InventoryAction))]
                        // [MessagePack.Union(1, typeof(NodeVisitRecordAction))]
                        //[MessagePack.Union(2, typeof(RecordSelectedItemAction))]
                        //[MessagePack.Union(3, typeof(PhoneMessageAction))]
                        if (baseAction is RecordSelectedItemActionData)
                        {
                            RecordSelectedItemActionData selectedItemAction = (RecordSelectedItemActionData)baseAction;

                            if ((selectedItemAction.SlotNames?.Any()).GetValueOrDefault(false) == false)
                            {
                                valResult.Add($"Node action index {actionIndex} is a record item action with no slot names");
                            }
                            else
                            {

                                for (int slotNameIndex = 0; slotNameIndex < selectedItemAction.SlotNames.Count; slotNameIndex++)
                                {
                                    string slotName = selectedItemAction.SlotNames[slotNameIndex];
                                    if (string.IsNullOrWhiteSpace(slotName))
                                    {
                                        valResult.Add($"Node action index {actionIndex} is a record item action with blank slot names at index {slotNameIndex}");
                                    }
                                    else
                                    {
                                        // TODO Validate the slot name
                                    }
                                }
                            }
                        }
                        else if (baseAction is InventoryActionData)
                        {
                            InventoryActionData invAction = (InventoryActionData)baseAction;

                            if (invAction.Item == null)
                            {
                                valResult.Add($"Node action index {actionIndex} is an inventory action with inventory items");
                            }
                            else
                            {
                                //[MessagePack.Union(0, typeof(UniqueItem))]
                                //[MessagePack.Union(1, typeof(MultiItem))]

                                if (invAction.Item is UniqueItem)
                                {
                                    UniqueItem uniItem = (UniqueItem)invAction.Item;

                                    if (string.IsNullOrWhiteSpace(uniItem.Name))
                                    {
                                        valResult.Add($"Node action index {actionIndex} is an inventory action with missing unique item name");
                                    }
                                    else
                                    {
                                        // TODO Validate unique item name

                                    }

                                }
                                else if(invAction.Item is MultiItem)
                                {
                                    MultiItem multiItem = (MultiItem)invAction.Item;
                                   
                                    if(string.IsNullOrWhiteSpace(multiItem.Name))
                                    {
                                        valResult.Add($"Node action index {actionIndex} is an inventory action with missing multi item name");
                                    }
                                    else
                                    {
                                        // TODO Validate multi item name

                                    }

                                }
                            }

                        }
                    }
                }
            }


            if((node.Choices?.Any()).GetValueOrDefault(false))
            {

                // if choices exist, the make sure the destination node exists
                for(int choiceIndex =0; choiceIndex < node.Choices.Count; choiceIndex++)
                {

                    var curChoice = node.Choices[choiceIndex];
                    Intent foundIntent = null;

                    if (string.IsNullOrWhiteSpace(curChoice.IntentName))
                    {
                        valResult.Add($"Choice index {choiceIndex} is missing an intent mapping.");
                    }
                    else
                    {
                        foundIntent = intents.FirstOrDefault(x => x.Name.Equals(curChoice.IntentName, StringComparison.OrdinalIgnoreCase));

                       


                        if(foundIntent==null)
                        {
                            // is the intent a system intent.


                            
                            valResult.Add($"Choice index {choiceIndex} with intent {curChoice.IntentName} does not resolve to a valid intent.");

                        }
                    }


                    if((curChoice.ConditionNames?.Any()).GetValueOrDefault(false))
                    {
                        if(node.Choices.Count==1)
                            valResult.Add($"Choice index {choiceIndex} has conditions and is the only choice and it has a condition. If the condition is false, then the user will be stuck.");

                        // Validate that condition names exist
                        for (int condIndex = 0; condIndex < curChoice.ConditionNames.Count; condIndex++)
                        {
                            string choiceCondName = curChoice.ConditionNames[condIndex];
                            if(string.IsNullOrWhiteSpace(choiceCondName))
                            {
                                valResult.Add($"Choice index {choiceIndex} condition index {condIndex} is blank");
                            }

                            if ((conditions?.Any()).GetValueOrDefault(false) == false)
                            {
                                valResult.Add($"Choice index {choiceIndex} references condition '{choiceCondName}' on condition index {condIndex} but no conditions have been configured");

                            }
                            else
                            {
                                var foundCondition = conditions.FirstOrDefault(x => x.Name.Equals(choiceCondName, StringComparison.OrdinalIgnoreCase));
                                if(foundCondition==null)
                                {
                                    valResult.Add($"Choice index {choiceIndex} references condition '{choiceCondName}' on condition index {condIndex} but the condition is not defined");
                                }
                            }
                        }
                    }

                    if(curChoice.NodeMapping==null)
                    {
                        string nodeIssue = $"Choice index {choiceIndex} is missing a node mapping.";
                        valResult.Add(nodeIssue);
                    }
                    else
                    {

                        if(curChoice.NodeMapping is SingleNodeMapping)
                        {
                            SingleNodeMapping singleMapping = (SingleNodeMapping)curChoice.NodeMapping;

                            if(string.IsNullOrWhiteSpace(singleMapping.NodeName))
                            {
                                valResult.Add($"Choice index {choiceIndex} single node mapping is missing a node name");
                            }
                            else
                            {
                                string destNode = singleMapping.NodeName;
                                bool doesExist = DoesNodeExist(destNode, allNodes);
                                if (!doesExist)
                                    valResult.Add($"Choice index {choiceIndex} single node mapping is pointing to a missing node {destNode}");
                            }

                        }
                        else if(curChoice.NodeMapping is MultiNodeMapping)
                        {
                            MultiNodeMapping multiMapping = (MultiNodeMapping)curChoice.NodeMapping;

                            if(( multiMapping.NodeNames?.Any()).GetValueOrDefault(false)==false)
                            {
                                valResult.Add($"Choice index {choiceIndex} multi node mapping is missing node names");
                            }
                            else
                            {
                                foreach(string nodeName in multiMapping.NodeNames)
                                {
                                    if (string.IsNullOrWhiteSpace(nodeName))
                                    {
                                        valResult.Add($"Choice index {choiceIndex} multi node name is empty or null");
                                    }
                                    else
                                    {
                                        bool doesExist = DoesNodeExist(nodeName, allNodes);
                                        if(!doesExist)
                                            valResult.Add($"Choice index {choiceIndex} multi node mapping is pointing to a missing node {nodeName}");
                                    }
                                }
                            }
                        }
                        else if(curChoice.NodeMapping is SlotNodeMapping)
                        {
                            SlotNodeMapping slotMapping = (SlotNodeMapping)curChoice.NodeMapping;

                        }
                        else if(curChoice.NodeMapping is ConditionalNodeMapping)
                        {
                            ConditionalNodeMapping conditionalMapping = (ConditionalNodeMapping)curChoice.NodeMapping;


                            if( (conditionalMapping.Conditions?.Any()).GetValueOrDefault(false)==false)
                            {
                                valResult.Add($"Choice index {choiceIndex} conditional mapping has missing conditionals");
                            }
                            else
                            {
                                foreach (string conditionName in conditionalMapping.Conditions)
                                {

                                    var foundCondition = conditions.FirstOrDefault(x => x.Name.Equals(conditionName));
                                    if (foundCondition == null)
                                    {
                                        // the referenced condition is not found.
                                        valResult.Add($"Choice index {choiceIndex} conditional {conditionName} does not map to a defined condition");
                                    }

                                }
                            }
                        }
                        else if(curChoice.NodeMapping is SlotMap)
                        {
                            string parentIntent = curChoice.IntentName;

                            SlotMap slotMap = (SlotMap)curChoice.NodeMapping;


                            if((slotMap.Mappings?.Any()).GetValueOrDefault(false)==false)
                            {
                                valResult.Add($"Choice index {choiceIndex} slot mapping is missing slot mappings");
                            }
                            else
                            {
                                for(int mapIndex =0; mapIndex < slotMap.Mappings.Count; mapIndex++)
                                {
                                    SlotNodeMapping slotMapping = slotMap.Mappings[mapIndex];

                                    if(slotMapping.NodeMap==null)
                                    {
                                        valResult.Add($"Choice index {choiceIndex} slot mapping {mapIndex} is missing a node map");
                                    }
                                    else
                                    {
                                        // TODO recursively walk the node map
                                    }


                                    if((slotMapping.RequiredSlotValues?.Any()).GetValueOrDefault(false)==false)
                                    {
                                        valResult.Add($"Choice index {choiceIndex} slot mapping {mapIndex} is missing required slot values");
                                    }
                                    else
                                    {
                                        // TODO verify the slot values.
                                        foreach(string slotKey in slotMapping.RequiredSlotValues.Keys)
                                        {
                                            if(string.IsNullOrWhiteSpace(slotKey))
                                                valResult.Add($"Choice index {choiceIndex} slot mapping {mapIndex} has a missing slot key.");
                                            else
                                            {
                                                List<string> slotVals = slotMapping.RequiredSlotValues[slotKey];
                                                if( (slotVals?.Any()).GetValueOrDefault(false)==false)
                                                {
                                                    valResult.Add($"Choice index {choiceIndex} slot mapping {mapIndex} with slot key {slotKey} has no values");
                                                }
                                                else
                                                {

                                                    if (foundIntent != null)
                                                        foreach (string slotValue in slotVals)
                                                        {
                                                            // TODO -- adjust for localization
                                                            bool doesSlotValExist = DoesSlotValueExist(slotKey, slotValue, foundIntent, slots);
                                                            if(!doesSlotValExist)
                                                                valResult.Add($"Choice index {choiceIndex} slot mapping {mapIndex} with slot key {slotKey} and value {slotValue} does not exist in intent {foundIntent.Name}");
                                                        }
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
            }


            return valResult;
        }

        private bool DoesSlotValueExist(string slotKey, string slotValue, Intent foundIntent, List<SlotType> slots)
        {
            bool doesSlotExist = false;


             if(foundIntent!=null && (slots?.Any()).GetValueOrDefault(false))
            {

                if (foundIntent.SlotMappingsByName != null)
                {

                    if (foundIntent.SlotMappingsByName.ContainsKey(slotKey))
                    {

                        string slotMapping = foundIntent.SlotMappingsByName[slotKey];

                        if (!string.IsNullOrWhiteSpace(slotMapping))
                        {
                            SlotType foundSlotType = slots.FirstOrDefault(x => x.Name.Equals(slotMapping, StringComparison.OrdinalIgnoreCase));
                            if (foundSlotType != null)
                            {
                                List<string> matchedValues = foundSlotType.GetMatchedValues(slotValue);

                                if ((matchedValues?.Any()).GetValueOrDefault(false))
                                    doesSlotExist = true;


                            }
                        }
                    }
                }                
            }


            return doesSlotExist;
        }

        private async Task<List<string>> ValidateClientSpeechResponsesAsync(TitleVersion titleVersion, int responseSetIndex, int locIndex, List<ClientSpeechFragments> speechFragments, List<FileCounter> audioFileCount, List<Models.Conditions.StoryConditionBase> conditions)
        {
            List<string> clientSpeechResponses = new List<string>();

            Dictionary<Client, bool> hasClientResponse = new Dictionary<Client, bool>();
            hasClientResponse.Add(Client.Unknown, false);

            foreach(Client reqClient in RequiredClients)
            {
                hasClientResponse.Add(reqClient, false);

            }

            if ((speechFragments?.Any()).GetValueOrDefault(false))
            {
                for (int speechRespIndex = 0; speechRespIndex < speechFragments.Count; speechRespIndex++)
                {
                    ClientSpeechFragments clientFrag = speechFragments[speechRespIndex];

                    if (clientFrag == null)
                    {
                        clientSpeechResponses.Add($"ResponseSet index {responseSetIndex}, localized response index {locIndex}, client speech fragment {speechRespIndex} is null");
                    }
                    else
                    {
                        if ((clientFrag.SpeechFragments?.Any()).GetValueOrDefault(false) == false)
                        {
                            clientSpeechResponses.Add($"ResponseSet index {responseSetIndex}, localized response index {locIndex}, client speech fragment {speechRespIndex} has no speech fragments");
                        }
                        else
                        {
                            if(clientFrag.SpeechClient.HasValue)
                                hasClientResponse[clientFrag.SpeechClient.Value] = true;                                                   
                            else
                                hasClientResponse[Client.Unknown] = true;


                            List<string> speechValidation = await ValidateSpeechNodesAsync(titleVersion, responseSetIndex, locIndex, speechRespIndex, clientFrag.SpeechFragments, audioFileCount, conditions);
                            if ((speechValidation?.Any()).GetValueOrDefault(false))
                            {
                                clientSpeechResponses.AddRange(speechValidation);
                            }
                        }
                    }
                }
            }

            // If the speech fragments have an unknown client response, then it applies to all clients.
            if(!hasClientResponse[Client.Unknown])
            {
                foreach (Client reqClient in RequiredClients)
                {
                    // There is no default client response, therefore, it needs speech fragments for other clients to be explicitly called out.
                    if (!hasClientResponse[reqClient])
                        clientSpeechResponses.Add($"ResponseSet index {responseSetIndex}, localized response index {locIndex} does not define any responses for {reqClient}");
                }
            }


            return clientSpeechResponses;

        }


        private async Task<List<string>> ValidateSpeechNodesAsync(TitleVersion titleVer, int responseSetIndex, int locIndex, int speechRespIndex, List<SpeechFragment> speechFragments, List<FileCounter> audioFileCount, List<Models.Conditions.StoryConditionBase> conditions)
        {
            List<string> valErrors = new List<string>();

            if((speechFragments?.Any()).GetValueOrDefault(false))
            {

                for (int fragmentIndex = 0; fragmentIndex < speechFragments.Count; fragmentIndex++)
                //foreach(var speechResp in speechFragments)
                {
                    SpeechFragment speechFrag = speechFragments[fragmentIndex];
                    if(speechFrag==null)
                    {
                        valErrors.Add(string.Format("ResponseSetIndex {0}, localized response index {1}, speechResponseIndex {2}, fragementIndex {3} fragment is null or missing. Possible formatting issue.", responseSetIndex, locIndex, speechRespIndex, fragmentIndex));
                    }
                    else if(speechFrag is DirectAudioFile)
                    {
                        DirectAudioFile dirFile = (DirectAudioFile)speechFrag;

                        string url = dirFile.AudioUrl;

                        if(string.IsNullOrWhiteSpace(url))                        
                            valErrors.Add(string.Format("ResponseSetIndex {0}, localized response index {1}, speechResponseIndex {2}, fragementIndex {3} DirectAudioFile url is missing", responseSetIndex, locIndex, speechRespIndex, fragmentIndex));

                        
                        // TODO: Validate url;

                    }
                   else if(speechFrag is AudioFile)
                    {
                        AudioFile audFile = (AudioFile)speechFrag;

                        string fileName = audFile.FileName;

                        if (string.IsNullOrWhiteSpace(fileName))
                            valErrors.Add(string.Format("ResponseSetIndex {0}, localized response index {1}, speechResponseIndex {2}, fragementIndex {3} AudioFile is missing the file name", responseSetIndex, locIndex, speechRespIndex, fragmentIndex));
                        else
                        {
                            FileCounter foundCounter = audioFileCount.FirstOrDefault(x => x.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));

                            if (foundCounter == null)
                                valErrors.Add(string.Format("ResponseSetIndex {0}, localized response index {1}, speechResponseIndex {2}, fragementIndex {3} AudioFile {4} is missing", responseSetIndex, locIndex, speechRespIndex, fragmentIndex, fileName));
                            else
                            {

                                Debug.WriteLine(string.Format("File {0} found for AudioFile speech fragment", fileName));
                                foundCounter.Increment();
                            }
                        }
                    }
                    else if(speechFrag is SsmlSpeechFragment)
                    {
                        SsmlSpeechFragment ssmlFrag = (SsmlSpeechFragment)speechFrag;

                        if(string.IsNullOrWhiteSpace(ssmlFrag.Ssml))
                            valErrors.Add(string.Format("ResponseSetIndex {0}, localized response index {1}, speechResponseIndex {2}, fragementIndex {3} SsmlFragement is missing the SSML entry", responseSetIndex, locIndex, speechRespIndex, fragmentIndex));
                    }
                    else if(speechFrag is PlainTextSpeechFragment)
                    {
                        PlainTextSpeechFragment plainFrag = (PlainTextSpeechFragment)speechFrag;

                        if(string.IsNullOrWhiteSpace(plainFrag.Text))
                            valErrors.Add(string.Format("ResponseSetIndex {0}, localized response index {1}, speechResponseIndex {2}, fragementIndex {3} PlainTextSpeechFragment is missing the text entry", responseSetIndex, locIndex, speechRespIndex, fragmentIndex));
                    }
                    else if(speechFrag is ConditionalFragment)
                    {
                        ConditionalFragment condFrag = (ConditionalFragment)speechFrag;

                        if((condFrag.Conditions?.Any()).GetValueOrDefault(false)==false)
                        {
                            valErrors.Add(string.Format("ResponseSetIndex {0}, localized response index {1}, speechResponseIndex {2}, fragementIndex {3} No conditions found on condition fragment", responseSetIndex, locIndex, speechRespIndex, fragmentIndex));
                        }
                        else
                        {
                            for (int condIndex = 0; condIndex < condFrag.Conditions.Count; condIndex++)
                            {
                                string condName = condFrag.Conditions[condIndex];


                                if ((conditions?.Any()).GetValueOrDefault(false) == false)
                                {
                                    valErrors.Add(string.Format("ResponseSetIndex {0}, localized response index {1}, speechResponseIndex {2}, fragementIndex {3} No title conditions defined", responseSetIndex, locIndex, speechRespIndex, fragmentIndex));
                                }
                                else
                                {
                                    if(string.IsNullOrWhiteSpace(condName))
                                    {
                                        valErrors.Add(string.Format("ResponseSetIndex {0}, localized response index {1}, speechResponseIndex {2}, fragementIndex {3}, conditionIndex {4} is blank", responseSetIndex, locIndex, speechRespIndex, fragmentIndex, condIndex));
                                    }
                                    else
                                    {
                                        var foundCondition = conditions.FirstOrDefault(x => x.Name.Equals(condName, StringComparison.OrdinalIgnoreCase));

                                        if(foundCondition==null)
                                            valErrors.Add(string.Format("ResponseSetIndex {0}, localized response index {1}, speechResponseIndex {2}, fragementIndex {3}, conditionIndex {4} condition name {5} not found", responseSetIndex, locIndex, speechRespIndex, fragmentIndex, condIndex, condName));
                                    }
                                }
                            }

                        }

                       
                        if ((!condFrag.FalseResultFragments?.Any()).GetValueOrDefault(false) && (!condFrag.TrueResultFragments?.Any()).GetValueOrDefault(false))
                        {
                            valErrors.Add(string.Format("ResponseSetIndex {0}, localized response index {1}, speechResponseIndex {2}, fragementIndex {3} conditional fragment is missing true and false fragments.", responseSetIndex, locIndex, speechRespIndex, fragmentIndex));
                        }
                        else
                        {
                            if((condFrag.TrueResultFragments?.Any()).GetValueOrDefault(false))
                            {
                                var frags = condFrag.TrueResultFragments;
                                List<string> trueFragIssues = await ValidateSpeechNodesAsync(titleVer, responseSetIndex, locIndex, speechRespIndex,frags, audioFileCount, conditions);

                                if((trueFragIssues?.Any()).GetValueOrDefault(false))
                                {
                                    foreach(string trueFragIssue in trueFragIssues)
                                    {
                                        valErrors.Add(string.Concat("True fragments issues: ", trueFragIssue));
                                    }

                                }

                            }
                            else if ((condFrag.FalseResultFragments?.Any()).GetValueOrDefault(false))
                            {

                                var frags = condFrag.FalseResultFragments;
                                List<string> falseFragIssues = await ValidateSpeechNodesAsync(titleVer, responseSetIndex, locIndex, speechRespIndex, frags, audioFileCount, conditions);

                                if ((falseFragIssues?.Any()).GetValueOrDefault(false))
                                {
                                    foreach (string falseFragIssue in falseFragIssues)
                                    {
                                        valErrors.Add(string.Concat("False fragments issues: ", falseFragIssue));
                                    }

                                }

                            }

                        }
                    }

                }
            }

            return valErrors;
        }

        private async Task<List<string>> ValidateCardNodeAsync(TitleVersion titleVersion, int responseSetIndex, int locIndex, LocalizedResponse locResp)
        {
            List<string> cardIssues = new List<string>();

            // If there are no card responses, perform the old logic
            if ( (locResp.CardResponses?.Any()).GetValueOrDefault(false) == false )
            {
                if (string.IsNullOrWhiteSpace(locResp.CardTitle))
                {
                    cardIssues.Add(string.Format("Card response for response set {0} and localized response index {1} is missing the card title", responseSetIndex, locIndex));
                }


                if ((locResp.TextFragments?.Any()).GetValueOrDefault(false) == false)
                {
                    cardIssues.Add(string.Format("Card response for response set {0} and localized response index {1} is missing text fragments", responseSetIndex, locIndex));
                }

                if (!string.IsNullOrWhiteSpace(locResp.SmallImageFile))
                {
                    bool doesExist = await DoesFileExistAsync(titleVersion, locResp.SmallImageFile);

                    if (!doesExist)
                        cardIssues.Add(string.Format("Card response for response set {0} and localized response index {1} smallImageFile {2} not found", responseSetIndex, locIndex, locResp.SmallImageFile));
                }


                if (!string.IsNullOrWhiteSpace(locResp.LargeImageFile))
                {
                    bool doesExist = await DoesFileExistAsync(titleVersion, locResp.LargeImageFile);

                    if (!doesExist)
                        cardIssues.Add(string.Format("Card response for response set {0} and localized response index {1} largeImageFile {2} not found", responseSetIndex, locIndex, locResp.LargeImageFile));
                }
            }
            else
            {
                // Otherwise, there should not be any values in the old nodes and we need to validate the individual nodes

                if (!string.IsNullOrWhiteSpace(locResp.CardTitle))
                {
                    cardIssues.Add(string.Format("Card response for response set {0} and localized response index {1} has card responses and a card title", responseSetIndex, locIndex));
                }


                if ((locResp.TextFragments?.Any()).GetValueOrDefault(false))
                {
                    cardIssues.Add(string.Format("Card response for response set {0} and localized response index {1} has card responses and text fragments", responseSetIndex, locIndex));
                }

                if (!string.IsNullOrWhiteSpace(locResp.SmallImageFile))
                {
                    cardIssues.Add(string.Format("Card response for response set {0} and localized response index {1} has card responses and a value for SmallImageFile", responseSetIndex, locIndex));
                }


                if (!string.IsNullOrWhiteSpace(locResp.LargeImageFile))
                {
                    cardIssues.Add(string.Format("Card response for response set {0} and localized response index {1} has card responses and a value for LargeImageFile", responseSetIndex, locIndex));
                }

                bool fHasBixby = false;
                bool fHasAlexa = false;
                bool fHasGoogle = false;
                bool fHasDefault = false;

                for(int n = 0; n < locResp.CardResponses.Count; n++)
                {
                    CardResponse cardResp = locResp.CardResponses[n];

                    List<string> cardResponseIssues = await ValidateCardResponseAsync(titleVersion, responseSetIndex, locIndex, n, cardResp);
                    if ((cardResponseIssues?.Any()).GetValueOrDefault(false))
                    {
                        cardIssues.AddRange(cardResponseIssues);
                    }

                    switch( cardResp.SpeechClient.GetValueOrDefault(Client.Unknown) )
                    {
                        case Client.Bixby:
                            {
                                fHasBixby = true;
                                break;
                            }

                        case Client.Alexa:
                            {
                                fHasAlexa = true;
                                break;
                            }

                        case Client.GoogleHome:
                            {
                                fHasGoogle = true;
                                break;
                            }

                        case Client.Unknown:
                            {
                                fHasDefault = true;
                                break;
                            }

                    }

                }

                //
                // We have to be able to resolve all client values.
                //
                if (!fHasBixby && !fHasDefault)
                    cardIssues.Add(string.Format("Card response for response set {0} and localized response index {1} cannot resolve Bixby.", responseSetIndex, locIndex));
                if (!fHasAlexa && !fHasDefault)
                    cardIssues.Add(string.Format("Card response for response set {0} and localized response index {1} cannot resolve Alexa.", responseSetIndex, locIndex));
                if (!fHasGoogle && !fHasDefault)
                    cardIssues.Add(string.Format("Card response for response set {0} and localized response index {1} cannot resolve Google.", responseSetIndex, locIndex));


            }


            return cardIssues;
        }

        private async Task<List<string>> ValidateCardResponseAsync(TitleVersion titleVersion, int responseSetIndex, int locIndex, int cardIndex, CardResponse cardResp)
        {
            List<string> cardIssues = new List<string>();

            if (string.IsNullOrWhiteSpace(cardResp.CardTitle))
            {
                cardIssues.Add(string.Format("Card response {0} for response set {1} and localized response index {2} is missing the card title", cardIndex, responseSetIndex, locIndex));
            }


            if ((cardResp.TextFragments?.Any()).GetValueOrDefault(false) == false)
            {
                cardIssues.Add(string.Format("Card response {0} for response set {1} and localized response index {2} is missing text fragments", cardIndex, responseSetIndex, locIndex));
            }

            if (!string.IsNullOrWhiteSpace(cardResp.SmallImageFile))
            {
                bool doesExist = await DoesFileExistAsync(titleVersion, cardResp.SmallImageFile);

                if (!doesExist)
                    cardIssues.Add(string.Format("Card response {0} for response set {1} and localized response index {2} smallImageFile {3} not found", cardIndex, responseSetIndex, locIndex, cardResp.SmallImageFile));
            }


            if (!string.IsNullOrWhiteSpace(cardResp.LargeImageFile))
            {
                bool doesExist = await DoesFileExistAsync(titleVersion, cardResp.LargeImageFile);

                if (!doesExist)
                    cardIssues.Add(string.Format("Card response {0} for response set {1} and localized response index {2} largeImageFile {3} not found", cardIndex, responseSetIndex, locIndex, cardResp.LargeImageFile));


            }


            return cardIssues;
        }


        private bool DoesNodeExist(string nodeName, ICollection<StoryNode> nodes)
        {
            bool doesExist = false;

            if((nodes?.Any()).GetValueOrDefault(false))
            {
                doesExist = nodes.Any(x => x.Name.Equals(nodeName, StringComparison.OrdinalIgnoreCase));
            }


            return doesExist;

        }

        private async Task<bool> DoesFileExistAsync(TitleVersion titleVersion, string fileName)
        {
            bool fileExists = false;


            fileExists = await _fileRep.DoesFileExistAsync(titleVersion, fileName);


            return fileExists;
        }

        private async Task<List<string>> GetAudioFileList( TitleVersion titleVer)
        {
            List<string> audioList = await _fileRep.GetAudioFileListAsync( titleVer);

            return audioList;
        }

    }

    [DebuggerDisplay("{FileName}:{UseCount}")]
    internal class FileCounter
    {
        private int _useCount = 0;

        internal FileCounter(string fileName)
        {
            FileName = fileName;
        
        }

        internal string FileName { get; set; }
        internal int UseCount { get { return _useCount; } }

        internal void Increment()
        {
            _useCount++;
        }

    }
        


   

}
