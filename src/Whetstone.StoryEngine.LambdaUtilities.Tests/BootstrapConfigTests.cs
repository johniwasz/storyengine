using Amazon.Lambda.TestUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Whetstone.StoryEngine.LambdaUtilities.ConfigUpdate;
using Whetstone.StoryEngine.Models.Configuration;
using Xunit;

namespace Whetstone.StoryEngine.LambdaUtilities.Tests
{
    public class BootstrapConfigTests
    {

        [Fact]
        public async Task SendCognitoUpdateRequestAsync()
        {
            System.Environment.SetEnvironmentVariable(StoryEngine.ContainerSettingsReader.AWSDEFAULTREGION, "us-east-1");


            string text = File.ReadAllText("Messages/CognitoServiceUpdate.json");

            CustomResourceRequest resourceReq = JsonConvert.DeserializeObject<CustomResourceRequest>(text);



            TestLambdaContext testContext = new TestLambdaContext();

            BootstrapConfigFunction configFunction = new BootstrapConfigFunction();

            await configFunction.FunctionHandler(resourceReq, testContext);


        }



        [Fact]
        public async Task BuildCognitoClientSecretRequestAsync()
        {
            System.Environment.SetEnvironmentVariable(StoryEngine.ContainerSettingsReader.AWSDEFAULTREGION, "us-east-1");
            ConfigUpdateRequest updateRequest = new ConfigUpdateRequest
            {
                ConfigEntries = new List<ConfigEntry>(),
                Parameter = "/storyengine/dev/bootstrap"
            };

            updateRequest.ConfigEntries.Add(new ConfigEntry
            { ConfigType = ConfigEntryType.CognitoUserClientSecret, Value = new string[] { "us-east-1_xu0geY2cC", "4o29k16f10ir3o7job8mmiabad" } });

            string text = File.ReadAllText("Messages/CognitoUpdate.json");

            CustomResourceRequest resourceReq = JsonConvert.DeserializeObject<CustomResourceRequest>(text);

            resourceReq.ResourceProperties = JObject.FromObject(updateRequest);
            resourceReq.RequestType = ResourceRequestType.Create;

            TestLambdaContext testContext = new TestLambdaContext();

            BootstrapConfigFunction configFunction = new BootstrapConfigFunction();

            await configFunction.FunctionHandler(resourceReq, testContext);


        }


        [Fact]
        public async Task BuildCognitoRequestAsync()
        {
            System.Environment.SetEnvironmentVariable(StoryEngine.ContainerSettingsReader.AWSDEFAULTREGION, "us-east-1");
            ConfigUpdateRequest updateRequest = new ConfigUpdateRequest
            {
                ConfigEntries = new List<ConfigEntry>(),
                Parameter = "/storyengine/dev/bootstrap"
            };

            //updateRequest.KeyId = "alias/devEnvironmentKey";
            //updateRequest.ConfigEntries.Add(new ConfigEntry
            //    { ConfigType = ConfigEntryType.ReportBucket, Value = "anotherreportbucketname" });


            updateRequest.ConfigEntries.Add(new ConfigEntry
            { ConfigType = ConfigEntryType.CognitoUserPoolId, Value = "us-east-1_xu0geY2cC" });

            string text = File.ReadAllText("Messages/CognitoUpdate.json");

            CustomResourceRequest resourceReq = JsonConvert.DeserializeObject<CustomResourceRequest>(text);

            resourceReq.ResourceProperties = JObject.FromObject(updateRequest);
            resourceReq.RequestType = ResourceRequestType.Create;

            TestLambdaContext testContext = new TestLambdaContext();

            BootstrapConfigFunction configFunction = new BootstrapConfigFunction();

            await configFunction.FunctionHandler(resourceReq, testContext);


        }

        [Fact]
        public async Task BuildDeleteRequestAsync()
        {
            System.Environment.SetEnvironmentVariable(StoryEngine.ContainerSettingsReader.AWSDEFAULTREGION, "us-east-1");
            ConfigUpdateRequest updateRequest = new ConfigUpdateRequest();
            updateRequest.ConfigEntries = new List<ConfigEntry>();
            updateRequest.Parameter = "/storyengine/dev/bootstrap";

            //updateRequest.KeyId = "alias/devEnvironmentKey";
            //updateRequest.ConfigEntries.Add(new ConfigEntry
            //    { ConfigType = ConfigEntryType.ReportBucket, Value = "anotherreportbucketname" });


            updateRequest.ConfigEntries.Add(new ConfigEntry
            { ConfigType = ConfigEntryType.SessionAuditQueue, Value = "WhetstoneQueue-Dev-SessionAuditQueue-ZQNCFM1I9XSL" });

            string text = File.ReadAllText("Messages/ConfigUpdateRequest.json");

            CustomResourceRequest resourceReq = JsonConvert.DeserializeObject<CustomResourceRequest>(text);

            resourceReq.ResourceProperties = JObject.FromObject(updateRequest);
            resourceReq.RequestType = ResourceRequestType.Delete;

            TestLambdaContext testContext = new TestLambdaContext();

            BootstrapConfigFunction configFunction = new BootstrapConfigFunction();

            await configFunction.FunctionHandler(resourceReq, testContext);


        }

