//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using Npgsql;
//using Whetstone.StoryEngine.Models;
//using Whetstone.StoryEngine.Models.Conditions;
//using Whetstone.StoryEngine.Models.Data;
//using Whetstone.StoryEngine.Models.Story;
//using Microsoft.Extensions.Logging;
//using Whetstone.StoryEngine;

//namespace Whetstone.StoryEngine.Data.EntityManager
//{
//    public class EntityTitleRepository : IEntityTitleRepository
//    {

//       // private StoryEngineContext _engineContext;

//        private DbContextOptions<StoryEngineContext> _contextOptions;

//        public EntityTitleRepository(DbContextOptions<StoryEngineContext> contextOptions, ILogger logger)
//        {

//            _contextOptions = contextOptions;
//            //  _engineContext = engineContext;
//            Logger = logger;

//        }

//        public EntityTitleRepository(DbContextOptions<StoryEngineContext> contextOptions)
//        {

//            _contextOptions = contextOptions;
//            //  _engineContext = engineContext;
            
//            Logger =  StoryEngineLogFactory.CreateLogger<EntityTitleRepository>();

//        }

//        private ILogger Logger { get; set; }


//        public async Task<DataTitleVersion> UpdateOrCreateVersionAsync(string shortName, string version, DataTitleVersion versionModel)
//        {
//            DataTitleVersion ver = new DataTitleVersion();


//            using (var context = new StoryEngineContext(_contextOptions))
//            {

//                try
//                {

//                    var identifiers = context.StoryTitles.Where(x => x.ShortName.Equals(shortName, StringComparison.OrdinalIgnoreCase)).Select(x => x.Id);

//                    if (identifiers.Any())
//                    {
//                        versionModel.StoryId = identifiers.FirstOrDefault().Value;
//                    }


//                    context.StoryVersions.AddOrUpdate( versionModel);


//                    //List<DataInventoryConditionXRef> invConditionXRefs =
//                    //    context.InventoryConditionXRefs.Join(context.InventoryConditions,
//                    //            xref => xref.ConditionId, cond => cond.Id,
//                    //            (xref, cond) => new { XRef = xref, Condition = cond })
//                    //        .Where(x => x.Condition.VersionId == versionModel.Id)
//                    //        .Select(xout => xout.XRef).ToList();

//                    //context.InventoryConditionXRefs.RemoveRange(invConditionXRefs);

//                    foreach (DataInventoryItem invItem in versionModel.InventoryItems)
//                    {
//                        invItem.StoryVersion = versionModel;
//                        invItem.VersionId = versionModel.Id.GetValueOrDefault(0);
//                        context.InventoryItems.Add(invItem);
//                    }


//                    foreach (DataSlotType slotType in versionModel.Slots)
//                    {
//                        slotType.VersionId = versionModel.Id;
//                        context.SlotTypes.AddOrUpdate(slotType);
//                    }


//                    List<DataIntentSlotMapping> sentMappings = new List<DataIntentSlotMapping>();
//                    foreach (DataIntent submittedIntent in versionModel.Intents)
//                    {
//                        submittedIntent.VersionId = versionModel.Id;
//                        context.Intents.AddOrUpdate(submittedIntent);

//                        if ((submittedIntent.SlotTypeMappings?.Any()).GetValueOrDefault(false))
//                        {

//                            foreach (DataIntentSlotMapping intentSlotMap in submittedIntent.SlotTypeMappings)
//                            {
//                                intentSlotMap.Intent = submittedIntent;
//                                context.MappedIntents.AddOrUpdate(intentSlotMap);
//                                sentMappings.Add(intentSlotMap);
//                            }
//                        }
//                    }


//                    List<DataInventoryConditionXRef> invConditionMappings = new List<DataInventoryConditionXRef>();
//                    foreach (DataInventoryCondition invCondition in versionModel.DataInventoryConditions)
//                    {
//                        context.InventoryConditions.Add(invCondition);

//                        if ((invCondition.InventoryConditionXRefs?.Any()).GetValueOrDefault(false))
//                        {
//                            // ReSharper disable once PossibleNullReferenceException
//                            foreach (DataInventoryConditionXRef invXRef in invCondition.InventoryConditionXRefs)
//                            {

                              
//                                context.InventoryConditionXRefs.AddOrUpdate(invXRef);
//                            }
//                            // ReSharper disable once AssignNullToNotNullAttribute
//                            invConditionMappings.AddRange(invCondition.InventoryConditionXRefs);
//                        }
//                    }


//                    // Delete missing Inventory Joins


//                    List<DataInventoryConditionXRef> invConditionXRefs =
//                        context.InventoryConditionXRefs.Join(context.InventoryConditions,
//                                xref => xref.ConditionId, cond => cond.Id,
//                                (xref, cond) => new { XRef = xref, Condition = cond })
//                            .Where(x => x.Condition.VersionId == versionModel.Id)
//                            .Select(xout => xout.XRef).ToList();

