using Amazon.Lambda.TestUtilities;
using MartinCostello.Testing.AwsLambdaTestServer;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Whetstone.Alexa;
using Xunit;

namespace Whetstone.StoryEngine.AlexaFunction.Test
{
    public class NativeFunctionTests : LambdaTestBase
    {

        [Fact(DisplayName = "Launch Test")]
        public void LaunchTest()
        {


            const string testDataFile = @"RequestSamples\LaunchSkillRequest.json";

            string jsonRawText = File.ReadAllText(testDataFile);

            AlexaRequest req = JsonConvert.DeserializeObject<AlexaRequest>(jsonRawText);

            var context = new TestLambdaContext();


            TestClientContext testContext = new TestClientContext();
            System.Environment.SetEnvironmentVariable("ENVIRONMENT", "dev");
            System.Environment.SetEnvironmentVariable("LOGLEVEL", "Debug");
            System.Environment.SetEnvironmentVariable("AWS_LAMBDA_RUNTIME_API", "localhost");
            System.Environment.SetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME", "customfunc");

            context.ClientContext = testContext;
            var function = new AlexaFunctionProxy();

            var testLogger = context.Logger as TestLambdaLogger;

            string testLog = testLogger.Buffer.ToString();

            Debug.WriteLine(testLog);

            // Assert.Contains("Stream processing complete", testLogger.Buffer.ToString());
        }

    }
}
