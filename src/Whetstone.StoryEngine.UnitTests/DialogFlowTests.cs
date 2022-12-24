using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Google.Repository;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Repository;
using Xunit;

namespace Whetstone.UnitTests
{
    public class DialogFlowTests
    {



        [Fact]
        public async Task WalkWhetstoneDialogFlow()
        {

            JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

            TitleVersion titleVer = TitleVersionUtil.GetWhetstoneTitle();

            string dialogFlowRequestText = File.ReadAllText("DialogFlowMessages/whetstonedialogflowpost.json");

            APIGatewayProxyRequest proxyRequest =
                JsonConvert.DeserializeObject<APIGatewayProxyRequest>(dialogFlowRequestText);

            string coreDialogFlowText = proxyRequest.Body;

            WebhookRequest coreWebHookRequest = jsonParser.Parse<WebhookRequest>(coreDialogFlowText);


            DialogFlowRepository dialogRep = GetDialogFlowRepository(titleVer);


            ILambdaContext lambdaConfig = Mock.Of<ILambdaContext>();
            APIGatewayProxyResponse proxyResponse =
                await dialogRep.ProcessDialogFlowRequestAsync(proxyRequest, lambdaConfig);

            Assert.True(proxyResponse.StatusCode == 200, $"Unexpected status code returned {proxyResponse.StatusCode}");


            WebhookResponse webHookResp = null;

            webHookResp = jsonParser.Parse<WebhookResponse>(proxyResponse.Body);

            Assert.True(webHookResp != null, "WebHook response is null");
            WriteWebhookResponse(webHookResp);



            string fileContents = File.ReadAllText("DialogFlowMessages/whetstonedialogflow_sonibridge.json");
            proxyRequest.Body = fileContents;

            proxyResponse =
                 await dialogRep.ProcessDialogFlowRequestAsync(proxyRequest, lambdaConfig);
            webHookResp = jsonParser.Parse<WebhookResponse>(proxyResponse.Body);
            Assert.True(webHookResp != null, "WebHook response is null");
            WriteWebhookResponse(webHookResp);

        }

#pragma warning disable IDE0051 // Remove unused private members
        private WebhookRequest LoadWebhookRequest(string filePath)
#pragma warning restore IDE0051 // Remove unused private members
        {
            string dialogFlowRequestText = File.ReadAllText($"DialogFlowMessages/{filePath}");
            JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

            WebhookRequest coreWebHookRequest = jsonParser.Parse<WebhookRequest>(dialogFlowRequestText);

            return coreWebHookRequest;
        }


        private void WriteWebhookResponse(WebhookResponse webHookResponse)
        {

            if (webHookResponse.FulfillmentMessages != null)
            {

                foreach (var hookMessage in webHookResponse.FulfillmentMessages)
                {

                    if (hookMessage.SimpleResponses?.SimpleResponses_ != null)
                    {
                        foreach (var simpleResponse in hookMessage.SimpleResponses.SimpleResponses_)
                        {
                            Debug.WriteLine(simpleResponse.Ssml);

                        }


                    }

                }



            }


        }


        [Theory]
        //  [InlineData("DialogFlow-FacebookMessengerWelcome.json")]
        [InlineData("DialogFlow-FacebookMessengerSonibridge.json")]
        public async Task DeserializeWhetstoneFacebook(string messageFile)
        {
            var mocker = new MockFactory();

            TitleVersion titleVer = TitleVersionUtil.GetWhetstoneTitle();

            IServiceCollection servCol = mocker.InitServiceCollection(titleVer);

            IServiceProvider servProv = servCol.BuildServiceProvider();

            IAppMappingReader mappingReader = servProv.GetService<IAppMappingReader>();

            ILogger<DialogFlowRepository> repLogger = servProv.GetService<ILogger<DialogFlowRepository>>();


            JsonParser jsonParser =
                new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

            string dialogFlowRequestText =
                File.ReadAllText($"DialogFlowMessages/{messageFile}");

            var request = jsonParser.Parse<WebhookRequest>(dialogFlowRequestText);


            await request.ToStoryRequestAsync(mappingReader, "LIVE", repLogger);
        }



