using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data.Caching;
using Whetstone.StoryEngine.Data.EntityFramework;
using Whetstone.StoryEngine.Data.EntityFramework.EntityManager;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Tracking;
using Xunit;

namespace Whetstone.StoryEngine.Data.Tests
{
    public class UserStoreTests : DataTestBase
    {


        [Fact]
        public async Task GetUserTestAsync()
        {
            EngineSessionContext userId = await GetUserAsync();


        }



        internal async Task<EngineSessionContext> GetUserAsync()
        {

            Client curClient = Client.Alexa;
            string clientAppId = "amzn1.ask.skill.75bd5c59-177e-4552-ba62-d861c2782cc1";


            ITitleCacheRepository titleCacheRep = GetTitleCache();

            IUserContextRetriever userContextRetriever = GetUserContextRetriever(DBConnectionRetreiverType.Direct);

            var appLogger = CreateLogger<DataAppMappingReader>();

            IAppMappingReader appReader = new DataAppMappingReader(userContextRetriever, titleCacheRep, appLogger);

            var userDataRep = CreateLogger<UserDataRepository>();

            IStoryUserRepository userRep = new UserDataRepository(userContextRetriever, userDataRep);
            Stopwatch titleTime = Stopwatch.StartNew();
            TitleVersion titleVer = await appReader.GetTitleAsync(curClient, clientAppId, null);
            Debug.WriteLine($"TitleRetrieval Time: {titleTime.ElapsedMilliseconds}");

            StoryRequest storyReq = new StoryRequest();

            storyReq.Client = curClient;
            storyReq.ApplicationId = clientAppId;

            storyReq.IsNewSession = true;
            storyReq.RequestType = StoryRequestType.Launch;
            storyReq.RequestTime = DateTime.UtcNow;
            storyReq.SessionId = Guid.NewGuid().ToString("N");
            storyReq.UserId = "93e14e4a1f594a19971893b07af01fb9";
            storyReq.Locale = "en-US";
            storyReq.SessionContext = new EngineSessionContext();

            //  storyReq.SessionContext.TitleVersion = titleVer;

            DataTitleClientUser user = await userRep.GetUserAsync(storyReq);
            user.CurrentNodeName = "BeginNode";

            // Now save the user.

            user.TitleState = new List<IStoryCrumb>() { new SelectedItem() { Name = "someitem", Value = "somevalue" } };

            await userRep.SaveUserAsync(user);

            EngineSessionContext sessionContext = new EngineSessionContext();

            sessionContext.EngineUserId = user.Id.Value;
            sessionContext.TitleVersion = titleVer;
            sessionContext.EngineSessionId = Guid.NewGuid();

            return sessionContext;
        }


    }
}