//                    List<DataInventoryConditionXRef> missingInvMappings = new List<DataInventoryConditionXRef>();
//                    foreach (DataInventoryConditionXRef dbMapping in invConditionXRefs)
//                    {
//                        DataInventoryConditionXRef foundMapping = invConditionMappings.Find(x =>
//                            x.ConditionId == dbMapping.ConditionId && x.InventoryItemId == dbMapping.InventoryItemId);

//                        // Add the mapping to a list to delete
//                        if (foundMapping == null)
//                        {
//                            missingInvMappings.Add(dbMapping);
//                        }

//                    }
//                    if (missingInvMappings.Any())
//                        context.InventoryConditionXRefs.RemoveRange(missingInvMappings);

//                    //db.Services.Join(db.ServiceAssignments,
//                    //        s => s.Id,
//                    //        sa => sa.ServiceId,
//                    //        (s, sa) => new { service = s, asgnmt = sa })
//                    //    .Where(ssa => ssa.asgnmt.LocationId == 1)
//                    //    .Select(ssa => ssa.service);

//                    List<DataSlotType> slotTypes = context.SlotTypes.Where(x => x.VersionId == versionModel.Id).ToList();

//                    DeleteMissingEntities<DataSlotType>(context.SlotTypes, versionModel.Slots, slotTypes);


//                    List<DataIntent> dbIntents = context.Intents.Where(x => x.VersionId == versionModel.Id).ToList();

//                    DeleteMissingEntities<DataIntent>(context.Intents, versionModel.Intents, dbIntents);

//                    List<DataIntentSlotMapping> deletedMappings = new List<DataIntentSlotMapping>();
//                    List<DataIntentSlotMapping> slotMappings = context.MappedIntents.Where(x => x.Intent.VersionId == versionModel.Id).ToList();
//                    foreach (DataIntentSlotMapping dbMapping in slotMappings)
//                    {
//                        DataIntentSlotMapping foundMapping = sentMappings.Find(x =>
//                            x.SlotTypeId == dbMapping.SlotTypeId && x.IntentId == dbMapping.IntentId);

//                        // Add the mapping to a list to delete
//                        if (foundMapping == null)
//                        {
//                            deletedMappings.Add(dbMapping);
//                        }

//                    }
//                    if(deletedMappings.Any())
//                        context.MappedIntents.RemoveRange(deletedMappings);

//                  //  DeleteMissingEntities<DataIntentSlotMapping>(context.MappedIntents, sentMappings, slotMappings);


//                    await context.SaveChangesAsync();
//                }
//                catch (Exception ex)
//                {

//                    Logger.LogError(ex, "Error saving story version");
//                    throw;

//                }

//            }



//            return ver;
//        }

//        private static void DeleteMissingEntities<T>(DbSet<T> entitySet, List<T> sourceModels, List<T> dbModels) 
//                                                                where T : class, IStoryDataItem
                                                               
//        {
//            List<T> removeSlots = new List<T>();
//            foreach (T dbSlot in dbModels)
//            {
//                var foundSlot = sourceModels.FirstOrDefault(x => dbSlot.Id == x.Id);
//                if (foundSlot == null)
//                    removeSlots.Add(dbSlot);
//            }

//            if (removeSlots.Any())
//                entitySet.RemoveRange(removeSlots);


//        }

//        public async Task CreateTitleAsync(DataTitle entity)
//        {

//            using (var context = new StoryEngineContext(_contextOptions))
//            {

//                await context.StoryTitles.AddAsync(entity);
//                await context.SaveChangesAsync();
//            }
//        }

//        public void DeleteTitleAsync(DataTitle entity)
//        {
//            using (var context = new StoryEngineContext(_contextOptions))
//            {
//                context.StoryTitles.Remove(entity);

//                context.SaveChanges();
//            }
//        }


//        public async Task<List<DataTitle>> GetTitles()
//        {
//            using (var context = new StoryEngineContext(_contextOptions))
//            {
//                 await context.StoryTitles.LoadAsync<DataTitle>();


//                List<DataTitle> titles = await context.StoryTitles.ToListAsync();


//                return titles;
//            }

//        }


//        public async Task AddLanguageMappingsAsync(List<DataIntentSlotMapping> intents)
//        {
//            using (var context = new StoryEngineContext(_contextOptions))
//            {
//                await context.MappedIntents.AddRangeAsync(intents);

//                await context.SaveChangesAsync();
//            }
//        }
        

//        public async Task<StoryNode> GetBadIntentNodeAsync(string titleId, int badIntentCount)
//        {
//            throw new NotImplementedException();
//        }

//        public async Task<DataTitle> GetByShortNameAsync(string shortName)
//        {

//            using (var context = new StoryEngineContext(_contextOptions))
//            {
//                // .Where(st => st.ShortName.Equals(shortName))
//                //  .Include(st => st.Intents)

