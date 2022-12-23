using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.Amazon;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Repository;
using Xunit;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Test
{

    public class FileImportTest : TestServerFixture
    {
  
        [Theory(DisplayName ="Import Audio File")]
        [InlineData("backgroundopen.mp3", "test_adventure", "1.0")]
        [InlineData("restart.mp3", null, "1.0")]
        [InlineData("storyintro.mp3",null, "1.0")]
        [InlineData("trollgrowl.mp3", "test_adventure", "1.1")]
        [InlineData("trollsniff.mp3", "test_adventure", "1.1")]
        public async void ImportAudioFile(string audioFile, string title, string version)
        {

          string testDataFile = string.Concat(@"AudioFiles\", audioFile);

           byte[] audioContent = File.ReadAllBytes(testDataFile);


            S3FileStore blobRep = GetBlobRepository();

            TitleVersion titleVer = new TitleVersion(title, version);

             await blobRep.StoreFileAsync( titleVer, audioFile, audioContent);

        }



        [Fact(DisplayName = "Get Audio File contents")]
        public async void GetFileContents()
        {

            S3FileStore blobRep = GetBlobRepository();

            TitleVersion titleVer = new TitleVersion("test_adventure", "1.0");

            byte[] fileContents = await blobRep.GetFileContentAsync( titleVer, "trollgrowl.mp3");
        }

    
        [Fact(DisplayName = "Save Yaml Dictionary")]
        public void AppYamlSave()
        {
            Dictionary<string, string> titleMappings = new Dictionary<string, string>();

            // This is Sanj's skill.
            titleMappings.Add("+17344283758", "symbicordsmsinbound");

            // This is John's skill.
            titleMappings.Add("amzn1.ask.skill.d03659bc-3e22-40b0-a85e-63d4f8c533b4", "test_adventure");


            var yamlSerializer = YamlSerializationBuilder.GetYamlSerializer();
            string rawText = yamlSerializer.Serialize(titleMappings);


            
            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();
            Dictionary<string, string> skillMappings = yamlDeserializer.Deserialize<Dictionary<string, string>>(rawText);


          //  File.WriteAllText("appmappings.yaml", rawText);

            
        }





    }
}
