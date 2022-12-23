using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Google.Repository;
using Whetstone.StoryEngine.Google.Repository.Models;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Repository;
using Whetstone.UnitTests;
using Xunit;
using MockFactory = Whetstone.UnitTests.MockFactory;

namespace Whetstone.StoryEngine.UnitTests
{
    public class AnimalFarmPIGoogleTests
    {

        //[Fact]
        //public async Task ProcessUnexpectedRequedstTest()
        //{

        //    TitleVersion titleVer = TitleVersionUtil.GetEOTEGTitle();

        //    var mocker = new MockFactory();

        //    IServiceCollection servCol = mocker.InitServiceCollection(titleVer);

        //    IServiceProvider servProv = servCol.BuildServiceProvider();

        //    IStoryRequestProcessor storyRepProc = servProv.GetRequiredService<IStoryRequestProcessor>();

        //    IMediaLinker linker = servProv.GetRequiredService<IMediaLinker>();
        //    Microsoft.Extensions.Logging.ILogger logger = Mock.Of<Microsoft.Extensions.Logging.ILogger>();


        //    IAppMappingReader appReader = mocker.GetMockAppMappingReader();

        //    WebhookRequest hookRequest = GetSessionRequest("DialogFlow-UnexpectedSession01.json");
        //    SurfaceCapabilities sufCaps = hookRequest.GetSurfaceCapabilities();

        //    StoryRequest storyReq = await hookRequest.ToStoryRequestAsync(appReader, null);

        //    StoryResponse resp =  await storyRepProc.ProcessStoryRequestAsync(storyReq);
        //    WebhookResponse hookResp =
        //      await resp.ToDialogFlowResponseAsync(sufCaps, linker, logger, storyReq.IsRegisteredUser, storyReq.SessionId);



        //    hookRequest = GetSessionRequest("DialogFlow-UnexpectedSession02.json");
        //    sufCaps = hookRequest.GetSurfaceCapabilities();

        //    storyReq = await hookRequest.ToStoryRequestAsync(appReader, null);

        //    resp = await storyRepProc.ProcessStoryRequestAsync(storyReq);
        //   hookResp =
        //        await resp.ToDialogFlowResponseAsync(sufCaps, linker, logger, storyReq.IsRegisteredUser, storyReq.SessionId);


        //   hookRequest = GetSessionRequest("DialogFlow-UnexpectedSession03.json");
        //   sufCaps = hookRequest.GetSurfaceCapabilities();

        //   storyReq = await hookRequest.ToStoryRequestAsync(appReader, null);

        //   resp = await storyRepProc.ProcessStoryRequestAsync(storyReq);
        //   hookResp =
        //       await resp.ToDialogFlowResponseAsync(sufCaps, linker, logger, storyReq.IsRegisteredUser, storyReq.SessionId);
        //}


        private WebhookRequest GetSessionRequest(string fileName)
        {

            string simulatorStartRequest = File.ReadAllText($"DialogFlowMessages/{fileName}");


            JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

            WebhookRequest request = jsonParser.Parse<WebhookRequest>(simulatorStartRequest);

            return request;
        }


        [Fact]
        public async Task AnimalFarmPIGoogleTest()
        {
            string simulatorStartRequest = File.ReadAllText("DialogFlowMessages/DialogFlow-SimulatorStart.json");

            JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
            WebhookRequest hookRequest = jsonParser.Parse<WebhookRequest>(simulatorStartRequest);
            
            var mocker = new MockFactory();

            IAppMappingReader appReader =  mocker.GetMockAppMappingReader();

            ILogger<AnimalFarmPIGoogleTests> logger = mocker.GetLogger<AnimalFarmPIGoogleTests>();

            StoryRequest storyReq = await hookRequest.ToStoryRequestAsync(appReader, null, logger);



        }

    }
}