//                DataTitle title = await context.StoryTitles
//                  .Include(x=> x.Versions)    
                
//                    .SingleOrDefaultAsync(st =>
//                        st.ShortName.Equals(shortName, StringComparison.OrdinalIgnoreCase));

//                return title;
//            }
            
//        }



      


//        public  async Task<DataTitleVersion> GetCurrentTitleVersionAsync(string titleId, string version)
//        {
//            using (var context = new StoryEngineContext(_contextOptions))
//            {
//                DataTitleVersion storyVersion = null;


//                using (var nativeCon = new Npgsql.NpgsqlConnection(context.Database.GetDbConnection().ConnectionString))
//                {


//                    await nativeCon.OpenAsync();
//                    using (Npgsql.NpgsqlTransaction tran = nativeCon.BeginTransaction())
//                    {
//                        //  .Include(st => st.Intents)
//                        var cmd = new Npgsql.NpgsqlCommand("getversion", nativeCon);


//                        // Run the sproc 
//                        cmd.CommandType = CommandType.StoredProcedure;
//                        cmd.Parameters.Add(new Npgsql.NpgsqlParameter("shortname", NpgsqlTypes.NpgsqlDbType.Text)
//                        { Value = titleId });
//                        cmd.Parameters.Add(new Npgsql.NpgsqlParameter("version", NpgsqlTypes.NpgsqlDbType.Text)
//                        { Value = version });


//                        await cmd.ExecuteNonQueryAsync();
//                        string fetchCommands = null;
//                        using (var cursorReader = cmd.ExecuteReader())
//                        {
//                            var sb = new StringBuilder();
//                            while (cursorReader.Read())
//                            {
//                                for (int i = 0; i < cursorReader.FieldCount; i++)
//                                {
//                                    sb.AppendFormat(@"FETCH ALL FROM ""{0}"";", cursorReader.GetString(i));
//                                    sb.AppendLine();
//                                }
//                            }
//                            fetchCommands = sb.ToString();
//                        }

//                        using (var cursorCommand = new Npgsql.NpgsqlCommand(fetchCommands, nativeCon))
//                        {


//                            using (NpgsqlDataReader fetchReader = cursorCommand.ExecuteReader())
//                            {

//                                storyVersion = LoadStoryVersion(fetchReader);

//                                if (storyVersion != null)
//                                {
                                  
//                                    LoadSlots(storyVersion, fetchReader);

//                                    // Get the intents
//                                    LoadIntents(storyVersion, fetchReader);

//                                    // Get the intent slot mappings
//                                    LoadSlotMappings(storyVersion, fetchReader);
                                    
//                                    LoadChapterWithTitles(storyVersion, fetchReader);

//                                    Tuple<List<DataNode>, List<DataNode>> nodeTuple = LoadNodes(storyVersion, fetchReader);
//                                    List<DataNode> standardNodes = nodeTuple.Item1;
//                                    List<DataNode> storyNodes = nodeTuple.Item2;

//                                    LoadStandardNodes(storyVersion,  standardNodes, fetchReader);

//                                    // Load Conditions and cross references.
//                                    LoadNodeVisitConditions(storyVersion,storyNodes,fetchReader);
                                    
//                                    LoadNodeInventoryConditions(storyVersion, fetchReader);

//                                    LoadLocalizedResponses(storyVersion,storyNodes, fetchReader);

//                                }
//                            }
//                        }
//                    }
//                }

//                return storyVersion;
//            }
       
//        }



//        //inventoryConditions refcursor; 13
//        //inventoryItems refcursor; 14
//        //inventoryItemXrefs refcursor; 15
//        private static void LoadNodeInventoryConditions(DataTitleVersion storyVersion, Npgsql.NpgsqlDataReader reader)
//        {

//            while (reader.Read())
//            {
//                //cv."Id", cv."Name", cv."RequiredOutcome"
//                DataInventoryCondition invCondition = new DataInventoryCondition();
//                invCondition.Id = reader.GetInt64(0);
//                invCondition.Name = reader.GetString(1);
//                invCondition.RequiredOutcome = reader.GetBoolean(2);

//                if (storyVersion.DataInventoryConditions == null)
//                    storyVersion.DataInventoryConditions = new List<DataInventoryCondition>();

//                storyVersion.DataInventoryConditions.Add(invCondition);

//            }

//            reader.NextResult();

//            if ((storyVersion.DataInventoryConditions?.Any()).GetValueOrDefault(false))
//            {

//                while (reader.Read())
//                {
//                    //i."Id", i."Name", i."IsMultiItem"
//                    DataInventoryItem invItem = new DataInventoryItem();
//                    invItem.Id = reader.GetInt64(0);
//                    invItem.Name = reader.GetString(1);
//                    invItem.IsMultiItem = reader.GetBoolean(2);

