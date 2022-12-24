using Amazon.S3;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data.Amazon;
using Whetstone.StoryEngine.Data.Caching;
using Whetstone.StoryEngine.Data.EntityFramework.EntityManager;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Story;
using Xunit;

namespace Whetstone.StoryEngine.Data.Tests
{
    public class VersionDeploymentTests : DataTestBase
    {
        protected const string APP_MAPPING_PATH = "global/appmappings.yaml";

        [Fact]
        public async Task CreateSampleVersion()
        {


            string titleId = "versiontitleid";


            IUserContextRetriever userContextRetriever = GetUserContextRetriever(DBConnectionRetreiverType.Direct);
            var distCacheDict = GetMemoryCache();


            IOptions<EnvironmentConfig> envConfigOpts = GetEnvironmentConfig();

            ILogger<S3FileStore> s3Logger = CreateLogger<S3FileStore>();

            IAmazonS3 s3Client = GetS3Client();

            IFileRepository fileRep = new S3FileStore(envConfigOpts, userContextRetriever, s3Client, s3Logger);

            var memDict = GetMemoryCache();

            var inMemoryCache = GetInMemoryCache();

            ILogger<TitleCacheRepository> cacheRep = CreateLogger<TitleCacheRepository>();

            ITitleCacheRepository titleCacheRep = new TitleCacheRepository(fileRep, memDict, inMemoryCache, cacheRep);

            DataTitleRepository dataRep = new DataTitleRepository(userContextRetriever, titleCacheRep, null, null);
            StoryTitle newTitle = new StoryTitle();
            newTitle.Id = titleId;
            newTitle.Description = "Test adding a new title";
            newTitle.Title = "New Title";
            await dataRep.CreateOrUpdateTitleAsync(newTitle);

            DataTitleVersionRepository versionRep = new DataTitleVersionRepository(userContextRetriever, titleCacheRep, fileRep);


            PublishVersionRequest publishRequest = new PublishVersionRequest()
            {
                TitleName = titleId,
                Version = "0.2",
                ClientType = Client.Alexa,
                ClientId = Guid.NewGuid().ToString("N")

            };

            await versionRep.PublishVersionAsync(publishRequest);
        }
    }
}