        [Fact]
        public async Task ProcessWhetstoneFacebookWelcome()
        {

            TitleVersion titleVer = TitleVersionUtil.GetWhetstoneTitle();

            DialogFlowRepository dialogRep = GetDialogFlowRepository(titleVer);


            JsonParser jsonParser =
                new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

            string apiRequest = File.ReadAllText("DialogFlowMessages/whetstonedialogflowpost.json");
            APIGatewayProxyRequest proxyRequest =
                JsonConvert.DeserializeObject<APIGatewayProxyRequest>(apiRequest);

            string dialogFlowRequestText =
                File.ReadAllText("DialogFlowMessages/DialogFlow-FacebookMessengerWelcome.json");

            proxyRequest.Body = dialogFlowRequestText;

            ILambdaContext lambdaConfig = Mock.Of<ILambdaContext>();

            APIGatewayProxyResponse proxyResponse = await dialogRep.ProcessDialogFlowRequestAsync(proxyRequest, lambdaConfig);

            WebhookResponse webHookResp = jsonParser.Parse<WebhookResponse>(proxyResponse.Body);

            Assert.True(webHookResp != null, "WebHook response is null");

        }


        [Fact]
        public async Task LaunchDelayedApiRequest()
        {
            TitleVersion titleVer = TitleVersionUtil.GetAnimalFarmPITitle();

            string dialogFlowRequestText = File.ReadAllText("DialogFlowMessages/ApiTimeout.json");

            APIGatewayProxyRequest proxyRequest =
                JsonConvert.DeserializeObject<APIGatewayProxyRequest>(dialogFlowRequestText);


            DialogFlowRepository dialogRep = GetDialogFlowRepository(titleVer);


            ILambdaContext lambdaConfig = Mock.Of<ILambdaContext>();
            APIGatewayProxyResponse proxyResponse =
                await dialogRep.ProcessDialogFlowRequestAsync(proxyRequest, lambdaConfig);

            Assert.True(proxyResponse.StatusCode == 200, $"Unexpected status code returned {proxyResponse.StatusCode}");


            WebhookResponse webHookResp = null;
            JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

            webHookResp = jsonParser.Parse<WebhookResponse>(proxyResponse.Body);

            Assert.True(webHookResp != null, "WebHook response is null");


        }

        [Fact]
        public async Task LaunchWhetstoneDialogFlow()
        {
            TitleVersion titleVer = TitleVersionUtil.GetWhetstoneTitle();

            string dialogFlowRequestText = File.ReadAllText("DialogFlowMessages/whetstonedialogflowpost.json");

            APIGatewayProxyRequest proxyRequest =
                JsonConvert.DeserializeObject<APIGatewayProxyRequest>(dialogFlowRequestText);


            DialogFlowRepository dialogRep = GetDialogFlowRepository(titleVer);


            ILambdaContext lambdaConfig = Mock.Of<ILambdaContext>();
            APIGatewayProxyResponse proxyResponse =
                await dialogRep.ProcessDialogFlowRequestAsync(proxyRequest, lambdaConfig);

            Assert.True(proxyResponse.StatusCode == 200, $"Unexpected status code returned {proxyResponse.StatusCode}");


            WebhookResponse webHookResp = null;
            JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

            webHookResp = jsonParser.Parse<WebhookResponse>(proxyResponse.Body);

            Assert.True(webHookResp != null, "WebHook response is null");


        }

        [Fact]
        public async Task LaunchWhetstoneDialogFlowBadRequest()
        {
            TitleVersion titleVer = TitleVersionUtil.GetWhetstoneTitle();

            string dialogFlowRequestText = File.ReadAllText("DialogFlowMessages/whetstonedialogflowpost.json");

            ILambdaContext lambdaConfig = Mock.Of<ILambdaContext>();

            APIGatewayProxyRequest proxyRequest =
                JsonConvert.DeserializeObject<APIGatewayProxyRequest>(dialogFlowRequestText);

            proxyRequest.Path = "/api/badpath";


            var dialogRep = GetDialogFlowRepository(titleVer);

            APIGatewayProxyResponse proxyResponse =
                await dialogRep.ProcessDialogFlowRequestAsync(proxyRequest, lambdaConfig);

            Assert.True(proxyResponse.StatusCode == 400, $"Unexpected status code returned {proxyResponse.StatusCode}");

        }


