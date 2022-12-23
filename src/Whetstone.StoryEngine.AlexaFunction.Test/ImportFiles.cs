﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.Amazon;
using Xunit;
using Whetstone.StoryEngine.Models.Configuration;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Serialization;
using System.Threading.Tasks;
using Amazon.S3;
using Whetstone.StoryEngine.Test;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

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

        [Fact(DisplayName = "Import Animal Farm Zip to S3")]
        public async Task ImportStoryTitleZipYaml()
        {

            string testDataFile = string.Concat(@"ImportFiles\", "animalfarmpi.zip");

            byte[] importZip = File.ReadAllBytes(testDataFile);



            ILogger<S3FileStore> fileStoreLogger = Services.GetService<ILogger<S3FileStore>>();

            IUserContextRetriever contRet = Services.GetService<IUserContextRetriever>();

            IAmazonS3 s3Client = Services.GetService<IAmazonS3>();


            IFileRepository fileRep = new S3FileStore(_envOptions, contRet, s3Client, fileStoreLogger);

            IStoryTitleImporter storyImporter = new S3StoryTitleImporter(fileRep);

           await storyImporter.ImportFromZipAsync(importZip);

        }


        [Fact(DisplayName = "Import Animal Farm Test Zip to S3")]
        public async Task ImportTestStoryTitleZipYaml()
        {



            string testDataFile = string.Concat(@"ImportFiles\", "animalfarmintenttest.zip");

            byte[] importZip = File.ReadAllBytes(testDataFile);



            ILogger<S3FileStore> fileStoreLogger = Services.GetService<ILogger<S3FileStore>>();

            IUserContextRetriever contRet = Services.GetService<IUserContextRetriever>();

            IAmazonS3 s3Client = Services.GetService<IAmazonS3>();


            IFileRepository fileRep = new S3FileStore(_envOptions, contRet, s3Client, fileStoreLogger);

            IStoryTitleImporter storyImporter = new S3StoryTitleImporter(fileRep);

            await storyImporter.ImportFromZipAsync(importZip);

        }

    }
}
