using Amazon.Lambda.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Whetstone.Alexa;
using Xunit;

namespace Whetstone.StoryEngine.AlexaFunction.Test
{
    public class FunctionTest
    {
        [Fact(DisplayName = "Launch Test")]
        public void LaunchTest()
        {

            IServiceCollection serviceCollection = new ServiceCollection();


            const string testDataFile = @"RequestSamples\LaunchSkillRequest.json";

            string jsonRawText = File.ReadAllText(testDataFile);

            AlexaRequest req = JsonConvert.DeserializeObject<AlexaRequest>(jsonRawText);

            var context = new TestLambdaContext();


            TestClientContext testContext = new TestClientContext();
            System.Environment.SetEnvironmentVariable("ENVIRONMENT", "dev");
            System.Environment.SetEnvironmentVariable("LOGLEVEL", "Debug");


            //testContext.Environment = new Dictionary<string, string>();
            //testContext.Environment.Add("LOGLEVEL", "Debug");
            //testContext.Environment.Add("RUNMODE", "Testing");
            //testContext.Environment.Add("CONTAINER", "dev.sbsstoryengine");

            context.ClientContext = testContext;
            var function = new AlexaFunctionProxy();

            var returnVal = function.FunctionHandlerAsync(req, context);

            var testLogger = context.Logger as TestLambdaLogger;


            string testLog = testLogger.Buffer.ToString();

            Debug.WriteLine(testLog);

            // Assert.Contains("Stream processing complete", testLogger.Buffer.ToString());
        }

        [Fact(DisplayName = "Restart Test")]
        public void RestartTest()
        {

            IServiceCollection serviceCollection = new ServiceCollection();


            const string testDataFile = @"RequestSamples\StartOverRequest.json";

            string jsonRawText = File.ReadAllText(testDataFile);

            AlexaRequest req = JsonConvert.DeserializeObject<AlexaRequest>(jsonRawText);

            var context = new TestLambdaContext();


            TestClientContext testContext = new TestClientContext();
            System.Environment.SetEnvironmentVariable("ENVIRONMENT", "dev");
            System.Environment.SetEnvironmentVariable("LOGLEVEL", "Debug");


            //testContext.Environment = new Dictionary<string, string>();
            //testContext.Environment.Add("LOGLEVEL", "Debug");
            //testContext.Environment.Add("RUNMODE", "Testing");
            //testContext.Environment.Add("CONTAINER", "dev.sbsstoryengine");

            context.ClientContext = testContext;
            var function = new AlexaFunctionProxy();

            var returnVal = function.FunctionHandlerAsync(req, context);

            var testLogger = context.Logger as TestLambdaLogger;


            string testLog = testLogger.Buffer.ToString();

            Debug.WriteLine(testLog);

            // Assert.Contains("Stream processing complete", testLogger.Buffer.ToString());
        }


        [Fact(DisplayName = "Empty Slot Test")]
        public void DeserializeEmptySlot()
        {
            const string TestDataFile = @"RequestSamples\TakeNothingSample.json";

            string jsonRawText = File.ReadAllText(TestDataFile);

            AlexaRequest req = JsonConvert.DeserializeObject<AlexaRequest>(jsonRawText);

        }

        [Fact(DisplayName = "Begin Test")]
        public void BeginIntent()
        {

            IServiceCollection serviceCollection = new ServiceCollection();


            const string TestDataFile = @"RequestSamples\StartRequestIntent.json";

            string jsonRawText = File.ReadAllText(TestDataFile);

            AlexaRequest req = JsonConvert.DeserializeObject<AlexaRequest>(jsonRawText);

            var context = new TestLambdaContext();


            TestClientContext testContext = new TestClientContext();
            testContext.Environment = new Dictionary<string, string>();
            System.Environment.SetEnvironmentVariable("ENVIRONMENT", "dev");
            System.Environment.SetEnvironmentVariable("LOGLEVEL", "Debug");

            context.ClientContext = testContext;
            var function = new AlexaFunctionProxy();

            var returnVal = function.FunctionHandlerAsync(req, context);

            string textRespose = JsonConvert.SerializeObject(returnVal);
            Amazon.Lambda.Serialization.Json.JsonSerializer ser = new Amazon.Lambda.Serialization.Json.JsonSerializer();


            string alexaResponse = null;
            using (MemoryStream memStream = new MemoryStream())
            {
                ser.Serialize(returnVal, memStream);

                memStream.Position = 0;
                using (var reader = new StreamReader(memStream))
                {
                    alexaResponse = reader.ReadToEnd();
                }
            }



            var testLogger = context.Logger as TestLambdaLogger;

        }



        [Fact(DisplayName = "Left Intent")]
        public void LeftIntent()
        {



            const string TestDataFile = @"RequestSamples\GoLeft.json";

            string jsonRawText = File.ReadAllText(TestDataFile);

            AlexaRequest req = JsonConvert.DeserializeObject<AlexaRequest>(jsonRawText);

            var context = new TestLambdaContext();


            TestClientContext testContext = new TestClientContext();
            testContext.Environment = new Dictionary<string, string>();
            testContext.Environment.Add("LOGLEVEL", "Debug");
            testContext.Environment.Add("RUNMODE", "Testing");

            context.ClientContext = testContext;
            var function = new AlexaFunctionProxy();

            var returnVal = function.FunctionHandlerAsync(req, context);

            var testLogger = context.Logger as TestLambdaLogger;

            // Assert.Contains("Stream processing complete", testLogger.Buffer.ToString());
        }



    }
}
