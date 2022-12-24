using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Whetstone.Google.Actions.V1;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Google.Repository;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Repository;
using Whetstone.UnitTests;
using Xunit;

namespace Whetstone.StoryEngine.UnitTests
{
    public class GoogleActionTests
    {


        [Fact(DisplayName = "LoadAlexaRequest")]
        public void LoadAlexaRequest()
        {



            string text = File.ReadAllText(@"GoogleActionMessages/Whetstone/ActionLaunchMessage.json");

            HandlerRequest actionRequest = HandlerRequest.FromJson(text);



            //AlexaRequest req = JsonConvert.DeserializeObject<AlexaRequest>(text);

            //var storyRequest = req.ToStoryRequest();



        }



        [Fact]
        public async Task WalkWhetstoneDialogFlowAsync()
        {

            TitleVersion titleVer = TitleVersionUtil.GetWhetstoneTitle();

            // Load the APIGateway wrapper message
            string dialogFlowRequestText = File.ReadAllText("DialogFlowMessages/whetstonedialogflowpost.json");

            APIGatewayProxyRequest proxyRequest =
                JsonConvert.DeserializeObject<APIGatewayProxyRequest>(dialogFlowRequestText);

            proxyRequest.QueryStringParameters.Add("appid", "ac1-whetstone");

            string coreDialogFlowText = proxyRequest.Body;

            // Get the raw launch request
            string text = File.ReadAllText(@"GoogleActionMessages/Whetstone/ActionLaunchMessage.json");
            //HandlerRequest actionRequest = HandlerRequest.FromJson(text);

            proxyRequest.Body = text;

            ActionV1Repository actionRep = GetActionV1Repository(titleVer);


            ILambdaContext lambdaConfig = Mock.Of<ILambdaContext>();
            APIGatewayProxyResponse proxyResponse =
                await actionRep.ProcessActionV1RequestAsync(proxyRequest, lambdaConfig);

            Assert.True(proxyResponse.StatusCode == 200, $"Unexpected status code returned {proxyResponse.StatusCode}");


            //WebhookResponse webHookResp = null;

            //webHookResp = jsonParser.Parse<WebhookResponse>(proxyResponse.Body);

            //Assert.True(webHookResp != null, "WebHook response is null");
            //WriteWebhookResponse(webHookResp);



            //string fileContents = File.ReadAllText("DialogFlowMessages/whetstonedialogflow_sonibridge.json");
            //proxyRequest.Body = fileContents;

            //proxyResponse =
            //     await dialogRep.ProcessDialogFlowRequestAsync(proxyRequest, lambdaConfig);

            //webHookResp = jsonParser.Parse<WebhookResponse>(proxyResponse.Body);

            //Assert.True(webHookResp != null, "WebHook response is null");

            //WriteWebhookResponse(webHookResp);

        }

        private static ActionV1Repository GetActionV1Repository(TitleVersion titleVer)
        {
            var mocker = new Whetstone.UnitTests.MockFactory();

            IServiceCollection servCol = mocker.InitServiceCollection(titleVer);

            IServiceProvider servProv = servCol.BuildServiceProvider();

            IOptionsSnapshot<AuditClientMessagesConfig> auditConfig =
                servProv.GetService<IOptionsSnapshot<AuditClientMessagesConfig>>();

            IStoryRequestProcessor storyProc = servProv.GetService<IStoryRequestProcessor>();
            IMediaLinker linker = servProv.GetService<IMediaLinker>();
            IAppMappingReader mappingReader = servProv.GetService<IAppMappingReader>();
            ISessionLogger sessionLogger = servProv.GetService<ISessionLogger>();
            ILogger<ActionV1Repository> repLogger = servProv.GetService<ILogger<ActionV1Repository>>();

            ActionV1Repository actionRep =
                new ActionV1Repository(auditConfig, storyProc, linker, mappingReader, sessionLogger, repLogger);

            return actionRep;
        }
    }
}
