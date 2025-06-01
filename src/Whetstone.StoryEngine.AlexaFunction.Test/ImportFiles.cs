using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.Amazon;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Test;
using Xunit;

namespace Whetstone.StoryEngine.AlexaFunction.Test
{
    public class ImportFiles : TestServerFixture
    {


        [Fact(DisplayName = "Simple Story Deserialization Test")]
        public void LoadYamlFile()
        {

            string yamlRawText = File.ReadAllText(@"localstore/stories/animalfarmpi/animalfarmpi.yaml");

            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();
            StoryTitle title = yamlDeserializer.Deserialize<StoryTitle>(yamlRawText);



        }

        /*
        [Fact(DisplayName = "Import Animal Farm Zip to S3")]
        public async Task ImportStoryTitleZipYaml()
        {

            string testDataFile = string.Concat(@"ImportFiles\", "animalfarmpi.zip");

            byte[] importZip = File.ReadAllBytes(testDataFile);

            ILogger<S3FileStore> fileStoreLogger = Services.GetService<ILogger<S3FileStore>>();

            IAmazonS3 s3Client = Services.GetService<IAmazonS3>();


            IFileRepository fileRep = new S3FileStore(_envOptions, s3Client, fileStoreLogger);

            IStoryTitleImporter storyImporter = new S3StoryTitleImporter(fileRep);

            await storyImporter.ImportFromZipAsync(importZip);
        }
        */

        [Fact(DisplayName = "Import Animal Farm Test Zip to S3")]
        public async Task ImportTestStoryTitleZipYaml()
        {
            string testDataFile = string.Concat(@"ImportFiles\", "animalfarmintenttest.zip");

            byte[] importZip = File.ReadAllBytes(testDataFile);

            ILogger<S3FileStore> fileStoreLogger = Services.GetService<ILogger<S3FileStore>>();

            IAmazonS3 s3Client = Services.GetService<IAmazonS3>();

            IFileRepository fileRep = new S3FileStore(_envOptions, s3Client, fileStoreLogger);

            IStoryTitleImporter storyImporter = new S3StoryTitleImporter(fileRep);

            await storyImporter.ImportFromZipAsync(importZip);

        }

    }
}
