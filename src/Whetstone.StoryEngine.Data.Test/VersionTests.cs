using Amazon.S3;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data.Amazon;
using Whetstone.StoryEngine.Data.Caching;
using Whetstone.StoryEngine.Data.EntityFramework.EntityManager;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Story;
using Xunit;

namespace Whetstone.StoryEngine.Data.Tests
{
    public class VersionTests : DataTestBase
    {
        //[Fact]
        //public async Task CreateSampleVersion()
        //{
        //    string titleId = "versiontitleid";

        //    IUserContextRetriever userContextRetriever = GetUserContextRetriever();

        //    var distCacheDict = GetMemoryCache();

        //    ITitleCacheRepository titleCacheRep = new TitleCacheRepository(distCacheDict);

        //    var opts = GetEnvironmentConfig();

        //    IFileRepository fileRep = new S3FileStore(opts);

        //    DataTitleRepository dataRep = new DataTitleRepository(userContextRetriever, titleCacheRep, null, null);
        //    StoryTitle newTitle = new StoryTitle();
        //    newTitle.Id = titleId;
        //    newTitle.Description = "Test adding a new title";
        //    newTitle.Title = "New Title";
        //    await dataRep.CreateOrUpdateTitleAsync(newTitle);
        //    TitleVersion titleVer = new TitleVersion(titleId, "0.1");
        //    DataTitleVersionRepository versionRep = new DataTitleVersionRepository(userContextRetriever, titleCacheRep, fileRep);
        //    await versionRep.CreateOrUpdateVersionAsync(titleVer, "new version test");
        //}



        [Fact]
        public async Task DeleteSampleVersion()
        {
            string titleId = "versiontitleid";

            IUserContextRetriever userContextRetriever = GetUserContextRetriever(DBConnectionRetreiverType.Direct);
            var distCacheDict = GetMemoryCache();

            var opts = GetEnvironmentConfig();
            var inMemoryCache = GetInMemoryCache();


            ILogger<S3FileStore> fileStoreLogger = CreateLogger<S3FileStore>();
            IAmazonS3 s3Client = GetS3Client();

            IFileRepository fileRep = new S3FileStore(opts, userContextRetriever, s3Client, fileStoreLogger);
            ITitleCacheRepository titleCacheRep = GetTitleCache();

            DataTitleRepository dataRep = new DataTitleRepository(userContextRetriever, titleCacheRep, null, null);
            StoryTitle newTitle = new StoryTitle();
            newTitle.Id = titleId;
            newTitle.Description = "Test adding a new title";
            newTitle.Title = "New Title";
            await dataRep.CreateOrUpdateTitleAsync(newTitle);


            TitleVersion titleVer = new TitleVersion(titleId, "0.1");

            DataTitleVersionRepository versionRep = new DataTitleVersionRepository(userContextRetriever, titleCacheRep, fileRep);


            await versionRep.DeleteVersionAsync(titleVer.ShortName, titleVer.Version);
        }

        [Fact]
        public async Task CopyMediaFiles()
        {
            string titleId = "animalfarmpi";
            var opts = GetEnvironmentConfig();
            IUserContextRetriever userContextRetriever = GetUserContextRetriever(DBConnectionRetreiverType.Direct);
            ILogger<S3FileStore> fileStoreLogger = CreateLogger<S3FileStore>();
            IAmazonS3 s3Client = GetS3Client();
            IFileRepository fileRep = new S3FileStore(opts, userContextRetriever, s3Client, fileStoreLogger);

            await fileRep.CopyMediaFilesAsync(titleId, null, "1.1");
        }

        [Fact]
        public async Task PurgeVersionTest()
        {

            string titleId = "animalfarmpi";

            IUserContextRetriever userContextRetriever = GetUserContextRetriever(DBConnectionRetreiverType.Direct);


            var opts = GetEnvironmentConfig();
            ILogger<S3FileStore> fileStoreLogger = CreateLogger<S3FileStore>();
            IAmazonS3 s3Client = GetS3Client();

            IFileRepository fileRep = new S3FileStore(opts, userContextRetriever, s3Client, fileStoreLogger);
            ITitleCacheRepository titleCacheRepository = GetTitleCache();

            DataTitleVersionRepository versionRep = new DataTitleVersionRepository(userContextRetriever, titleCacheRepository, fileRep);


            await versionRep.PurgeVersionAsync(titleId, "0.2");



        }


        [Fact]
        public async Task ListBreakdownTest()
        {
            //string env = "dev";
            string titleId = "eyeoftheeldergods";
            string version = "0.8";



            var opts = GetEnvironmentConfig();

            ILogger<S3FileStore> fileStoreLogger = CreateLogger<S3FileStore>();

            IUserContextRetriever userContextRetriever = GetUserContextRetriever(DBConnectionRetreiverType.Direct);

            IAmazonS3 s3Client = GetS3Client();
            IFileRepository fileRep = new S3FileStore(opts, userContextRetriever, s3Client, fileStoreLogger);
            TitleVersion titleVer = new TitleVersion(titleId, version);

            List<string> audioFiles = await fileRep.GetAudioFileListAsync(titleVer);

            string[] audioFileArray = audioFiles.ToArray();

            int portions = 10;

            for (int i = 0; i < audioFileArray.Length; i += portions)
            {
                int maxCount = (i + portions < audioFileArray.Length) ? portions : audioFileArray.Length % portions;
                string[] buffer = new string[maxCount];
                Array.Copy(audioFileArray, i, buffer, 0, maxCount);
                // Process the buffer here if needed
            }



        }
    }
}
