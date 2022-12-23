using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Auth.AccessControlPolicy;
using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using Whetstone.StoryEngine.ConfigUtilities;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.ConfigUtilities.Tests
{
    public class FunctionTest
    {

        [Fact]
        public async Task BadRoleCreateTest()
        {
            System.Environment.SetEnvironmentVariable(StoryEngine.ContainerSettingsReader.AWSDEFAULTREGION, "us-east-1");



            string text = File.ReadAllText("Messages/BadRoleCreateRequest.json");

            CustomResourceRequest resourceReq = JsonConvert.DeserializeObject<CustomResourceRequest>(text);

            TestLambdaContext testContext = new TestLambdaContext();

            KeyPolicyCustomActionFunction policyFunction = new KeyPolicyCustomActionFunction();

             await policyFunction.FunctionHandlerAsync(resourceReq, testContext);


        }


        [Fact]
        public async Task CreateResourceTest()
        {
            System.Environment.SetEnvironmentVariable(StoryEngine.ContainerSettingsReader.AWSDEFAULTREGION, "us-east-1");



            string text = File.ReadAllText("Messages/CreateKeyPolicyUpdateRequest.json");

            CustomResourceRequest resourceReq = JsonConvert.DeserializeObject<CustomResourceRequest>(text);

            TestLambdaContext testContext = new TestLambdaContext();

            KeyPolicyCustomActionFunction policyFunction = new KeyPolicyCustomActionFunction();

            await policyFunction.FunctionHandlerAsync(resourceReq, testContext);


        }



        [Fact]
        public async Task DeleteResourceTest()
        {
            string text = File.ReadAllText("Messages/DeleteKeyPolicyRequest.json");

            CustomResourceRequest resourceReq = JsonConvert.DeserializeObject<CustomResourceRequest>(text);

            TestLambdaContext testContext = new TestLambdaContext();

            KeyPolicyCustomActionFunction policyFunction = new KeyPolicyCustomActionFunction();

            await policyFunction.FunctionHandlerAsync(resourceReq, testContext);


        }

        [Fact]
        public async Task TetGetMethod()
        {

           string envVar =  Environment.GetEnvironmentVariable("_LAMBDA_RUNTIME_LOAD_TIME");

            var loadTime =  TimeSpan.FromTicks(1858608824241);

            //DateTime curTime = 1603876105013.FromUnixTime();

           long curNanoTime =  GetNanoTime();


            long runtime = (curNanoTime - 1603876105013) / 1000000;


            var epochDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(1603876105013);




            CustomResourceRequest resourceReq = new CustomResourceRequest();
            resourceReq.RequestId = Guid.NewGuid().ToString();
            resourceReq.RequestType = ResourceRequestType.Create;
            resourceReq.ResourceType = Guid.NewGuid().ToString();
            resourceReq.ResponseUrl = "http://localhost";
            resourceReq.ServiceToken = Guid.NewGuid().ToString();
            resourceReq.StackId = Guid.NewGuid().ToString();
            resourceReq.PhysicalResourceId = Guid.NewGuid().ToString();
            resourceReq.LogicalResourceId = "MyResource";

            string jsonText = JsonConvert.SerializeObject(resourceReq, Formatting.Indented);

            TestLambdaContext testContext = new TestLambdaContext();
            KeyPolicyCustomActionFunction policyFunction = new KeyPolicyCustomActionFunction();
           await  policyFunction.FunctionHandlerAsync(resourceReq, testContext);
        }


        private static long GetNanoTime()
        {
            long nano = 10000L * Stopwatch.GetTimestamp();
            nano /= TimeSpan.TicksPerMillisecond;
            nano *= 100L;
            return nano;
        }


 
    }
}
