using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Story.Ssml;

namespace Whetstone.StoryEngine.Models.Data
{
    public static class DataConversionExtensions
    {




        //public static DataNode ToDataNode(this StoryNode node, DataTitleVersion storyVersion)
        //{
        //    DataNode retNode = new DataNode();


        //    retNode.Name = node.Name;
        //    retNode.ResponseBehavior = node.ResponseBehavior;
        //    retNode.NodeVisitConditionXRefs = new List<DataNodeVisitConditionXRef>();
        //    retNode.Coordinates = node.Coordinates;
        //    retNode.Version = storyVersion;
        //    //retNode.Actions = new List<NodeActionBase>();

        //    if (node.Choices != null && node.Choices.Any())
        //    {

        //        retNode.Choices = new List<DataChoice>();

        //        foreach (Choice storyChoice in node.Choices)
        //        {
        //            retNode.Choices.Add(storyChoice.ToDataChoice(storyVersion));


        //        }
        //    }



        //    return retNode;
        //}



        //public static DataChoice ToDataChoice(this Choice choice, DataTitleVersion storyVersion)
        //{
        //    DataChoice retChoice = new DataChoice();

            
        //    // Find intent by name
            
        //    DataIntent foundIntent =  storyVersion.Intents.FirstOrDefault(x => x.Name.Equals(choice.IntentName));

            

        //    if (foundIntent != null)
        //        retChoice.Intent = foundIntent;



        //    return retChoice;
        //}


        public static DataIntent ToDataIntent(this Intent intent, List<DataSlotType> slotTypes)
        {

            DataIntent dataIntent = new DataIntent();

            if (intent.UniqueId.HasValue)
                dataIntent.UniqueId = intent.UniqueId.Value;
            else
                dataIntent.UniqueId = Guid.NewGuid();

            dataIntent.Id = intent.Id;
            dataIntent.Name = intent.Name;
            dataIntent.LocalizedIntents = intent.LocalizedIntents;


            if (intent.SlotMappingsByName != null && intent.SlotMappingsByName.Keys.Any())
            {
                dataIntent.SlotTypeMappings = new List<DataIntentSlotMapping>();
                foreach (string key in intent.SlotMappingsByName.Keys)
                {

                    string slotMappingValue = intent.SlotMappingsByName[key];


                    DataSlotType foundSlotType =
                        slotTypes.FirstOrDefault(x => x.Name.Equals(slotMappingValue, StringComparison.OrdinalIgnoreCase));

                    if (foundSlotType != null)
                    {

                        DataIntentSlotMapping mapping = new DataIntentSlotMapping();
                        mapping.Alias = key;
                        mapping.SlotType = foundSlotType;                     
                        dataIntent.SlotTypeMappings.Add(mapping);

                    }
                       
                }
            }

            if ((intent.SlotMappings?.Any()).GetValueOrDefault(false))
            {
                dataIntent.SlotTypeMappings = new List<DataIntentSlotMapping>();

                foreach(IntentSlotMapping intentSlot in intent.SlotMappings)
                {
                    string slotName = intentSlot.SlotType.Name;


                    DataSlotType foundSlotType =
                        slotTypes.FirstOrDefault(x => x.Name.Equals(slotName, StringComparison.OrdinalIgnoreCase));

                    if (foundSlotType != null)
                    {

                        DataIntentSlotMapping mapping = new DataIntentSlotMapping();
                        mapping.Alias = intentSlot.Alias;

                        mapping.Intent = dataIntent;
                        mapping.IntentId = dataIntent.Id;
                        mapping.SlotTypeId = foundSlotType.Id;
                        mapping.SlotType = foundSlotType;

                        dataIntent.SlotTypeMappings.Add(mapping);

                    }
                }
            }
            return dataIntent;
        }


        //internal static DataLocalizedIntent ToDataLocalizedIntent(this LocalizedIntent locIntent)
        //{
        //    DataLocalizedIntent dataLocIntent = new DataLocalizedIntent();