//                    if (storyVersion.InventoryItems == null)
//                        storyVersion.InventoryItems = new List<DataInventoryItem>();

//                    storyVersion.InventoryItems.Add(invItem);
//                }

//                reader.NextResult();

//                if ((storyVersion.InventoryItems?.Any()).GetValueOrDefault(false))
//                {

//                    long conditionId;
//                    long itemId;

//                    while (reader.Read())
//                    {
//                        //icx."ConditionId", icx."ItemId" 
//                        conditionId = reader.GetInt64(0);
//                        itemId = reader.GetInt64(1);
//                        DataInventoryCondition foundInvCondition = storyVersion.DataInventoryConditions.FirstOrDefault(x => x.Id == conditionId);
//                        if (foundInvCondition != null)
//                        {
//                            DataInventoryItem foundItem = storyVersion.InventoryItems.FirstOrDefault(x => x.Id == itemId);
//                            if (foundItem != null)
//                            {
//                                DataInventoryConditionXRef invCondItem = new DataInventoryConditionXRef();
//                                invCondItem.Condition = foundInvCondition;
//                                invCondItem.ConditionId = foundInvCondition.Id.GetValueOrDefault(0);
//                                invCondItem.InventoryItem = foundItem;
//                                invCondItem.InventoryItemId = foundItem.Id.GetValueOrDefault(1);



//                                if(foundInvCondition.InventoryConditionXRefs==null)
//                                    foundInvCondition.InventoryConditionXRefs = new List<DataInventoryConditionXRef>();

//                                if(foundItem.InventoryConditionXRefs ==null)
//                                    foundItem.InventoryConditionXRefs = new List<DataInventoryConditionXRef>();

//                                foundItem.InventoryConditionXRefs.Add(invCondItem);

//                                foundInvCondition.InventoryConditionXRefs.Add(invCondItem);
                                
//                            }


//                        }
//                    }

//                }

//                reader.NextResult();

//            }
//            else 
//            {
//                reader.NextResult();
//                reader.NextResult();
//            }



//        }


//        private static void LoadNodeVisitConditions(DataTitleVersion storyVersion, List<DataNode> storyNodes, Npgsql.NpgsqlDataReader reader)
//        {

//            while (reader.Read())
//            {
//                DataNodeVisitCondition visitCondition = new DataNodeVisitCondition();
//                // cnv."Id", cnv."Name", cnv."RequiredOutcome"
//                visitCondition.Id = reader.GetInt64(0);
//                visitCondition.Name = reader.GetString(1);
//                visitCondition.RequiredOutcome = reader.GetBoolean(2);

//                if (storyVersion.DataNodeVisitConditions == null)
//                    storyVersion.DataNodeVisitConditions = new List<DataNodeVisitCondition>();

//                storyVersion.DataNodeVisitConditions.Add(visitCondition);

//            }


//            reader.NextResult();

//            if ((storyVersion.DataNodeVisitConditions?.Any()).GetValueOrDefault(false) &&
//                (storyNodes?.Any()).GetValueOrDefault(false))
//            {

//                long conditionId;
//                long nodeId;
//                while (reader.Read())
//                {
//                    //nvx."ConditionId", nvx."NodeId"
//                    conditionId = reader.GetInt64(0);
//                    nodeId = reader.GetInt64(1);

//                    DataNodeVisitCondition foundNodeVisitCondition = storyVersion.DataNodeVisitConditions.FirstOrDefault(x => x.Id == conditionId);
//                    if (foundNodeVisitCondition != null)
//                    {
//                        DataNode foundNode = storyNodes.FirstOrDefault(x => x.Id == nodeId);
//                        if (foundNode != null)
//                        {


//                           // foundNodeVisitCondition.Nodes.Add(foundNode);
//                        }
//                    }
//                }

//            }

//            reader.NextResult();
//        }

//        private static void LoadLocalizedResponses(DataTitleVersion storyVersion, List<DataNode> storyNodes, Npgsql.NpgsqlDataReader reader)
//        {


//            List<DataLocalizedResponseSet> responseSets = new List<DataLocalizedResponseSet>();


//            while (reader.Read())
//            {
//                DataLocalizedResponseSet localizedResponseSet = new DataLocalizedResponseSet();
//                localizedResponseSet.Id = reader.GetInt64(0);
//                long parentNodeId = reader.GetInt64(1);

//                DataNode parentNode = storyNodes.FirstOrDefault(x => x.Id == parentNodeId);
//                if (parentNode.ResponseSet == null)
//                    parentNode.ResponseSet = new List<DataLocalizedResponseSet>();

//                parentNode.ResponseSet.Add(localizedResponseSet);

//                responseSets.Add(localizedResponseSet);
//            }

//            reader.NextResult();


//            List<DataLocalizedResponse> locResponses = new List<DataLocalizedResponse>();