        [Fact]
        public async Task BuildRequestAsync()
        {
            System.Environment.SetEnvironmentVariable(StoryEngine.ContainerSettingsReader.AWSDEFAULTREGION, "us-east-1");
            ConfigUpdateRequest updateRequest = new ConfigUpdateRequest();
            updateRequest.ConfigEntries = new List<ConfigEntry>();
            updateRequest.Parameter = "/storyengine/dev/bootstrap";
            //updateRequest.KeyId = "alias/devEnvironmentKey";
            updateRequest.ConfigEntries.Add(new ConfigEntry
            { ConfigType = ConfigEntryType.TwilioTestKey, Value = "dsfdsfL" });

            string text = File.ReadAllText("Messages/ConfigUpdateRequest.json");

            CustomResourceRequest resourceReq = JsonConvert.DeserializeObject<CustomResourceRequest>(text);

            resourceReq.ResourceProperties = JObject.FromObject(updateRequest);


            TestLambdaContext testContext = new TestLambdaContext();

            BootstrapConfigFunction configFunction = new BootstrapConfigFunction();

            await configFunction.FunctionHandler(resourceReq, testContext);


        }

        [Fact]
        public async Task BuildRequestIntAsync()
        {
            System.Environment.SetEnvironmentVariable(StoryEngine.ContainerSettingsReader.AWSDEFAULTREGION, "us-east-1");
            ConfigUpdateRequest updateRequest = new ConfigUpdateRequest();
            updateRequest.ConfigEntries = new List<ConfigEntry>();
            updateRequest.Parameter = "/storyengine/dev/bootstrap";
            //updateRequest.KeyId = "alias/devEnvironmentKey";
            updateRequest.ConfigEntries.Add(new ConfigEntry
            { ConfigType = ConfigEntryType.DynamoDBMaxErrorRetries, Value = "1" });

            string text = File.ReadAllText("Messages/ConfigUpdateRequest.json");

            CustomResourceRequest resourceReq = JsonConvert.DeserializeObject<CustomResourceRequest>(text);

            resourceReq.ResourceProperties = JObject.FromObject(updateRequest);


            TestLambdaContext testContext = new TestLambdaContext();

            BootstrapConfigFunction configFunction = new BootstrapConfigFunction();

            await configFunction.FunctionHandler(resourceReq, testContext);


        }

        [Fact]
        public async Task BuildRequestEnumAsync()
        {
            System.Environment.SetEnvironmentVariable(StoryEngine.ContainerSettingsReader.AWSDEFAULTREGION, "us-east-1");
            ConfigUpdateRequest updateRequest = new ConfigUpdateRequest();
            updateRequest.ConfigEntries = new List<ConfigEntry>();
            updateRequest.Parameter = "/storyengine/dev/bootstrap";
            //updateRequest.KeyId = "alias/devEnvironmentKey";
            updateRequest.ConfigEntries.Add(new ConfigEntry
            { ConfigType = ConfigEntryType.DefaultSmsSenderType, Value = "TwilioTestKey" });

            string text = File.ReadAllText("Messages/ConfigUpdateRequest.json");

            CustomResourceRequest resourceReq = JsonConvert.DeserializeObject<CustomResourceRequest>(text);

            resourceReq.ResourceProperties = JObject.FromObject(updateRequest);


            TestLambdaContext testContext = new TestLambdaContext();

            BootstrapConfigFunction configFunction = new BootstrapConfigFunction();

            await configFunction.FunctionHandler(resourceReq, testContext);


        }

        [Fact]
        public async Task BuildSocketConfigUpdateRequest()
        {
            System.Environment.SetEnvironmentVariable(StoryEngine.ContainerSettingsReader.AWSDEFAULTREGION, "us-east-1");
            ConfigUpdateRequest updateRequest = new ConfigUpdateRequest();
            updateRequest.ConfigEntries = new List<ConfigEntry>();
            updateRequest.Parameter = "/storyengine/dev/bootstrap";
            //updateRequest.KeyId = "alias/devEnvironmentKey";
            updateRequest.ConfigEntries.Add(new ConfigEntry
            { ConfigType = ConfigEntryType.SocketConnectionTableName, Value = "Whetstone-SocketInfrastructure-Dev-ConnectionMappingTable-10XOI0KOMXQNA" });
            updateRequest.ConfigEntries.Add(new ConfigEntry
            { ConfigType = ConfigEntryType.SocketWriteEndpoint, Value = "https://99xyut9t7d.execute-api.us-east-1.amazonaws.com/Prod" });
            updateRequest.ConfigEntries.Add(new ConfigEntry
            { ConfigType = ConfigEntryType.PendingNotificationsTableName, Value = "Whetstone-SocketInfrastructure-Dev-PendingNotificationsTable-1NMVCJRV5JHNX" });
            updateRequest.ConfigEntries.Add(new ConfigEntry
            { ConfigType = ConfigEntryType.NotificationsLambdaArn, Value = "arn:aws:lambda:us-east-1:940085449815:function:Whetstone-Notifications-Dev-NotificationsFunction-AXOG9KIX5NS9" });
            updateRequest.ConfigEntries.Add(new ConfigEntry
            { ConfigType = ConfigEntryType.NotificationsLambdaName, Value = "Whetstone-Notifications-Dev-NotificationsFunction-AXOG9KIX5NS9" });

            string text = File.ReadAllText("Messages/ConfigUpdateRequest.json");

            CustomResourceRequest resourceReq = JsonConvert.DeserializeObject<CustomResourceRequest>(text);

            resourceReq.ResourceProperties = JObject.FromObject(updateRequest);

            TestLambdaContext testContext = new TestLambdaContext();

            BootstrapConfigFunction configFunction = new BootstrapConfigFunction();

            await configFunction.FunctionHandler(resourceReq, testContext);

        }


    }
}