        //    dataLocIntent.Locale = locIntent.Locale;
        //    dataLocIntent.PlainTextPrompt = locIntent.PlainTextPrompt;

        //    if (locIntent.UniqueId.HasValue)
        //        dataLocIntent.UniqueId = locIntent.UniqueId.Value;
        //    else
        //        dataLocIntent.UniqueId = Guid.NewGuid();

        //    if (locIntent.Utterances != null && locIntent.Utterances.Any())
        //    {
        //        dataLocIntent.Utterances = locIntent.Utterances.ToArray();
        //        // dataLocIntent.Utterances = new List<string>();
        //        // dataLocIntent.Utterances.AddRange(locIntent.Utterances);
        //    }
        

        //    return dataLocIntent;
        //}


        public static List<DataSlotType> ToDataSlotTypes(this List<SlotType> slotTypes)
        {

            if (slotTypes != null)
            {
                List<DataSlotType> dataTypes = new List<DataSlotType>();

                foreach (SlotType slotType in slotTypes)
                {
                    DataSlotType dataType = slotType.ToDataSlotType();
                    if (dataType != null)
                        dataTypes.Add(dataType);

                }


                return dataTypes;
            }

            return null;

        }


        public static DataSlotType ToDataSlotType(this SlotType slotType)
        {

            if (slotType != null)
            {
                DataSlotType retType = new DataSlotType();


                retType.Id = slotType.Id;
                retType.Name = slotType.Name;
                retType.Values = slotType.Values;

                if (slotType.UniqueId.HasValue)
                {
                    retType.UniqueId = slotType.UniqueId.Value;

                }
                else
                    retType.UniqueId = Guid.NewGuid();

                return retType;
            }

            return null;
        }


        //public static DataLocalizedResponseSet ToDataLocalizedResponseSet(this LocalizedResponseSet respSet, DataTitleVersion parentVersion)
        //{
        //    if (respSet != null)
        //    {
        //        DataLocalizedResponseSet retVal = new DataLocalizedResponseSet();

        //        if ((respSet.LocalizedResponses?.Any()).GetValueOrDefault(false))
        //        {
        //            retVal.LocalizedResponses = new List<DataLocalizedResponse>();
        //            foreach (LocalizedResponse locResp in respSet.LocalizedResponses)
        //            {
        //                if (locResp != null)
        //                {
        //                    DataLocalizedResponse dataLocResp = locResp.ToDataLocalizedResponse(parentVersion);
        //                    retVal.LocalizedResponses.Add(dataLocResp);

        //                }
        //            }

        //        }


        //        return retVal;
        //    }


        //    return null;
        //}


        //public static DataLocalizedResponse ToDataLocalizedResponse(this LocalizedResponse locResponse, DataTitleVersion parentVersion)
        //{

        //    if (locResponse != null)
        //    {

        //        DataLocalizedResponse retVal = new DataLocalizedResponse();

        //        retVal.CardTitle = locResponse.CardTitle;
        //        retVal.GeneratedTextResponse = locResponse.GeneratedTextResponse;
        //        retVal.LargeImageFile = locResponse.LargeImageFile;
        //        retVal.Locale = locResponse.Locale;

        //        retVal.RepromptTextResponse = locResponse.RepromptTextResponse;
        //        retVal.SendCardResponse = locResponse.SendCardResponse;
        //        retVal.SmallImageFile = locResponse.SmallImageFile;


        //        if ((locResponse.RepromptSpeechResponses?.Any()).GetValueOrDefault(false))
        //        {
        //            retVal.RepromptSpeechResponses = new List<DataClientSpeechFragments>();
        //            foreach (ClientSpeechFragments clientFrag in locResponse.RepromptSpeechResponses)
        //            {
        //                retVal.RepromptSpeechResponses.Add(clientFrag.ToDataClientSpeechFragments(parentVersion));
        //            }
        //        }