//            while (reader.Read())
//            {
//                DataLocalizedResponse locResponse = new DataLocalizedResponse();
//                //                 locResp."Id", locResp."CardTitle", locResp."DataLocalizedResponseSetId", locResp."LargeImageFile",
//                //  locResp."Locale", locResp."RepromptTextResponse", locResp."SendCardResponse", locResp."SmallImageFile"
//                locResponse.Id = reader.GetInt64(0);
//                locResponse.CardTitle = reader.IsDBNull(1) ? null : reader.GetString(1);
//                long parentSetId = reader.GetInt64(2);
//                locResponse.LargeImageFile = reader.IsDBNull(3) ? null : reader.GetString(3);
//                locResponse.Locale = reader.IsDBNull(4) ? null : reader.GetString(4);
//                locResponse.RepromptTextResponse = reader.IsDBNull(5) ? null : reader.GetString(5);
//                locResponse.SendCardResponse = reader.IsDBNull(6) ? null : (bool?)reader.GetBoolean(6);
//                locResponse.SmallImageFile = reader.IsDBNull(7) ? null : reader.GetString(7);

//                DataLocalizedResponseSet foundResponseSet = responseSets.FirstOrDefault(x => x.Id == parentSetId);
//                if (foundResponseSet.LocalizedResponses == null)
//                    foundResponseSet.LocalizedResponses = new List<DataLocalizedResponse>();

//                foundResponseSet.LocalizedResponses.Add(locResponse);

//                locResponses.Add(locResponse);
//            }

//            reader.NextResult();

//            LoadSpeechFragments(storyVersion, locResponses, reader);

//        }


//        private static void LoadSpeechFragments(DataTitleVersion storyVersion,  List<DataLocalizedResponse> locResponses, Npgsql.NpgsqlDataReader reader)
//        {
//            List<DataClientSpeechFragments> clientFragments = new List<DataClientSpeechFragments>();


//            while (reader.Read())
//            {
//                long? parentResponseId = default(long?);
//                long? parentRepromptId = default(long?);
//                // OPEN clientFrags FOR SELECT csf."Id", csf."SpeechClient", csf."LocResponseId", csf."LocRepromptResponseId"
//                DataClientSpeechFragments clientFragment = new DataClientSpeechFragments();

//                clientFragment.Id = reader.GetInt64(0);
//                clientFragment.SpeechClient = reader.IsDBNull(1) ? null : (Client?)reader.GetInt32(1);
//                parentResponseId = reader.IsDBNull(2) ? null : (long?)reader.GetInt64(2);
//                parentRepromptId = reader.IsDBNull(3) ? null : (long?)reader.GetInt64(3);

//                clientFragments.Add(clientFragment);

//                if (parentResponseId.HasValue)
//                {
//                    DataLocalizedResponse foundResponse = locResponses.FirstOrDefault(x => x.Id == parentResponseId.Value);
//                    if (foundResponse != null)
//                    {

//                        if (foundResponse.SpeechResponses == null)
//                            foundResponse.SpeechResponses = new List<DataClientSpeechFragments>();

//                        foundResponse.SpeechResponses.Add(clientFragment);
//                    }

//                    DataLocalizedResponse foundReprompt = locResponses.FirstOrDefault(x => x.Id == parentRepromptId);

//                    if (foundReprompt != null)
//                    {

//                        if (foundReprompt.RepromptSpeechResponses == null)
//                            foundReprompt.RepromptSpeechResponses = new List<DataClientSpeechFragments>();

//                        foundReprompt.RepromptSpeechResponses.Add(clientFragment);
//                    }
//                }
//            }

//            reader.NextResult();

//            List<DataConditionalFragment> conFragments = new List<DataConditionalFragment>();

//            if (clientFragments.Any())
//            {
//                List<DataSpeechFragment> speechFragments = new List<DataSpeechFragment>();


//                while (reader.Read())
//                {
//                    //id1 (0), clientspeechfragid (1) , trueparentid (2), falseparentid (3), text(4), filename (5), audiourl (6), discriminator (7), comment (8), sequence (9)

//                    string discriminator = reader.GetString(7);
//                    long? parentFragId = reader.IsDBNull(1) ? null : (long?)reader.GetInt64(1);


//                    DataSpeechFragment speechFragment = null;
//                    if (discriminator.Equals("DataSpeechText", StringComparison.OrdinalIgnoreCase))
//                    {
//                        DataSpeechText speechText = new DataSpeechText();
//                        speechText.Text = reader.IsDBNull(4) ? null : reader.GetString(4);
//                        speechText.Voice = reader.IsDBNull(11) ? null : reader.GetString(11);
//                        speechFragment = speechText;
//                    }

//                    //[BsonKnownTypes(typeof(DirectAudioFile))]

