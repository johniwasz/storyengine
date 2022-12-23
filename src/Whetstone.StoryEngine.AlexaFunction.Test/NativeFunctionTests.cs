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

            //testContext.Environment = new Dictionary<string, string>();
            //testContext.Environment.Add("LOGLEVEL", "Debug");
            //testContext.Environment.Add("RUNMODE", "Testing");
            //testContext.Environment.Add("CONTAINER", "dev.sbsstoryengine");

            context.ClientContext = testContext;
            var function = new AlexaFunctionProxy();

            Whetstone.StoryEngine.AlexaFunction.Program natFunc = new Program();

           // await Native.Function.Main(null);


            // var returnVal = await Native.Function.FunctionHandler(req, context);

          // string returnVal = Native.Function.FunctionHandler("sometext", context);

            var testLogger = context.Logger as TestLambdaLogger;


            string testLog = testLogger.Buffer.ToString();

            Debug.WriteLine(testLog);

            // Assert.Contains("Stream processing complete", testLogger.Buffer.ToString());
        }


        [Fact]
        public async Task ProcessAlexaRequest()
        {

            const string testDataFile = @"RequestSamples\LaunchSkillRequest.json";

            string jsonRawText = File.ReadAllText(testDataFile);

            AlexaRequest req = JsonConvert.DeserializeObject<AlexaRequest>(jsonRawText);

            req.Context.System.Application.ApplicationId = "amzn1.ask.skill.92304d4d-42a5-4371-9b13-97b4a79b9ad0";
            req.Session.Application.ApplicationId = req.Context.System.Application.ApplicationId;
            req.Request.Timestamp = DateTime.UtcNow;

            jsonRawText = JsonConvert.SerializeObject(req);


            // Arrange
            using (LambdaTestServer server = new LambdaTestServer())
            {
                using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1)))
                {

                    await server.StartAsync(cancellationTokenSource.Token);


                    // string json = JsonConvert.SerializeObject(value);

                    string jsonResponse;
                    LambdaTestContext context = await server.EnqueueAsync(jsonRawText);

                    using (var httpClient = server.CreateClient())
                    {

                        // Act
                        await Program.RunAsync(httpClient, cancellationTokenSource.Token).ContinueWith(async x =>
                        {
                            // Assert
                            Assert.True(context.Response.TryRead(out LambdaTestResponse response));
                            Assert.True(response.IsSuccessful);

                            jsonResponse = await response.ReadAsStringAsync();

                            AlexaResponse actual = JsonConvert.DeserializeObject<AlexaResponse>(jsonResponse);

                           // Assert.Equal(new[] { 3, 2, 1 }, actual);
                        });

                    }
                }
            }
        }
    }
}