        [Fact]
        public async Task LaunchHealthCheckRequest()
        {
            TitleVersion titleVer = TitleVersionUtil.GetWhetstoneTitle();

            string dialogFlowRequestText = File.ReadAllText("DialogFlowMessages/whetstoneapihealthcheck.json");

            ILambdaContext lambdaConfig = Mock.Of<ILambdaContext>();

            APIGatewayProxyRequest proxyRequest =
                JsonConvert.DeserializeObject<APIGatewayProxyRequest>(dialogFlowRequestText);

            var dialogRep = GetDialogFlowRepository(titleVer);

            APIGatewayProxyResponse proxyResponse =
                await dialogRep.ProcessDialogFlowRequestAsync(proxyRequest, lambdaConfig);

            Assert.True(proxyResponse.StatusCode == 200, $"Unexpected status code returned {proxyResponse.StatusCode}");

        }

        private static DialogFlowRepository GetDialogFlowRepository(TitleVersion titleVer)
        {
            var mocker = new MockFactory();

            IServiceCollection servCol = mocker.InitServiceCollection(titleVer);

            IServiceProvider servProv = servCol.BuildServiceProvider();

            IOptionsSnapshot<AuditClientMessagesConfig> auditConfig =
                servProv.GetService<IOptionsSnapshot<AuditClientMessagesConfig>>();

            IStoryRequestProcessor storyProc = servProv.GetService<IStoryRequestProcessor>();
            IMediaLinker linker = servProv.GetService<IMediaLinker>();
            IAppMappingReader mappingReader = servProv.GetService<IAppMappingReader>();
            ISessionLogger sessionLogger = servProv.GetService<ISessionLogger>();
            ILogger<DialogFlowRepository> repLogger = servProv.GetService<ILogger<DialogFlowRepository>>();

            DialogFlowRepository dialogRep =
                new DialogFlowRepository(auditConfig, storyProc, linker, mappingReader, sessionLogger, repLogger);
            return dialogRep;
        }