//                    //      [BsonKnownTypes(typeof(ConditionalFragment))]
//                    if (discriminator.Equals("DataAudioFile", StringComparison.OrdinalIgnoreCase))
//                    {
//                        DataAudioFile audioFile = new DataAudioFile();
//                        audioFile.FileName = reader.IsDBNull(5) ? null : reader.GetString(5);
//                        speechFragment = audioFile;
//                    }

//                    if (discriminator.Equals("DataDirectAudioFile", StringComparison.OrdinalIgnoreCase))
//                    {
//                        DataDirectAudioFile directAudioFile = new DataDirectAudioFile();
//                        directAudioFile.AudioUrl = reader.IsDBNull(6) ? null : reader.GetString(6);
//                        speechFragment = directAudioFile;
//                    }

//                    if (discriminator.Equals("DataConditionalFragment", StringComparison.OrdinalIgnoreCase))
//                    {
//                        DataConditionalFragment conFragment = new DataConditionalFragment();
//                        conFragments.Add(conFragment);
//                        speechFragment = conFragment;
//                    }

//                    speechFragment.TrueResultParentId = reader.IsDBNull(2) ? null : (long?)reader.GetInt64(2);
//                    speechFragment.FalseResultParentId = reader.IsDBNull(3) ? null : (long?)reader.GetInt64(3);
//                    speechFragment.Sequence = reader.IsDBNull(9) ? 1 : reader.GetInt32(9);
//                    speechFragment.Comment = reader.IsDBNull(8) ? null : reader.GetString(8);
//                    speechFragment.Id = reader.GetInt64(0);

//                    speechFragments.Add(speechFragment);
//                    if (parentFragId.HasValue)
//                    {
//                        DataClientSpeechFragments foundClientFrag = clientFragments.FirstOrDefault(x => x.Id == parentFragId.Value);
//                        if (foundClientFrag != null)
//                        {
//                            if (foundClientFrag.SpeechFragments == null)
//                                foundClientFrag.SpeechFragments = new List<DataSpeechFragment>();

//                            foundClientFrag.SpeechFragments.Add(speechFragment);
//                        }
//                    }
//                } // End of initial speech fragment loading
                

//                // loop through all the loaded fragments and wire up the true and false references
//                foreach (DataSpeechFragment speechFrag in speechFragments)
//                {

//                    speechFrag.StoryVersion = storyVersion;
//                    speechFrag.VersionId = storyVersion.Id;
//                    if (speechFrag.TrueResultParentId.HasValue)
//                    {
//                        DataConditionalFragment foundTrueParent = conFragments.FirstOrDefault(x => x.Id == speechFrag.TrueResultParentId.Value);

//                        if (foundTrueParent != null)
//                        {
//                            if (foundTrueParent.TrueResultFragments == null)
//                                foundTrueParent.TrueResultFragments = new List<DataSpeechFragment>();

//                            foundTrueParent.TrueResultFragments.Add(speechFrag);

//                        }
//                    }

//                    if (speechFrag.FalseResultParentId.HasValue)
//                    {
//                        DataConditionalFragment foundFalseParent = conFragments.FirstOrDefault(x => x.Id == speechFrag.FalseResultParentId.Value);

//                        if (foundFalseParent != null)
//                        {
//                            if (foundFalseParent.FalseResultFragments == null)
//                                foundFalseParent.FalseResultFragments = new List<DataSpeechFragment>();

//                            foundFalseParent.FalseResultFragments.Add(speechFrag);
//                        }
//                    }
//                }

//                reader.NextResult();

//               LoadFragmentConditions(storyVersion,  conFragments, reader);
//            }
//            else
//            {
//                reader.NextResult();
//                reader.NextResult();
//                reader.NextResult();

               
//                reader.NextResult();
//            }
//        }

//        private static void LoadFragmentConditions(DataTitleVersion storyVersion, List<DataConditionalFragment> conditionalFrags, Npgsql.NpgsqlDataReader reader)
//        {

//            if ((conditionalFrags?.Any()).GetValueOrDefault(false))
//            {
//                long fragId;
//                long invConditionId;
//                //fnv."ConditionFragmentId", fnv."ConditionId"
//                while (reader.Read())
//                {
//                    fragId = reader.GetInt64(0);
//                    invConditionId = reader.GetInt64(1);

//                    DataConditionalFragment foundFrag = conditionalFrags.FirstOrDefault(x => x.Id == fragId);
//                    if (foundFrag != null)
//                    {
//                        // Locate the inventory condition
//                        DataInventoryCondition foundInvCondition = storyVersion.DataInventoryConditions.FirstOrDefault(x => x.Id == invConditionId);
//                        if (foundInvCondition != null)
//                        {
//                           // foundFrag.InventoryConditions.Add(foundInvCondition);
//                        }
//                    }
//                }

//                reader.NextResult();

//                long visitConditionId;
//                //fnv."ConditionFragmentId", fnv."ConditionId"
//                while (reader.Read())
//                {
//                    fragId = reader.GetInt64(0);
//                    visitConditionId = reader.GetInt64(1);

