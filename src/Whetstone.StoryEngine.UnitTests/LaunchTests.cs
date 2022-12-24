using Amazon.Lambda.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Whetstone.Alexa;
using Whetstone.StoryEngine.AlexaProcessor;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.Repository.Actions;
using Whetstone.UnitTests.MessageManagement;
using Xunit;

namespace Whetstone.UnitTests
{
    public class LaunchTests
    {
        [Fact(DisplayName = "Validate empty request")]
        public async Task LaunchEmptyRequestAsync()
        {



            var mocker = new MockFactory();

            TitleVersion titleVer = new TitleVersion();
            titleVer.ShortName = "animalfarmpi";
            titleVer.Version = "1.2";
            titleVer.TitleId = Guid.NewGuid();
            titleVer.VersionId = Guid.NewGuid();

            var servCol = mocker.InitServiceCollection(titleVer);

            IServiceProvider servProv = servCol.BuildServiceProvider();

            IStoryRequestProcessor storyProcessor = servProv.GetService<IStoryRequestProcessor>();

            string appIdGuid = Guid.NewGuid().ToString();


            AlexaSessionContext sessionContext = new AlexaSessionContext(appIdGuid, Guid.NewGuid().ToString(), "en-US");

            var context = new TestLambdaContext();

            AlexaRequest req = sessionContext.CreateLaunchRequest();

            StoryRequest storyReq = req.ToStoryRequest();
            storyReq.SessionContext = new EngineSessionContext();
            storyReq.SessionContext.TitleVersion = titleVer;

            StoryResponse resp = await storyProcessor.ProcessStoryRequestAsync(storyReq);


            Assert.True(resp.ForceContinueSession);
        }


        /// <summary>
        /// Validates that a new user node is returned when a new user request comes in.
        /// </summary>
        /// <returns></returns>
        [Fact(DisplayName = "Validate New User Launch Response")]
        public async Task ValidateLaunchResponseAsync()
        {

            string appIdGuid = Guid.NewGuid().ToString();

            AlexaSessionContext sessionContext = new AlexaSessionContext(appIdGuid, Guid.NewGuid().ToString(), "en-US");
            AlexaRequest req = sessionContext.CreateLaunchRequest();

            StoryRequest storyReq = req.ToStoryRequest();
            storyReq.SessionContext = new EngineSessionContext();


            var mocker = new MockFactory();


            TitleVersion titleVer = TitleVersionUtil.GetClinicalTrialTitle();
            storyReq.SessionContext.TitleVersion = titleVer;
            IServiceCollection servCol = mocker.InitServiceCollection(titleVer);


            IStoryUserRepository StoryUserFunc(UserRepositoryType handlerKey) => mocker.GetStoryUserRepository();

            IStoryUserRepository storyRep = mocker.GetStoryUserRepository();

            string newUserNodeName = "WelcomeNewUser";

            var servProv = servCol.BuildServiceProvider();

            ITitleReader reader = servProv.GetRequiredService<ITitleReader>();
            IAppMappingReader appReader = servProv.GetRequiredService<IAppMappingReader>();
            ISkillCache skillCache = servProv.GetRequiredService<ISkillCache>();
            ISessionStoreManager sessionStore = servProv.GetRequiredService<ISessionStoreManager>();

            IOptions<EnvironmentConfig> envOptions = servProv.GetRequiredService<IOptions<EnvironmentConfig>>();
            Func<NodeActionEnum, INodeActionProcessor> actionFunc =
                servProv.GetRequiredService<Func<NodeActionEnum, INodeActionProcessor>>();

            ILogger<StoryRequestProcessor> logger = servProv.GetRequiredService<ILogger<StoryRequestProcessor>>();


            StoryRequestProcessor reqProcessor = new StoryRequestProcessor(appReader, reader, StoryUserFunc, sessionStore, skillCache, actionFunc, envOptions, logger);

            var context = new TestLambdaContext();
            storyReq.SessionContext.TitleVersion = titleVer;

            StoryResponse resp = await reqProcessor.ProcessStoryRequestAsync(storyReq);


            Assert.Equal(newUserNodeName, resp.NodeName);

        }


    }
}