        [Fact]
        public async Task DialogFlowEmptyTest()
        {
            var requestStr = await File.ReadAllTextAsync("./DialogFlowMessages/DialogFlow-BadEmpty.json");

            JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
            WebhookRequest webReq = jsonParser.Parse<WebhookRequest>(requestStr);

            var appMappingReader = new Mock<IAppMappingReader>();


            appMappingReader.Setup(x => x.GetTitleAsync(It.IsAny<Client>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((Client clientType, string appId, string alias) =>
                {
                    TitleVersion titleVersion = new TitleVersion
                    {
                        ShortName = "animalfarmpi",
                        Version = "1.0",
                        Alias = alias
                    };
                    return titleVersion;
                });


            var loggerMock = new Mock<ILogger<DialogFlowTests>>();

            StoryRequest storyReq = await webReq.ToStoryRequestAsync(appMappingReader.Object, null, loggerMock.Object);



            Assert.True(storyReq.IntentConfidence.HasValue);

            Assert.True(!string.IsNullOrWhiteSpace(storyReq.RawText));

        }




        [Fact(DisplayName = "WebHook To StoryRequest Test")]
        public async Task DialogFlowGetQueryTextTest()
        {
            var requestStr = await File.ReadAllTextAsync("./DialogFlowMessages/DialogFlow-QueryText.json");

            JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
            WebhookRequest webReq = jsonParser.Parse<WebhookRequest>(requestStr);

            var appMappingReader = new Mock<IAppMappingReader>();


            _ = appMappingReader.Setup(x => x.GetTitleAsync(It.IsAny<Client>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((Client clientType, string appId, string alias) =>
                {
                    TitleVersion titleVersion = new TitleVersion
                    {
                        ShortName = "animalfarmpi",
                        Version = "1.0",
                        DeploymentId = Guid.NewGuid(),
                        VersionId = Guid.NewGuid(),
                        Alias = alias
                    };
                    return titleVersion;
                });


            var loggerMock = new Mock<ILogger<DialogFlowTests>>();

            StoryRequest storyReq = await webReq.ToStoryRequestAsync(appMappingReader.Object, null, loggerMock.Object);


            Assert.True(storyReq.IntentConfidence.HasValue);

            Assert.True(!string.IsNullOrWhiteSpace(storyReq.RawText));

        }

        [Fact]
        public async Task ProcessMessageRequestAsync()
        {
            string healthCheckText = File.ReadAllText("./DialogFlowMessages/googlerequest.json");

            string dialogFlowRequestText = File.ReadAllText("DialogFlowMessages/whetstonedialogflowpost.json");

            APIGatewayProxyRequest proxyRequest =
                JsonConvert.DeserializeObject<APIGatewayProxyRequest>(dialogFlowRequestText);

            proxyRequest.Body = healthCheckText;

            TitleVersion titleVer = TitleVersionUtil.GetWhetstoneTitle();

            DialogFlowRepository dialogRep = GetDialogFlowRepository(titleVer);


            ILambdaContext lambdaConfig = Mock.Of<ILambdaContext>();
            APIGatewayProxyResponse proxyResponse =
                await dialogRep.ProcessDialogFlowRequestAsync(proxyRequest, lambdaConfig);





        }


        [Fact]
        public void GetCapabilitiesTest()
        {
            string fileText = File.ReadAllText(@"DialogFlowMessages/googlerequest.json");

            JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
            WebhookRequest req = jsonParser.Parse<WebhookRequest>(fileText);

            var surfaceCaps = req.GetSurfaceCapabilities();



        }


        [Fact(DisplayName = "Validate Health check creates ping request")]
        public async Task ProcessHealthCheckAsync()
        {
            string healthCheckText = File.ReadAllText("./DialogFlowMessages/whetstoneapihealthcheck.json");


            APIGatewayProxyRequest apiRequest = JsonConvert.DeserializeObject<APIGatewayProxyRequest>(healthCheckText);

            string webHookBodyText = apiRequest.Body;
            JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
            WebhookRequest req = jsonParser.Parse<WebhookRequest>(webHookBodyText);


            var appMappingReader = new Mock<IAppMappingReader>();


            appMappingReader.Setup(x => x.GetTitleAsync(It.IsAny<Client>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((Client clientType, string appId, string alias) =>
                {
                    TitleVersion titleVersion = TitleVersionUtil.GetWhetstoneTitle();
                    return titleVersion;
                });

            var loggerMock = new Mock<ILogger<DialogFlowTests>>();

            StoryRequest storyReq = await req.ToStoryRequestAsync(appMappingReader.Object, null, loggerMock.Object);

            Assert.True(storyReq.IsPingRequest.GetValueOrDefault(false) == true);

        }

        [Fact]
        public void LoadRequest()
        {
            string fileText = File.ReadAllText(@"DialogFlowMessages/googlerequest.json");

            JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
            WebhookRequest req = jsonParser.Parse<WebhookRequest>(fileText);

            StoryRequest storyReq = new StoryRequest();

            string session = req.Session;

            string[] sessionParts = session.Split('/');

            storyReq.ApplicationId = sessionParts[1];

            storyReq.SessionId = session;

            storyReq.RequestId = req.ResponseId;

            var userStruct = req.OriginalDetectIntentRequest?.Payload?.Fields?["user"];

            storyReq.UserId = userStruct?.StructValue?.Fields?["userId"]?.StringValue;

            storyReq.Locale = req.QueryResult.LanguageCode;

            storyReq.Client = Client.GoogleHome;

            storyReq.RequestType = StoryRequestType.Intent;

            storyReq.Intent = req.QueryResult.Intent.DisplayName;

            var slotParams = req.QueryResult.Parameters;

            if (slotParams?.Fields?.Keys?.Count > 0)
            {
                storyReq.Slots = new Dictionary<string, string>();
                foreach (string slotKey in slotParams.Fields.Keys)
                {
                    string slotVal = slotParams.Fields[slotKey].StringValue;
                    storyReq.Slots.Add(slotKey, slotVal);
                }

            }
        }
    }
}

