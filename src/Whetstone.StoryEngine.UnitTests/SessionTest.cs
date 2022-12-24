using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Repository;
using Xunit;

namespace Whetstone.UnitTests
{

    public class SessionTest
    {


        [Fact]
        public async Task SaveSessionAsync()
        {

            string sessionTextMsg = File.ReadAllText(@"SessionSamples/smssession.json");

            var mocker = new MockFactory();

            TitleVersion titleVer = TitleVersionUtil.GetWhetstoneTitle();

            IServiceCollection servCol = mocker.InitServiceCollection(titleVer);

            IServiceProvider servProv = servCol.BuildServiceProvider();

            RequestRecordMessage sessionQueueMsg = JsonConvert.DeserializeObject<RequestRecordMessage>(sessionTextMsg);

            ISessionLogger sessionLogger = servProv.GetRequiredService<ISessionLogger>();


            await sessionLogger.LogRequestAsync(sessionQueueMsg);


        }

        [Fact]
        public async Task StoreSessionState()
        {
            StoryRequest req = new StoryRequest
            {
                ApplicationId = Guid.NewGuid().ToString("N"),
                UserId = Guid.NewGuid().ToString("N"),
                Client = Client.Alexa,
                SessionId = Guid.NewGuid().ToString("N"),
                RequestType = StoryRequestType.Launch,
                IsNewSession = true
            };

            ISessionStoreManager storeManager = GetSessionStoreManager();

            var sessionStartType = await storeManager.SaveSessionStartTypeAsync(req);
            var retreivedType = await storeManager.GetSessionStartTypeAsync(req);

            Assert.True(retreivedType.HasValue, "Start type not saved to cache");

            Assert.True(retreivedType.Value == sessionStartType, "Start types are not equal");

        }



        [Fact]
        public async Task StoreBadIntentCounterTestAsync()
        {
            StoryRequest req = new StoryRequest
            {
                ApplicationId = Guid.NewGuid().ToString("N"),
                UserId = Guid.NewGuid().ToString("N"),
                Client = Client.Alexa,
                SessionId = Guid.NewGuid().ToString("N"),
                RequestType = StoryRequestType.Launch,
                IsNewSession = true
            };
            var storeManager = GetSessionStoreManager();

            int badIntentCounter = await storeManager.GetBadIntentCounterAsync(req);
            Debug.Assert(badIntentCounter == 0, "Bad intent counter is not zero");

            // increment the counter
            badIntentCounter = await storeManager.IncrementBadIntentCounterAsync(req);
            Debug.Assert(badIntentCounter == 1, "Bad intent counter is not incremented");

            // reset the counter
            await storeManager.ResetBadIntentCounterAsync(req);

            badIntentCounter = await storeManager.GetBadIntentCounterAsync(req);
            Debug.Assert(badIntentCounter == 0, "Bad intent counter is not reset");


        }

        private ISessionStoreManager GetSessionStoreManager()
        {

            var mocker = new MockFactory();

            TitleVersion titleVer = TitleVersionUtil.GetWhetstoneTitle();

            IServiceCollection servCol = mocker.InitServiceCollection(titleVer);

            IServiceProvider servProv = servCol.BuildServiceProvider();

            ISessionStoreManager storeManager = servProv.GetRequiredService<ISessionStoreManager>();


            return storeManager;

        }

    }
}