//                    DataConditionalFragment foundFrag = conditionalFrags.FirstOrDefault(x => x.Id == fragId);
//                    if (foundFrag != null)
//                    {
//                        // Locate the inventory condition
//                        DataNodeVisitCondition nodeVisitCondition = storyVersion.DataNodeVisitConditions.FirstOrDefault(x => x.Id == visitConditionId);
//                        if (nodeVisitCondition != null)
//                        {
//                           // foundFrag.NodeVisitConditions.Add(nodeVisitCondition);
//                        }
//                    }
//                }
//                reader.NextResult();
//            }
//            else
//            {
//                reader.NextResult();
//                reader.NextResult();
//            }
          
            


//        }


//        private static void LoadStandardNodes(DataTitleVersion storyVersion, List<DataNode> standardNodes, Npgsql.NpgsqlDataReader reader)
//        {

//            while (reader.Read())
//            {
//                DataStandardNode standardNode = new DataStandardNode();
//                standardNode.Id = reader.GetInt64(0);
//                long parentVersion = reader.GetInt64(1);
//                long nodeId = reader.GetInt64(2);
//                standardNode.NodeType = (StoryNodeType)reader.GetValue(3);

//                if (standardNodes != null)
//                {
//                    DataNode foundNode = standardNodes.FirstOrDefault(x => x.Id == nodeId);
//                    if (foundNode != null && storyVersion.Id == parentVersion)
//                    {
//                        if (storyVersion.StandardNodes == null)
//                            storyVersion.StandardNodes = new List<DataStandardNode>();
//                        standardNode.Node = foundNode;
//                        storyVersion.StandardNodes.Add(standardNode);
//                    }
//                }


//            }

//            reader.NextResult();
//        }

//        private static Tuple<List<DataNode>, List<DataNode>> LoadNodes(DataTitleVersion storyVersion, Npgsql.NpgsqlDataReader reader)
//        {
//            List<DataNode> standardNodes = new List<DataNode>();
//            List<DataNode> storyNodes = new List<DataNode>();

//            while (reader.Read())
//            {

//                string coorText = reader.IsDBNull(4) ? null : reader.GetString(4);

//                DataNode node = new DataNode(coorText);
//                node.Id = reader.GetInt64(0);
//                long? parentChapter = reader.IsDBNull(1) ? null : (long?)reader.GetInt64(1);
//                node.Name = reader.IsDBNull(2) ? null : reader.GetString(2);
//                node.ResponseBehavior = (ResponseBehavior)reader.GetValue(3);
//                if (parentChapter.HasValue)
//                {
//                    if (storyVersion.Chapters != null)
//                    {
//                        DataChapter foundChapter = storyVersion.Chapters.FirstOrDefault(x => x.Id == parentChapter.Value);
//                        if (foundChapter != null)
//                        {
//                            if (foundChapter.Nodes == null)
//                                foundChapter.Nodes = new List<DataNode>();

//                            foundChapter.Nodes.Add(node);

//                            storyNodes.Add(node);
//                        }
//                    }
//                }
//                else
//                {
//                    standardNodes.Add(node);
//                }
//            }

//            reader.NextResult();

//            return new Tuple<List<DataNode>, List<DataNode>>(standardNodes, storyNodes);
//        }

//        private static void LoadChapterWithTitles(DataTitleVersion storyVersion, Npgsql.NpgsqlDataReader reader)
//        {
//                while (reader.Read())
//                {
//                    DataChapter chapter = new DataChapter();

//                    chapter.Id = reader.GetInt64(0);
//                    long parentVersion = reader.GetInt64(1);
//                    chapter.Sequence = reader.GetInt32(2);

//                    if (storyVersion.Chapters == null)
//                        storyVersion.Chapters = new List<DataChapter>();

//                    storyVersion.Chapters.Add(chapter);
//                }

//            reader.NextResult();

//        }

//        private static void LoadSlotMappings(DataTitleVersion storyVersion, Npgsql.NpgsqlDataReader reader)
//        {
         
//            while (reader.Read())
//            {
//                DataIntentSlotMapping slotMapping = new DataIntentSlotMapping();
             
//                slotMapping.Alias = reader.GetString(0);
//                long parentIntentId = reader.GetInt64(1);
//                long? slotTypeId = reader.IsDBNull(2) ? null : (long?)reader.GetInt64(2);

//                slotMapping.IntentId = parentIntentId;

//                if (slotTypeId.HasValue)
//                {
//                    if (storyVersion.Slots != null)
//                    {
//                        DataSlotType mappedType = storyVersion.Slots.FirstOrDefault(x => x.Id.Equals(slotTypeId));

//                        if (mappedType != null)
//                            slotMapping.SlotType = mappedType;

//                    }
//                }

