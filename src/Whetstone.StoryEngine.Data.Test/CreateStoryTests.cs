using Amazon;
using Amazon.S3;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data.Amazon;
using Whetstone.StoryEngine.Data.Caching;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Story;
using Xunit;

namespace Whetstone.StoryEngine.Data.Tests
{
    public class CreateStoryTests : DataTestBase
    {
        [Fact]
        public async Task CreateSampleStory()
        {

            var inMemoryCache = GetInMemoryCache();

            var distCacheDict = GetMemoryCache();

            ILogger<TitleCacheRepository> titleLogger = CreateLogger<TitleCacheRepository>();

            ILogger<S3FileStore> fileStoreLogger = CreateLogger<S3FileStore>();


            EnvironmentConfig envConfig = new EnvironmentConfig();
            envConfig.BucketName = "whetstonebucket-dev-s3bucket-1nridm382p5vm";
            envConfig.Region = RegionEndpoint.USEast1;

            IOptions<EnvironmentConfig> envConfigOpts = Options.Create<EnvironmentConfig>(envConfig);

            IAmazonS3 s3Client = GetS3Client();

            IFileRepository fileRep = new S3FileStore(envConfigOpts, s3Client, fileStoreLogger);

            ITitleCacheRepository titleCacheRep = new TitleCacheRepository(fileRep, distCacheDict, inMemoryCache, titleLogger);


            StoryTitle newTitle = new StoryTitle();

            newTitle.Id = "newtitle123";
            newTitle.Title = "New Title 123";
            newTitle.Description = "Sample Title added during unit test";
            newTitle.Version = "0.1";
        }
    }
}