        //        if ((locResponse.SpeechResponses?.Any()).GetValueOrDefault(false))
        //        {
        //            retVal.SpeechResponses = new List<DataClientSpeechFragments>();
        //            foreach (ClientSpeechFragments clientFrag in locResponse.SpeechResponses)
        //            {
        //                retVal.SpeechResponses.Add(clientFrag.ToDataClientSpeechFragments(parentVersion));
        //            }
        //        }



        //        return retVal;

        //    }


        //    return null;
        //}


        //public static DataClientSpeechFragments ToDataClientSpeechFragments(this ClientSpeechFragments clientFrags, DataTitleVersion parentVersion)
        //{

        //    if (clientFrags != null)
        //    {
        //        DataClientSpeechFragments retVal = new DataClientSpeechFragments();

        //        retVal.SpeechClient = clientFrags.SpeechClient;

               

        //        if ((clientFrags.SpeechFragments?.Any()).GetValueOrDefault(false))
        //        {
        //            retVal.SpeechFragments = new List<DataSpeechFragment>();


        //            int sequence = 0;
        //            foreach (SpeechFragment speechFrag in clientFrags.SpeechFragments)
        //            {
        //                DataSpeechFragment dataFragment = ImportSpeechFragment(parentVersion, speechFrag);

        //                dataFragment.Sequence = sequence;
        //                sequence++;

        //                retVal.SpeechFragments.Add(dataFragment);
        //            }



        //        }

        //        return retVal;
        //    }


        //    return null;
        //}

        //public static DataSpeechFragment ImportSpeechFragment(DataTitleVersion parentVersion, SpeechFragment speechFrag)
        //{
        //    if (parentVersion == null)
        //        throw new ArgumentException("parentVersion cannot be null");

        //    if (speechFrag == null)
        //        throw new ArgumentException("speechFrag cannot be null");


        //    DataSpeechFragment dataFragment = null;
        //    if (speechFrag is PlainTextSpeechFragment sourceFragment)
        //    {
        //        DataSpeechText dataSpeechText = new DataSpeechText();
        //        dataSpeechText.Text = sourceFragment.Text;
        //        dataSpeechText.Voice = sourceFragment.Voice;

        //        if (!string.IsNullOrWhiteSpace(dataSpeechText.Voice))
        //        {
        //            dataSpeechText.VoiceFileId = dataSpeechText.VoiceFileId.HasValue ? sourceFragment.VoiceFileId : Guid.NewGuid();
        //        }
        //        dataFragment = dataSpeechText;
        //    }

        //    if (speechFrag is AudioFile file)
        //    {
        //        DataAudioFile audioFile = new DataAudioFile();
        //        audioFile.FileName = file.FileName;
        //        dataFragment = audioFile;
        //    }

        //    if (speechFrag is DirectAudioFile directAudioFile)
        //    {
        //        DataDirectAudioFile dataAudioFile = new DataDirectAudioFile();
        //        dataAudioFile.AudioUrl = directAudioFile.AudioUrl;
        //        dataFragment = dataAudioFile;
        //    }

        //    if (speechFrag is SsmlSpeechFragment ssmlFragment)
        //    {
        //        DataSsmlSpeechFragment ssmlDataFragment = new DataSsmlSpeechFragment();
        //        ssmlDataFragment.Ssml = ssmlFragment.Ssml;
        //        ssmlDataFragment.Voice = ssmlFragment.Voice;
        
        //        if (!string.IsNullOrWhiteSpace(ssmlFragment.Voice))
        //        {
        //            ssmlDataFragment.VoiceFileId = ssmlFragment.VoiceFileId.HasValue ? ssmlFragment.VoiceFileId : Guid.NewGuid();
        //        }
        //    }

        //    if (speechFrag is ConditionalFragment origFrag)
        //    {
        //        DataConditionalFragment dataConditional = new DataConditionalFragment();