//                if (storyVersion.Intents != null)
//                {
//                    DataIntent parentIntent = storyVersion.Intents.FirstOrDefault(x => x.Id.Equals(parentIntentId));
//                    if (parentIntent != null)
//                    {

//                        if (parentIntent.SlotTypeMappings == null)
//                            parentIntent.SlotTypeMappings = new List<DataIntentSlotMapping>();

//                        parentIntent.SlotTypeMappings.Add(slotMapping);
//                    }
//                }



//            }

//            reader.NextResult();

//        }

//        private static void LoadIntents(DataTitleVersion storyVersion, Npgsql.NpgsqlDataReader reader)
//        {

//            storyVersion.Intents = new List<DataIntent>();

//            while (reader.Read())
//            {
//                string locIntents = reader.IsDBNull(4) ? null : reader.GetString(4);
//                DataIntent intent = new DataIntent(locIntents);

//                intent.Id = reader.GetInt64(0);
//                intent.Name = reader.GetString(1);
//                intent.UniqueId = reader.GetGuid(2);
//                intent.VersionId = reader.GetInt64(3);
//                storyVersion.Intents.Add(intent);


//            }

//            reader.NextResult();
//        }


//        //private static void LoadSlots(DataTitleVersion storyVersion, Npgsql.NpgsqlDataReader reader)
//        //{

//        //    storyVersion.Slots = new List<DataSlotType>();
//        //    while (reader.Read())
//        //    {
//        //        string retObject = reader.IsDBNull(4) ? null : reader.GetString(4);

//        //        DataSlotType slotType = new DataSlotType(retObject);
//        //        slotType.Id = reader.GetInt64(0);
//        //        slotType.Name = reader.GetString(1);
//        //        slotType.UniqueId = reader.GetGuid(2);
//        //        slotType.VersionId = reader.GetInt64(3);


//        //        storyVersion.Slots.Add(slotType);
//        //    }


//        //    reader.NextResult();
//        //}

//        //private static DataTitleVersion LoadStoryVersion(Npgsql.NpgsqlDataReader reader)
//        //{
//        //    //   unnamed portal 1
//        //    DataTitleVersion storyVersion = null;


//        //    //  reader.NextResult();
//        //    List<DataTitleVersion> versions = new List<DataTitleVersion>();
//        //    while (reader.Read())
//        //    {
//        //        DataTitleVersion rowVersion = new DataTitleVersion();

//        //        rowVersion.Id = reader.GetInt64(0);
//        //        rowVersion.PublishDate = reader["PublishDate"] == DBNull.Value ? null : (DateTime?)reader["PublishDate"];
//        //        rowVersion.StoryId = reader.GetInt64(2);
//        //        rowVersion.Version = reader.GetString(3);
//        //        rowVersion.IsDeleted = reader.GetBoolean(4);
//        //        rowVersion.UniqueId = reader.GetGuid(5);
//        //        versions.Add(rowVersion);
//        //    }

//        //    reader.NextResult();

//        //    if ((versions?.Count).GetValueOrDefault(0) == 1)
//        //        storyVersion = versions[0];


//        //    return storyVersion;
//        //}

//        //public async Task PurgeStoryAsync(long storyId)
//        //{

//        //    using (var context = new StoryEngineContext(_contextOptions))
//        //    {
//        //        DataTitle storyCore = await context.StoryTitles.Where(x => x.Id == storyId)
//        //                                       .Include(x => x.Versions).IgnoreQueryFilters().SingleOrDefaultAsync();
//        //        if(storyCore!=null)
//        //        {
//        //            if((storyCore.Versions?.Any()).GetValueOrDefault(false))
//        //            {
//        //                // ReSharper disable once PossibleNullReferenceException
//        //                foreach(DataTitleVersion ver in storyCore.Versions)
//        //                {
//        //                    // ReSharper disable once PossibleInvalidOperationException
//        //                    await context.Database.ExecuteSqlCommandAsync("SELECT purgeversion({0})", ver.Id.Value);

//        //                }

//        //            }

//        //            await context.Database.ExecuteSqlCommandAsync("SELECT purgestory({0})", storyId);

//        //        }


//        //    }
//        //}



//        public async Task PurgeVersionAsync(long versionId)
//        {



//            using (var context = new StoryEngineContext(_contextOptions))
//            {

//               await  context.Database.ExecuteSqlCommandAsync("SELECT purgeversion({0})", versionId);

               
//            }
            
//        }

//        public Task<StoryNode> GetNodeByNameAsync(string titleId, string storyNodeName)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<ICollection<StoryNode>> GetNodesByTitleAsync(string titleId)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<StoryConditionBase> GetStoryConditionAsync(string titleId, string conditionName)
//        {
//            throw new NotImplementedException();
//        }

//        public void UpdateTitle(StoryTitle entity)
//        {
//            throw new NotImplementedException();
//        }

//        public Task UpdateTitleAsync(StoryTitle entity)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
