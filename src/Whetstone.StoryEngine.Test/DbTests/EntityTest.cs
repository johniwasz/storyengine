using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.Amazon;
using Whetstone.StoryEngine.Data.Yaml;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Conditions;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Tracking;
using Xunit;

namespace Whetstone.StoryEngine.Test.DbTests
{

    public class EntityTest : EntityContextTestBase
    {
 


        //[Fact(DisplayName = "Purge Story")]
        //public async Task PurgeStory()
        //{

        //    DbContextOptions<StoryEngineContext> contextOptions = GetContextOptions<StoryEngineContext>(connection);

        //    EntityTitleRepository entityRep = new EntityTitleRepository(contextOptions);

        //    await entityRep.PurgeStoryAsync(1);

        //    await entityRep.PurgeStoryAsync(2);
        //}

        //[Fact(DisplayName = "Purge Version")]
        //public async Task PurgeVersion()
        //{

        //    DbContextOptions<StoryEngineContext> contextOptions = GetContextOptions<StoryEngineContext>(connection);

        //    EntityTitleRepository entityRep = new EntityTitleRepository(contextOptions);

        //    await entityRep.PurgeVersionAsync(1);
        //}

        //[Fact(DisplayName = "Export Data Representation")]
        //public async Task ExportDataRepresentation()
        //{

        //    DbContextOptions<StoryEngineContext> contextOptions = GetContextOptions<StoryEngineContext>(connection);

        //    EntityTitleRepository entityRep = new EntityTitleRepository(contextOptions);
        //    try
        //    {

        //        DataTitleVersion storyVersion = await entityRep.GetCurrentTitleVersionAsync(title, version);


        //        JsonSerializerSettings serSettings = new JsonSerializerSettings();
        //        serSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        //        serSettings.NullValueHandling = NullValueHandling.Ignore;
        //        serSettings.Formatting = Formatting.Indented;
        //        serSettings.ReferenceResolverProvider = () => new ReferenceResolverWithUuid();
        //        serSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
        //        serSettings.Converters.Add(new StringEnumConverter
        //        {
        //            CamelCaseText = true
        //        });

        //      //  serSettings.Converters.Add(new DollarIdPreservingConverter());


        //        StoryVersion modelVersion = storyVersion.ToStoryVersion();

        //        // Map to JSON
        //        string jsonExport = JsonConvert.SerializeObject(modelVersion, serSettings);


        //        StoryVersion verImport = JsonConvert.DeserializeObject<StoryVersion>(jsonExport, serSettings);



        //        DataTitleVersion dataVersion = verImport.ToDataStoryVersion();

        //    }

        //    catch(Npgsql.NpgsqlException sqlEx)
        //    {
        //        Debug.WriteLine(sqlEx);


        //    }
        //    catch (Exception ex)
        //    {

        //        Debug.WriteLine(ex);

        //    }

        //}

        //[Fact(DisplayName = "Create Entity Test")]
        //public async Task CreateStoryEntryTest()
        //{
        //    await ImportTitle(title);

        //    await ImportTitle("conversationenginedemo");

        //}



    //    private async Task ImportTitle(string title)
    //    {
    //        string env = "dev";
    //        EnvironmentConfigOptions envConfigOptions = new EnvironmentConfigOptions(env);

    //        IFileRepository fileRep = new S3FileStore(envConfigOptions);

    //        ITitleReader titleReader = new YamlTitleReader(fileRep);

    //        StoryTitle animalFarmTitle = await titleReader.GetByIdAsync(env, title);

    //        DataTitle saveTitle = null;

    //        try
    //        {
    //            saveTitle = await animalFarmTitle.ToRelationalStructure();
    //        }

    //        catch (Exception ex)
    //        {

    //            Debug.WriteLine(ex);

    //        }

    //        if (saveTitle != null)
    //        {
    //            DbContextOptions<StoryEngineContext> contextOptions = GetContextOptions<StoryEngineContext>(connection);

    //            EntityTitleRepository entityRep = new EntityTitleRepository(contextOptions);
    //            try
    //            {

    //                await entityRep.CreateTitleAsync(saveTitle);

    //            }
    //            catch (Exception ex)
    //            {

    //                Debug.WriteLine(ex);

    //            }
    //        }



    //    }

    }

}
