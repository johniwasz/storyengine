using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.Amazon;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;
using Xunit;
using YamlDotNet.Core;

namespace Whetstone.StoryEngine.Test
{

    public class AnimalFarmTest : TestServerFixture
    {



        [Fact(DisplayName = "Import Animal Farm Zip to S3 - Old")]
        public async Task ImportStoryTitleZipYaml()
        {

            byte[] importZip = await GetResourceAsync("ImportFiles.animalfarmpi.zip");

            IFileRepository fileRep = Services.GetRequiredService<IFileRepository>();

            IStoryTitleImporter storyImporter = new S3StoryTitleImporter(fileRep);


            await storyImporter.ImportFromZipAsync(importZip);

        }


        [Fact(DisplayName = "AnimalFarmDeserializer")]
        public void AnimalFarmDeserializer()
        {

            string testDataFile = string.Concat(@"ImportFiles\animalfarmpi\", "animalfarmpi.yaml");

            string rawText = File.ReadAllText(testDataFile);
            try
            {
                var deser = YamlSerializationBuilder.GetYamlDeserializer();

                StoryTitle title = deser.Deserialize<StoryTitle>(rawText);


                // Get the dark path 3 node

                StoryNode darkPath3 = title.Nodes.FirstOrDefault(x => x.Name.Equals("DarkPath3", StringComparison.InvariantCultureIgnoreCase));

                if (darkPath3 != null)
                {

                    var choice = darkPath3.Choices.FirstOrDefault(x => x.IntentName.Equals("No", StringComparison.InvariantCultureIgnoreCase));

                    darkPath3.Choices.Remove(choice);

                    var mulitChoice = new Choice();
                    mulitChoice.IntentName = "No";
                    //mulitChoice.StoryNodeNames = new List<string>();

                    //mulitChoice.StoryNodeNames.Add("DarkPath1");
                    //mulitChoice.StoryNodeNames.Add("DarkPath2");
                    //mulitChoice.StoryNodeNames.Add("DarkPath3");

                    darkPath3.Choices.Add(mulitChoice);

                    var ser = YamlSerializationBuilder.GetYamlSerializer();
                    rawText = ser.Serialize(title);

                    File.WriteAllText("animalfarm.yaml", rawText);

                }


            }
            catch (YamlException yamlEx)
            {
                Debug.WriteLine(yamlEx);
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }

        }
    }
}
