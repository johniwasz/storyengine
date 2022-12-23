using Google.Cloud.Dialogflow.V2;
using Google.Cloud;
using Google.Api.Gax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Story;
using Microsoft.Extensions.Logging;
using Intent = Google.Cloud.Dialogflow.V2.Intent;
using System.Text.RegularExpressions;
using System.Data;
using System.Threading;
using Whetstone.StoryEngine.Google.Management.Models;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Google.Management
{
    public class DialogFlowManager : IDialogFlowManager
    {
        private ILogger<DialogFlowManager> _logger;

        private const string _IntentsFolderName = "intents";
        private const string _EntitiesFolderName = "entities";
        private const string _phoneNumberExample = "215-555-1212";

        public DialogFlowManager(ILogger<DialogFlowManager> logger)
        {

            _logger = logger;
        }



        public async Task ImportTitleAsync(string projectId, StoryTitle title)
        {
            List<SlotType> slotTypes = title.Slots;

            await ImportSlotTypesAsync(projectId, slotTypes);

            var intentsToAdd = title.Intents;

            await ImportIntentsByUsageAsync(projectId, title.Nodes, title.Intents, title.Slots);

            var sysIntents = ReservedIntents.GetSystemIntents();

            await ImportIntentsAsync(projectId, sysIntents, title.Slots);
        }

        private async Task<PagedEnumerable<ListIntentsResponse, Intent>> GetListIntentsAsync(string projectId)
        {
            IntentsClient client = await IntentsClient.CreateAsync();
            ListIntentsRequest listReq = new ListIntentsRequest();
            listReq.IntentView = IntentView.Full;
            listReq.Parent = GetParentName(projectId);

            PagedEnumerable<ListIntentsResponse, Intent> listResponses = client.ListIntents(listReq);

          
            return listResponses;

        }

        

        private async Task ImportIntentsByUsageAsync(string projectId, List<StoryNode> nodes, List<Whetstone.StoryEngine.Models.Story.Intent> intents, List<SlotType> slotTypes)
        {

            if ((intents?.Any()).GetValueOrDefault())
            {

                var listIntents = await GetListIntentsAsync(projectId);
                var entities = await GetEntityListAsync(projectId);

                foreach (var storyIntent in intents)
                {

                    var curIntent = storyIntent;

                    if (storyIntent.Name.Equals(ReservedIntents.YesIntent.Name))
                    {
                        curIntent = ReservedIntents.YesIntent;

                    }
                    else if (storyIntent.Name.Equals(ReservedIntents.NoIntent.Name))
                    {
                        curIntent = ReservedIntents.NoIntent;

                    }


                    foreach (LocalizedIntent locIntent in curIntent.LocalizedIntents)
                    {
                        Intent googleIntent = new Intent();
                        googleIntent.Events.Add("actions_intent_NO_INPUT");
                        googleIntent.Action = "no.input";

                        googleIntent.DisplayName = storyIntent.Name;

                        ContextName outContext = new ContextName(projectId, "-", $"{storyIntent.Name}-FollowUp");

                        googleIntent.OutputContexts.Add(new Context
                        {
                            ContextName = outContext,
                            LifespanCount = 5
                        });


                        bool intentHasSlots = (storyIntent.SlotMappingsByName?.Keys?.Any()).GetValueOrDefault(false);
                        ;

                        googleIntent.WebhookState = intentHasSlots ? Intent.Types.WebhookState.EnabledForSlotFilling :
                                                    Intent.Types.WebhookState.Enabled;

                        var trainingPhrases = GetTrainingPhrases(locIntent, storyIntent.SlotMappingsByName, slotTypes);

                        googleIntent.TrainingPhrases.AddRange(trainingPhrases);

                        //  googleIntent.TrainingPhrases.Add( new Intent.Types.TrainingPhrase)


                        if ((storyIntent.SlotMappingsByName?.Any()).GetValueOrDefault(false))
                        {
                            foreach (string key in storyIntent.SlotMappingsByName.Keys)
                            {
                                string slotName = storyIntent.SlotMappingsByName[key];
                                var foundEntity = await entities.FirstOrDefault(x => x.DisplayName.Equals(slotName, StringComparison.OrdinalIgnoreCase));

                                if (foundEntity == null)
                                {
                                    // Check if the entity is a system entity 

                                    if (key.Equals(WhetstoneIntents.US_PHONENUMBER_INTENT, StringComparison.OrdinalIgnoreCase))
                                    {
                                        Intent.Types.Parameter entityParam = new Intent.Types.Parameter();
                                        entityParam.DisplayName = "phone-number";
                                        entityParam.EntityTypeDisplayName = "@sys.phone-number";
                                        entityParam.Value = "$phone-number";
                                        entityParam.Mandatory = true;
                                        // entityParam.Name = foundEntity.Name;
                                        googleIntent.Parameters.Add(entityParam);
                                    }
                                    else
                                        _logger.LogError($"Slot mapping {slotName} not resolved for intent {storyIntent.Name}");
                                }
                                else
                                {
                                    Intent.Types.Parameter entityParam = new Intent.Types.Parameter();
                                    entityParam.DisplayName = foundEntity.DisplayName;
                                    entityParam.EntityTypeDisplayName = $"@{foundEntity.DisplayName}";
                                    entityParam.Value = $"${foundEntity.DisplayName}";
                                    entityParam.Mandatory = true;
                                    // entityParam.Name = foundEntity.Name;
                                    googleIntent.Parameters.Add(entityParam);
                                }
                            }

                        }
                        Thread.Sleep(1100);
                        await CreateOrUpdateIntentAsync(projectId, googleIntent, listIntents);

                    }


                }


            }

        }

        public async Task<byte[]> ExportTitleNlpAsync(StoryTitle title, string[] languages = null)
        {
            return await Task.Run(() =>
            {
                if (languages != null)
                {
                    throw new NotImplementedException("DialogFlowManager::ExportTitleNlpAsync, languages parameter cannot be non-null.");
                }

                // Initialize the languages to "en" if we need to.
                if (languages == null)
                {
                    string[] tempLanguages = { "en" };
                    languages = tempLanguages;
                }

                if (title == null)
                    throw new ArgumentNullException(nameof(title));

                byte[] zipBytes = null;

                var sysIntents = ReservedIntents.GetSystemIntents();

                using (var outStream = new MemoryStream())
                {
                    using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
                    {

                        this.AddPackageFile(archive);

                        // Add the system intents to the zip archive.
                        foreach (var sysIntent in sysIntents)
                        {
                            OutputIntentFiles(archive, sysIntent, title.Slots, languages);
                        }

                        foreach (var titleIntent in title.Intents)
                        {
                            var curIntent = this.NormalizeIntent(titleIntent);

                            if (curIntent != null)
                            {
                                OutputIntentFiles(archive, curIntent, title.Slots, languages);
                            }
                        }

                        foreach (SlotType slot in title.Slots)
                        {
                            OutputEntityFiles(archive, slot, languages);
                        }


                    }
                    zipBytes = outStream.ToArray();
                }



                return zipBytes;

            });


        }

        private void AddPackageFile(ZipArchive archive)
        {
            DialogFlowPackage package = new DialogFlowPackage();
            package.Version = "1.0.0";

            JsonSerializerSettings serSettings = new JsonSerializerSettings();
            serSettings.Formatting = Formatting.Indented;
            string packageText = JsonConvert.SerializeObject(package, serSettings);


            var fileInArchive = archive.CreateEntry("package.json", CompressionLevel.Optimal);
            using (var entryStream = fileInArchive.Open())
            using (var fileToCompressStream = new MemoryStream(Encoding.UTF8.GetBytes(packageText)))
            {
                fileToCompressStream.CopyTo(entryStream);
            }

        }

        private StoryEngine.Models.Story.Intent NormalizeIntent(StoryEngine.Models.Story.Intent intent)
        {
            StoryEngine.Models.Story.Intent normalizedIntent = intent;

            // Yes Intent and No Intent are special cased
            if (intent.Name.Equals(ReservedIntents.YesIntent.Name))
            {
                normalizedIntent = ReservedIntents.YesIntent;

            }
            else if (intent.Name.Equals(ReservedIntents.NoIntent.Name))
            {
                normalizedIntent = ReservedIntents.NoIntent;
            }
            else if (this.IsReservedSystemIntent(intent))
            {
                // Skip the intent if it's one of our system intents
                normalizedIntent = null;
            }

            return normalizedIntent;

        }

        private bool IsReservedSystemIntent( StoryEngine.Models.Story.Intent intent )
        {
            var foundIntent = ReservedIntents.GetSystemIntents().FirstOrDefault(x => x.Name.Equals(intent.Name, StringComparison.OrdinalIgnoreCase));
            return foundIntent != null;

        }
        private void OutputIntentFiles(ZipArchive archive, StoryEngine.Models.Story.Intent intent, List<SlotType> slots, string[] languages)
        {
            DialogFlowIntent exportedIntent = InitializeExportIntent(intent);

            // Forward slash paths will work for both Windows and Mac/Linuz
            string fileName = $"{_IntentsFolderName}/{intent.Name}.json";

            JsonSerializerSettings serSettings = new JsonSerializerSettings();
            serSettings.Formatting = Formatting.Indented;
            string intentText = JsonConvert.SerializeObject(exportedIntent, serSettings);


            var fileInArchive = archive.CreateEntry(fileName, CompressionLevel.Optimal);
            using (var entryStream = fileInArchive.Open())
            using (var fileToCompressStream = new MemoryStream(Encoding.UTF8.GetBytes(intentText)))
            {
                fileToCompressStream.CopyTo(entryStream);
            }

            // Make files with language specific utterances
            foreach (string language in languages)
            {
                List<IntentTrainingPhrase> trainingPhrases = this.InitializeExportTrainingPhrases(intent, slots, language);
                string utterancesFileName = $"{_IntentsFolderName}/{intent.Name}_usersays_{language}.json";

                string utteranceText = JsonConvert.SerializeObject(trainingPhrases, serSettings);

                var utterancesArchiveFile = archive.CreateEntry(utterancesFileName, CompressionLevel.Optimal);
                using (var utteranceStream = utterancesArchiveFile.Open())
                using (var fileToCompressStream = new MemoryStream(Encoding.UTF8.GetBytes(utteranceText)))
                {
                    fileToCompressStream.CopyTo(utteranceStream);
                }

            }

        }

        private void OutputEntityFiles(ZipArchive archive, StoryEngine.Models.SlotType slot, string[] languages)
        {

            DialogFlowEntity exportedEntity = InitializeExportEntity(slot.Name);

            // Forward slash paths will work for both Windows and Mac/Linuz
            string fileName = $"{_EntitiesFolderName}/{slot.Name}.json";

            JsonSerializerSettings serSettings = new JsonSerializerSettings();
            serSettings.Formatting = Formatting.Indented;
            string entityText = JsonConvert.SerializeObject(exportedEntity, serSettings);


            var fileInArchive = archive.CreateEntry(fileName, CompressionLevel.Optimal);
            using (var entryStream = fileInArchive.Open())
            using (var fileToCompressStream = new MemoryStream(Encoding.UTF8.GetBytes(entityText)))
            {
                fileToCompressStream.CopyTo(entryStream);
            }

            // Make files with language specific slot definitions
            // We need to update slots to support locales before this can happen
            // For now, we'll just pretend we're enumerating languages (we'll get
            // "en" this way
            foreach( string language in languages )
            {
                List<DialogFlowEntityValues> entityValues = this.InitializeExportEntityValues(slot, language);
                string valuesFileName = $"{_EntitiesFolderName}/{slot.Name}_entries_{language}.json";

                string valuesText = JsonConvert.SerializeObject(entityValues, serSettings);

                var valuesArchiveFile = archive.CreateEntry(valuesFileName, CompressionLevel.Optimal);
                using (var entryStream = valuesArchiveFile.Open())
                using (var fileToCompressStream = new MemoryStream(Encoding.UTF8.GetBytes(valuesText)))
                {
                    fileToCompressStream.CopyTo(entryStream);
                }

            }

        }

        private IntentResponse InitializeIntentResponse(StoryEngine.Models.Story.Intent intent)
        {
            IntentResponse response = new IntentResponse();
            response.ResetContexts = false;
            response.Action = "no.input";
            response.AffectedContexts = new List<AffectedContext>();

            AffectedContext ac = new AffectedContext();
            ac.Name = $"{intent.Name}-FollowUp";
            ac.Parameters = new object();
            ac.Lifespan = 5;

            response.AffectedContexts.Add(ac);

            // Add Parameters for slots if we have them
            response.Parameters = new List<IntentParameter>();

            if ( intent.SlotMappingsByName != null && intent.SlotMappingsByName.Count > 0 )
            {
                // We have one parameter per slot
                foreach( string slotName in intent.SlotMappingsByName.Keys )
                {
                    string slotType = intent.SlotMappingsByName[slotName];

                    IntentParameter intentParam = new IntentParameter();

                    intentParam.Required = true;
                    intentParam.PromptMessages = new List<object>();
                    intentParam.NoMatchPromptMessages = new List<object>();
                    intentParam.NoInputPromptMessages = new List<object>();
                    intentParam.OutputDialogContexts = new List<object>();
                    intentParam.IsList = false;

                    if ( slotType.Equals("WHETSTONE.US_PHONENUMBER") )
                    {
                        slotType = "sys.phone-number";
                    }

                    intentParam.Name = slotName;
                    intentParam.DataType = $"@{slotType}";
                    intentParam.Value = $"${slotName}";

                    response.Parameters.Add(intentParam);

                }
            }

            response.Messages = new List<IntentMessage>();
            response.DefaultResponsePlatforms = new IntentResponsePlatforms();
            response.Speech = new List<object>();

            return response;

        }

        private NamedEvent CreateDefaultNamedEvent()
        {
            NamedEvent namedEvent = new NamedEvent();
            namedEvent.Name = "actions_intent_NO_INPUT";

            return namedEvent;
        }

        private DialogFlowIntent InitializeExportIntent(StoryEngine.Models.Story.Intent intent)
        {
            DialogFlowIntent exportedIntent = new DialogFlowIntent();
            exportedIntent.Name = intent.Name;
            exportedIntent.Auto = true;
            exportedIntent.Contexts = new List<object>();
            exportedIntent.Responses = new List<IntentResponse>();

            exportedIntent.Responses.Add(this.InitializeIntentResponse(intent));

            exportedIntent.Priority = 500000;
            exportedIntent.WebhookUsed = true;
            exportedIntent.WebhookForSlotFilling = false;
            exportedIntent.FallbackIntent = false;
            exportedIntent.Events = new List<NamedEvent>();
            exportedIntent.Events.Add(this.CreateDefaultNamedEvent());

            exportedIntent.ConditionalResponses = new List<object>();
            exportedIntent.Condition = string.Empty;
            exportedIntent.ConditionalFollowupEvents = new List<object>();

            return exportedIntent;

        }

        private DialogFlowEntity InitializeExportEntity(string name)
        {
            DialogFlowEntity entity = new DialogFlowEntity();
            entity.Name = name;
            entity.IsOverridable = true;
            return entity;
        }

        private List<DialogFlowEntityValues> InitializeExportEntityValues(StoryEngine.Models.SlotType slot, string language )
        {
            List<DialogFlowEntityValues> entityValues = new List<DialogFlowEntityValues>();

            foreach (SlotValue slotValue in slot.Values)
            {
                DialogFlowEntityValues entityValue = new DialogFlowEntityValues();

                entityValue.Value = slotValue.Value;
                entityValue.Synonyms = new List<string>();

                // If we don't have synonyms, just add one using the value since DialogFlow
                // seems to barf on an entity if the synonyms box is checked and there isn't a value
                if (slotValue.Synonyms != null)
                {
                    foreach (string synonym in slotValue.Synonyms)
                    {
                        entityValue.Synonyms.Add(synonym);
                    }
                }
                else
                {
                    entityValue.Synonyms.Add(entityValue.Value);
                }

                entityValues.Add(entityValue);
            }

            return entityValues;
        }

        private LocalizedIntent GetLocalizedIntentForLanguage(StoryEngine.Models.Story.Intent intent, string language)
        {
            LocalizedIntent localizedIntent = intent.LocalizedIntents.FirstOrDefault(x => !String.IsNullOrEmpty(x.Locale) && x.Locale.Equals(language));

            if ( localizedIntent == null )
            {
                localizedIntent = intent.LocalizedIntents.FirstOrDefault(x => String.IsNullOrEmpty(x.Locale));
            }

            return localizedIntent;
        }

        private List<IntentTrainingPhrase> InitializeExportTrainingPhrases(StoryEngine.Models.Story.Intent intent, List<SlotType> slots, string language)
        {
            List<IntentTrainingPhrase> trainingPhrases = new List<IntentTrainingPhrase>();

            LocalizedIntent localizedIntent = this.GetLocalizedIntentForLanguage(intent, language);

            if ( localizedIntent == null )
            {
                throw new Exception($"Intent: {intent.Name} could not resolve LocalizedIntent for locale: {language}.");
            }

            // Should validate language here
            if ( localizedIntent.Utterances != null )
            {
                foreach (string utterance in localizedIntent.Utterances)
                {
                    IntentTrainingPhrase trainingPhrase = new IntentTrainingPhrase();
                    trainingPhrase.Data = this.BuildTrainingPhrasePartList(intent, slots, utterance);

                    trainingPhrase.IsTemplate = false;
                    trainingPhrase.Count = 0;
                    trainingPhrase.Updated = 0;

                    trainingPhrases.Add(trainingPhrase);
                }
            }

            return trainingPhrases;
        }

        private List<TrainingPhrasePart> BuildTrainingPhrasePartList(StoryEngine.Models.Story.Intent intent, List<SlotType> slots, string utterance )
        {
            List<TrainingPhrasePart> lstParts = new List<TrainingPhrasePart>();

            string parsedUtterance = utterance;

            while(!String.IsNullOrEmpty(parsedUtterance))
            {
                int nSlotStart = 0;

                if ( ( nSlotStart = parsedUtterance.IndexOf( '{' ) ) != -1 )
                {
                    // Add the part up to the start of the slot
                    string beforeSlot = parsedUtterance.Substring(0, nSlotStart);
                    parsedUtterance = parsedUtterance.Substring(nSlotStart + 1);

                    // If we had a string before the slot, add that part.
                    if ( !String.IsNullOrEmpty(beforeSlot) )
                    {
                        TrainingPhrasePart phrasePart = new TrainingPhrasePart();

                        phrasePart.Text = beforeSlot;
                        phrasePart.UserDefined = true;

                        lstParts.Add(phrasePart);
                    }

                    // Now see if the slot name matches one of our slot mappings.
                    int nSlotEnd = parsedUtterance.IndexOf('}');

                    if ( nSlotEnd == -1 )
                    {
                        throw new Exception($"Utterance: '{utterance}' for intent: {intent.Name} missing closing brace for slot.");
                    }

                    // Extract the slot name and strip the slot from the parsed string
                    string slotName = parsedUtterance.Substring(0, nSlotEnd);
                    parsedUtterance = parsedUtterance.Substring(nSlotEnd + 1);

                    // Find the mapping based on the slot name alias in the string
                    if ( intent.SlotMappingsByName == null || !intent.SlotMappingsByName.ContainsKey(slotName) )
                    {
                        throw new Exception($"Utterance: '{utterance}' for intent: {intent.Name} slot mapping: {slotName} not found.");
                    }

                    var slotType = intent.SlotMappingsByName[slotName];
                    // Find the actual slot based on the mapping's slot type
                    var slot = slots.FirstOrDefault(x => x.Name.Equals(slotType));

                    bool isPhoneNumberSlot = false;
                    if (slot == null)
                    {
                        // The phone number slot is special cased
                        if ( !(isPhoneNumberSlot = slotType.Equals("WHETSTONE.US_PHONENUMBER") ) )
                        {
                            throw new Exception($"Utterance: '{utterance}' for intent: {intent.Name} slot mapping: {slotName} slot type: {slotType} not found.");
                        }
                    }

                    TrainingPhrasePart part = new TrainingPhrasePart();
                    if (isPhoneNumberSlot )
                    {
                        // Special case phone numbers - those use a system type.
                        part.Text = _phoneNumberExample;
                        part.Alias = slotName;
                        part.Meta = "@sys.phone-number";
                        part.UserDefined = true;
                    }
                    else
                    {
                        // The text for the training phrase can be the first value in the slot
                        part.Text = slot.Values[0].Value;
                        part.Alias = slotName;
                        part.Meta = $"@{slot.Name}";
                        part.UserDefined = true;
                    }

                    lstParts.Add(part);
                }
                else
                {
                    TrainingPhrasePart part = new TrainingPhrasePart();

                    part.Text = parsedUtterance;
                    part.UserDefined = true;

                    lstParts.Add(part);
                    parsedUtterance = String.Empty;
                }

            }

            return lstParts;
        }

        private async Task ImportIntentsAsync(string projectId, List<Whetstone.StoryEngine.Models.Story.Intent> intents, List<SlotType> slotTypes)
        {

            if ((intents?.Any()).GetValueOrDefault())
            {

                var listIntents = await GetListIntentsAsync(projectId);
                var entities = await GetEntityListAsync(projectId);

                foreach (var storyIntent in intents)
                {

                    var curIntent = storyIntent;

                    if(storyIntent.Name.Equals(ReservedIntents.YesIntent.Name))
                    {
                        curIntent = ReservedIntents.YesIntent;

                    }
                    else if (storyIntent.Name.Equals(ReservedIntents.NoIntent.Name))
                    {
                        curIntent = ReservedIntents.NoIntent;

                    }


                    foreach (LocalizedIntent locIntent in curIntent.LocalizedIntents)
                    {
                        Intent googleIntent = new Intent();
                        googleIntent.Events.Add("actions_intent_NO_INPUT");
                        googleIntent.Action = "no.input";

                        googleIntent.DisplayName = storyIntent.Name;

                        ContextName outContext = new ContextName(projectId, "-", $"{storyIntent.Name}-FollowUp") ;

                       googleIntent.OutputContexts.Add( new Context { ContextName = outContext,
                        LifespanCount=5 }  );


                        bool intentHasSlots = (storyIntent.SlotMappingsByName?.Keys?.Any()).GetValueOrDefault(false);
;

                        googleIntent.WebhookState = intentHasSlots ? Intent.Types.WebhookState.EnabledForSlotFilling :
                                                    Intent.Types.WebhookState.Enabled;

                        var trainingPhrases = GetTrainingPhrases(locIntent, storyIntent.SlotMappingsByName, slotTypes);

                        googleIntent.TrainingPhrases.AddRange(trainingPhrases);

                        //  googleIntent.TrainingPhrases.Add( new Intent.Types.TrainingPhrase)


                        if ((storyIntent.SlotMappingsByName?.Any()).GetValueOrDefault(false))
                        {
                            foreach (string key in storyIntent.SlotMappingsByName.Keys)
                            {
                                string slotName = storyIntent.SlotMappingsByName[key];
                                var foundEntity = await entities.FirstOrDefault(x => x.DisplayName.Equals(slotName, StringComparison.OrdinalIgnoreCase));

                                if (foundEntity == null)
                                {
                                    // Check if the entity is a system entity 

                                    if(key.Equals(WhetstoneIntents.US_PHONENUMBER_INTENT, StringComparison.OrdinalIgnoreCase))
                                    {
                                        Intent.Types.Parameter entityParam = new Intent.Types.Parameter();
                                        entityParam.DisplayName = "phone-number";
                                        entityParam.EntityTypeDisplayName = "@sys.phone-number";
                                        entityParam.Value = "$phone-number";
                                        entityParam.Mandatory = true;
                                        // entityParam.Name = foundEntity.Name;
                                        googleIntent.Parameters.Add(entityParam);
                                    }
                                    else
                                        _logger.LogError($"Slot mapping {slotName} not resolved for intent {storyIntent.Name}");
                                }
                                else
                                {
                                    Intent.Types.Parameter entityParam = new Intent.Types.Parameter();
                                    entityParam.DisplayName = foundEntity.DisplayName;
                                    entityParam.EntityTypeDisplayName = $"@{foundEntity.DisplayName}";
                                    entityParam.Value = $"${foundEntity.DisplayName}";
                                    entityParam.Mandatory = true;
                                    // entityParam.Name = foundEntity.Name;
                                    googleIntent.Parameters.Add(entityParam);
                                }
                            }

                        }
                        Thread.Sleep(1100);
                        await CreateOrUpdateIntentAsync(projectId, googleIntent, listIntents);

                    }


                }


            }

        }


        private List<Intent.Types.TrainingPhrase> GetTrainingPhrases(LocalizedIntent locIntent, Dictionary<string, string> slotNameMap, List<SlotType> slotTypes)
        {
            List<Intent.Types.TrainingPhrase> generatedPhrases = new List<Intent.Types.TrainingPhrase>();

            if ((locIntent?.Utterances?.Any()).GetValueOrDefault(false))
            {
                foreach (string utterance in locIntent.Utterances)
                {

                    List<Match> slotMatches = Regex.Matches(utterance, "{(.*?)}").Cast<Match>().ToList();
                    string formattedUtterance = utterance;
                    List<SlotType> mappedSlots = new List<SlotType>();
                    foreach (Match foundMatch in slotMatches)
                    {

                        string matchedVal = foundMatch.Value.Substring(1, foundMatch.Length - 2);

                        if (slotNameMap.ContainsKey(matchedVal))
                        {
                            string slotName = slotNameMap[matchedVal];

                            // Retrieve the slot list
                            SlotType sType = slotTypes.FirstOrDefault(x => x.Name.Equals(slotName));
                            if (sType != null)
                            {
                                formattedUtterance = formattedUtterance.Replace(foundMatch.Value, string.Concat("{", sType.Name, "}"));
                                mappedSlots.Add(sType);
                            }
                            else
                            {
                                if (slotName.Equals(WhetstoneIntents.US_PHONENUMBER_INTENT))
                                {
                                    formattedUtterance = formattedUtterance.Replace(foundMatch.Value, string.Concat("{", WhetstoneIntents.US_PHONENUMBER_INTENT, "}"));

                                    SlotType phoneSlot = new SlotType();
                                    phoneSlot.Name = WhetstoneIntents.US_PHONENUMBER_INTENT;
                                    mappedSlots.Add(phoneSlot);
                                }
                                else
                                {

                                    string error = $"Slot name {slotName} not found for {utterance}";
                                    _logger.LogError(error);
                                    throw new Exception(error);
                                }
                            }
                        }
                        else
                        {
                            string error = $"Slot not found for alias {matchedVal} for utterance {utterance}";
                            _logger.LogError(error);
                            throw new Exception(error);
                        }

                    }

                    List<string> results = new List<string>();
                    AddCombinations(results, mappedSlots, 0, formattedUtterance);

                    // Convert the training phrase

                    int maxIndex = results.Count > 2000 ? 2000 : results.Count;

                    for (int i = 0; i < maxIndex; i++)
                    {
                        string result = results[i];
                        List<UtterancePart> utParts = ParseUtterance(result);
                        Intent.Types.TrainingPhrase trainingPhrase = new Intent.Types.TrainingPhrase();

                        foreach (UtterancePart utPart in utParts)
                        {
                            Intent.Types.TrainingPhrase.Types.Part trainingPart = new Intent.Types.TrainingPhrase.Types.Part();
                            trainingPart.UserDefined = true;
                            if (utPart.Text != null)
                            {
                                trainingPart.Text = utPart.Text;
                            }
                            else if (utPart.SlotName != null)
                            {
                                string[] slotParts = utPart.SlotName.Split(':');

                                string slotName = slotParts[0];

                                trainingPart.Text = slotParts[1];

                                if (slotName.Equals(WhetstoneIntents.US_PHONENUMBER_INTENT))
                                {

                                    //EntityTypeDisplayName @sys.phone-number
                                    //Displayname phone-number
                                    //Value $phone - number

                                    trainingPart.Alias = slotName;
                                    trainingPart.EntityType = "@sys.phone-number";
                                }
                                else
                                {
                                    trainingPart.Alias = slotParts[0];
                                    trainingPart.EntityType = $"@{slotParts[0]}";
                                }
                            }


                            trainingPhrase.Parts.Add(trainingPart);
                        }

                        generatedPhrases.Add(trainingPhrase);
                    }


                    //  List<UtterancePart> utteranceParts = ParseUtterance(utterance);


                }
            }


            if (generatedPhrases.Count > 2000)
            {
                generatedPhrases.RemoveRange(2000, generatedPhrases.Count - 2000);

            }


            return generatedPhrases;

        }


        private static string GetRandomTelNo()
        {

            StringBuilder telNo = new StringBuilder(12);
            int number;
            for (int i = 0; i < 3; i++)
            {
                number = StaticRandom.Next(0, 8); // digit between 0 (incl) and 8 (excl)
                telNo = telNo.Append(number.ToString());
            }
            telNo = telNo.Append("-");
            number = StaticRandom.Next(0, 743); // number between 0 (incl) and 743 (excl)
            telNo = telNo.Append($"{number:D3}");
            telNo = telNo.Append("-");
            number = StaticRandom.Next(0, 10000); // number between 0 (incl) and 10000 (excl)
            telNo = telNo.Append($"{number:D4}");
            return telNo.ToString();
        }


        private static void AddCombinations(List<string> result, List<SlotType> levels, int level, string path)
        {
            if (level >= levels.Count)
            {
                result.Add(path);
                return;
            }

            SlotType curSlot = levels[level];

            if(curSlot.Name.Equals(WhetstoneIntents.US_PHONENUMBER_INTENT))
            {
                string pathVal = null;
                
                if (path != null)
                {
                    string itemVal = GetRandomTelNo();

                    pathVal = path.Replace(string.Concat("{", curSlot.Name, "}"),
                        string.Concat("{", curSlot.Name, ":", itemVal, "}"));

                }

                AddCombinations(result, levels, level + 1, pathVal);
            }
            else
                foreach (var item in curSlot.Values)
                {
                    string pathVal = null;

                    if (path != null)
                    {
                        pathVal = path.Replace(string.Concat("{", curSlot.Name, "}"),
                            string.Concat("{", curSlot.Name, ":", item.Value, "}"));

                    }

                    AddCombinations(result, levels, level + 1, pathVal);
                }
        }


        private List<UtterancePart> ParseUtterance(string utterance)
        {

            List<UtterancePart> utteranceParts = new List<UtterancePart>();

            bool isInBrackets = false;


            UtterancePart curPart = default(UtterancePart);
            for (int i = 0; i < utterance.Length; i++)
            {
                char curChar = utterance[i];

                if (curChar == '{')
                {
                    isInBrackets = true;

                    if (!string.IsNullOrWhiteSpace(curPart.SlotName) || curPart.Text?.Length > 0)
                        utteranceParts.Add(curPart);


                    i++;
                    if (i < utterance.Length)
                    {
                        curChar = utterance[i];
                        curPart = new UtterancePart();
                    }
                }
                else if (curChar == '}')
                {
                    isInBrackets = false;
                    utteranceParts.Add(curPart);
                    i++;
                    if (i < utterance.Length)
                    {
                        curChar = utterance[i];
                        curPart = new UtterancePart();
                    }
                }


                if (isInBrackets)
                {
                    if (curPart.SlotName == null)
                    {
                        curPart.SlotName = "";
                    }
                    curPart.SlotName += curChar;

                }
                else
                {
                    if (curPart.Text == null)
                    {
                        curPart.Text = "";
                    }

                    if (curChar != '}')
                        curPart.Text += curChar;
                }

            }

            if (curPart.Text != null && curPart.Text.Length > 0)
            {
                utteranceParts.Add(curPart);
            }
            return utteranceParts;
        }


        protected async Task CreateOrUpdateIntentAsync(string projectId, Intent importedIntent, PagedEnumerable<ListIntentsResponse, Intent> existingIntents)
        {
            var intentClient = await IntentsClient.CreateAsync();

            // Get the phone intent
            //var phoneIntent = existingIntents.FirstOrDefault(x => x.DisplayName.Equals("PhoneNumberIntent", StringComparison.OrdinalIgnoreCase));

            var foundIntent = existingIntents.FirstOrDefault(x => x.DisplayName.Equals(importedIntent.DisplayName, StringComparison.OrdinalIgnoreCase));

            if (foundIntent == null)
            {
                // Create the intent
                CreateIntentRequest createIntentReq = new CreateIntentRequest();
                createIntentReq.Parent = GetParentName(projectId);
                createIntentReq.Intent = importedIntent;
                try
                {
                    var createResponse = await intentClient.CreateIntentAsync(createIntentReq);
                    _logger.LogInformation($"Created entity {createResponse.DisplayName} Name: {createResponse.Name}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error creating entity {importedIntent.DisplayName}");

                }
            }
            else
            {
                UpdateIntentRequest updateIntentReq = new UpdateIntentRequest();
                importedIntent.Name = foundIntent.Name;
                updateIntentReq.Intent = importedIntent;
                try
                {
                    var updateResponse = await intentClient.UpdateIntentAsync(updateIntentReq);
                    _logger.LogInformation($"Updated intent {updateIntentReq.Intent.DisplayName} Name: {updateIntentReq.Intent.Name}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating intent {importedIntent.DisplayName}");

                }

            }

        }

        protected async Task ImportSlotTypesAsync(string projectId, List<SlotType> slotTypes)
        {

            if((slotTypes?.Any()).GetValueOrDefault())
            {
                string parentName = GetParentName(projectId);

                // import the slot types into the google project

                var entityList = await GetEntityListAsync(projectId);
             
                foreach (SlotType slotType in slotTypes)
                {
                    CreateEntityTypeRequest createReq = new CreateEntityTypeRequest();
                    createReq.Parent = parentName;

                    EntityType entityType = new EntityType();
                    entityType.Kind = EntityType.Types.Kind.Map;
                    //entityType.EntityTypeName = new EntityTypeName(projectId, slotType.Name);
                    entityType.DisplayName = slotType.Name;

                    if ((slotType.Values?.Any()).GetValueOrDefault(false))
                    {
                        foreach (var slotVal in slotType.Values)
                        {
                            var ent = new EntityType.Types.Entity();

                            ent.Value = slotVal.Value;

                            ent.Synonyms.Add(slotVal.Value);
                            if ((slotVal.Synonyms?.Any()).GetValueOrDefault(false))
                            {
                                ent.Synonyms.AddRange(slotVal.Synonyms);
                            }
                            entityType.Entities.Add(ent);
                        }
                    }

                    Thread.Sleep(3000);
                    await CreateOrUpdateEntityAsync(projectId, entityType, entityList);

                    createReq.EntityType = entityType;
                }
            }
        }

        private async Task CreateOrUpdateEntityAsync(string projectId, EntityType entityType, PagedAsyncEnumerable<ListEntityTypesResponse, EntityType> entityList)
        {

           var foundEntity = await entityList.FirstOrDefault(x => x.DisplayName.Equals(entityType.DisplayName, StringComparison.OrdinalIgnoreCase));

            var entityClient = await EntityTypesClient.CreateAsync();

            if(foundEntity==null)
            {
                CreateEntityTypeRequest createRequest = new CreateEntityTypeRequest();
                createRequest.EntityType = entityType;
                createRequest.Parent = GetParentName(projectId);
                try
                {

                    var createResponse = await entityClient.CreateEntityTypeAsync(createRequest);
                    _logger.LogInformation($"Created entity {entityType.DisplayName} Name: {createResponse.Name}");
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, $"Error creating entity {entityType.DisplayName}");

                }
            }
            else
            {
                UpdateEntityTypeRequest updateRequest = new UpdateEntityTypeRequest();
                entityType.Name = foundEntity.Name;
                updateRequest.EntityType = entityType;
                try
                {
                    var updateResponse = await entityClient.UpdateEntityTypeAsync(updateRequest);
                    _logger.LogInformation($"Updated entity {entityType.DisplayName}, Name:{entityType.Name}");
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, $"Error updating entity {entityType.DisplayName} with name {entityType.Name}");
                }
            }

        }

        private string GetParentName(string projectId)
        {

            return $"projects/{projectId}/agent";
        }


        private async Task<PagedAsyncEnumerable<ListEntityTypesResponse, EntityType>> GetEntityListAsync(string projectId)
        {
            var client = await EntityTypesClient.CreateAsync();
            ListEntityTypesRequest listRequest = new ListEntityTypesRequest();
            listRequest.Parent = GetParentName(projectId);
            var entityList = client.ListEntityTypesAsync(listRequest);
            return entityList;

        }
    }
}
