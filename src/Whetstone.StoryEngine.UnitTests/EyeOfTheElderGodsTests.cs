using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Google.Repository.Models;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.UnitTests;
using Xunit;

namespace Whetstone.UnitTests
{
    public class EyeOfTheElderGodsTests
    {

        //[Fact]
        //public async Task TestB3Node()


        //}


        [Fact]
        public async Task GoogleElderGodsTest()
        {


            TitleVersion eotegTitle = TitleVersionUtil.GetEOTEGTitle();

            Client clientType = Client.GoogleHome;

            SurfaceCapabilities surfaceCaps = new SurfaceCapabilities();

            surfaceCaps.HasAudio = true;
            surfaceCaps.HasMediaAudio = true;
            surfaceCaps.HasScreen = true;
            surfaceCaps.HasWebBrowser = true;

            void sessionLogFunc(RequestRecordMessage sl)
            {
                Assert.True(eotegTitle.DeploymentId.Value.Equals(sl.DeploymentId));

                Debug.WriteLine($"Logging deployment id: {sl.DeploymentId}");

            }

            // wire the delegates together so that the request to send the sms message is captured in the 
            // delegage above.
            var mocker = new MockFactory
            {
                ProcessSessionLogFunc = sessionLogFunc
            };

            IServiceCollection servCol = mocker.InitServiceCollection(eotegTitle);

            IServiceProvider servProv = servCol.BuildServiceProvider();

            IStoryRequestProcessor storyRepProc = servProv.GetRequiredService<IStoryRequestProcessor>();

            IMediaLinker linker = servProv.GetRequiredService<IMediaLinker>();


            var userStoreFunc = servProv.GetService<Func<UserRepositoryType, IStoryUserRepository>>();

            // Check the user state to see if the phone number is valid or not.
            IStoryUserRepository userRepo = userStoreFunc(UserRepositoryType.DynamoDB);

            ISessionLogger sessLogger = servProv.GetRequiredService<ISessionLogger>();

            StorySessionContext context = new StorySessionContext(eotegTitle, clientType);




            Microsoft.Extensions.Logging.ILoggerFactory loggerFactory = servProv.GetRequiredService<ILoggerFactory>();

            var logger = loggerFactory.CreateLogger("EOTEG");

            var expectedResult = new ExpectedNodeResult();

            expectedResult.NodeName = "WelcomeNewUser";
            expectedResult.HasCardResponse = true;

            StoryRequest request = context.CreateLaunchRequest();
            StoryResponse resp = await ValidationUtility.ValidateNode(expectedResult, storyRepProc, request, userRepo, linker, sessLogger);
            await ValidationUtility.ValidateGoogleResponse(resp, surfaceCaps, linker, logger, request.UserId, "prefix");


            // (this StoryResponse response, SurfaceCapabilities surfaceCaps, IMediaLinker mediaLinker, ILogger responseLogger, string userId, string contextPrefix)

            expectedResult.NodeName = "A1";
            expectedResult.HasCardResponse = true;

            request = context.CreateIntentRequest("BeginIntent");
            resp = await ValidationUtility.ValidateNode(expectedResult, storyRepProc, request, userRepo, linker, sessLogger);
            await ValidationUtility.ValidateGoogleResponse(resp, surfaceCaps, linker, logger, request.UserId, "prefix");




            //expectedResult.NodeName = "SymbicortRegularDiscountEnroll";
            //expectedResult.HasCardResponse = true;
            //expectedResult.HasCardButtons = true;
            //// Return Yes.
            //request = context.CreateIntentRequest("NoIntent");
            //var googleEndCardResult = await ValidationUtility.ValidateNode(expectedResult, storyRepProc, request, userRepo, linker, sessLogger, false);




        }
    }
}