        //        if ((origFrag.TrueResultFragments?.Any()).GetValueOrDefault(false))
        //        {
        //            List<SpeechFragment> trueResultFragments = origFrag.TrueResultFragments;
        //            dataConditional.TrueResultFragments = new List<DataSpeechFragment>();
        //            int trueSequence = 1;
        //            foreach (SpeechFragment trueFrag in trueResultFragments)
        //            {
        //                DataSpeechFragment processFrag = ImportSpeechFragment(parentVersion, trueFrag);
        //                processFrag.Sequence = trueSequence;
        //                trueSequence++;
        //                dataConditional.TrueResultFragments.Add(processFrag);
        //            }


        //        }

        //        if ((origFrag.FalseResultFragments?.Any()).GetValueOrDefault(false))
        //        {
        //            List<SpeechFragment> falseResultFragments = origFrag.FalseResultFragments;

        //          //  dataConditional.FalseResultFragments = new List<DataSpeechFragment>();
        //            int falseSequence = 1;
        //            foreach(SpeechFragment falseFrag in falseResultFragments)
        //            {
        //                DataSpeechFragment processFrag = ImportSpeechFragment(parentVersion, falseFrag);
        //                processFrag.Sequence = falseSequence;
        //                falseSequence++;
        //              //  dataConditional.FalseResultFragments.Add(processFrag);
        //            }
        //        }

        //        if ((origFrag.Conditions?.Any()).GetValueOrDefault(false))
        //        {
        //            foreach (string condition in origFrag.Conditions)
        //            {
        //                // Find the condition in the node visit or the inventory conditions

          

        //                DataNodeVisitCondition foundVisitConditon = null;
        //                DataInventoryCondition dataInventoryCondition = null;
        //                try
        //                {
        //                    if (parentVersion.DataNodeVisitConditions == null)
        //                    {
        //                        parentVersion.DataNodeVisitConditions = new List<DataNodeVisitCondition>();
        //                    }

        //                    foundVisitConditon = parentVersion.DataNodeVisitConditions.FirstOrDefault(x => x.Name.Equals(condition, StringComparison.OrdinalIgnoreCase));
        //                    if (foundVisitConditon != null)
        //                    {
        //                       // dataConditional.NodeVisitConditions.Add(foundVisitConditon);
        //                    }
        //                    else
        //                    {
        //                        if (parentVersion.DataInventoryConditions == null)
        //                        {
        //                            parentVersion.DataInventoryConditions = new List<DataInventoryCondition>();
        //                        }

        //                        dataInventoryCondition = parentVersion.DataInventoryConditions.FirstOrDefault(x => x.Name.Equals(condition, StringComparison.OrdinalIgnoreCase));
        //                        //if (dataInventoryCondition != null)
        //                        //    dataConditional.InventoryConditions.Add(dataInventoryCondition);
        //                    }
        //                }

        //                catch(Exception ex)
        //                {
        //                    Debug.WriteLine(ex);
                            

        //                }
        //            }

        //        }


        //        dataFragment = dataConditional;

        //    }

        //    if (dataFragment != null)
        //        dataFragment.StoryVersion = parentVersion;


        //    return dataFragment;
        //}

        //public static List<DataLocalizedResponseSet> ToDataLocalizedResponseSetList(
        //    this List<LocalizedResponseSet> responseSetList, DataTitleVersion parentVersion)
        //{



        //    if ((responseSetList?.Any()).GetValueOrDefault(false))
        //    {
        //        List<DataLocalizedResponseSet> dataResponseSet = new List<DataLocalizedResponseSet>();

        //        foreach (LocalizedResponseSet locSet in responseSetList)
        //        {
        //            if(locSet!=null)
        //                dataResponseSet.Add(locSet.ToDataLocalizedResponseSet(parentVersion));
        //        }

        //        return dataResponseSet;
        //    }


        //    return null;
        //}


    }
}
