using System.Threading.Tasks;
using Whetstone.StoryEngine.Data.Caching;
using Whetstone.StoryEngine.Data.EntityFramework.EntityManager;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Story;
using Xunit;

namespace Whetstone.StoryEngine.Data.Tests
{
    public class DataAppMappingReaderTest : DataTestBase
    {


        [Fact]
        public async Task GetWhetstoneAppMappingsAsync()
        {

            IUserContextRetriever userContextRetriever = GetUserContextRetriever(DBConnectionRetreiverType.Direct);


        

            ITitleCacheRepository titleCacheRep = GetTitleCache();

            var appMappingReader = CreateLogger<DataAppMappingReader>();

            DataAppMappingReader dataRep = new DataAppMappingReader(userContextRetriever, titleCacheRep, appMappingReader);

            string clientAppId = "amzn1.ask.skill.c4cabd50-2cd5-4e4c-a03c-a57d4f2a0e5f";

            TitleVersion titleVer = await dataRep.GetTitleAsync( Models.Client.Alexa, clientAppId, null);

            Assert.True(titleVer.Version.Equals("0.1"), "Expected version 0.1");

            titleVer = await dataRep.GetTitleAsync( Models.Client.Alexa, clientAppId, "ws02");

            Assert.True(titleVer.Version.Equals("0.2"), "Expected version 0.2");


            titleVer = await dataRep.GetTitleAsync(Models.Client.Alexa, clientAppId, "PROD");

            Assert.True(titleVer.Version.Equals("0.1"), "Expected version 0.1");

        }



        [Fact]
        public async Task GetClientMappingAsync()
        {

            IUserContextRetriever userContextRetriever = GetUserContextRetriever(DBConnectionRetreiverType.Direct);
            var distCacheDict = GetMemoryCache();

            var inMemCache = GetInMemoryCache();

            var appMappingReader = CreateLogger<DataAppMappingReader>();
            ITitleCacheRepository titleCacheRep = GetTitleCache();

            DataAppMappingReader dataRep = new DataAppMappingReader(userContextRetriever, titleCacheRep, appMappingReader);


            var mappedTitle = await dataRep.GetTitleAsync( Models.Client.Alexa, "amzn1.ask.skill.ba0ed979-60c3-4eaf-b410-329d9bd70cb6", null);

        }


    }
}
