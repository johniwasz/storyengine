using Amazon.Lambda.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Whetstone.Alexa;
using Xunit;

namespace Whetstone.StoryEngine.AlexaFunction.Test
{
    public class PingRequestTest
    {
        private const string WHETSTONE_TECH_SKILL_ID = "amzn1.ask.skill.c4cabd50-2cd5-4e4c-a03c-a57d4f2a0e5f";
        private const string PING_SESSION_ID = "4D0682D2-94DD-4D8E-9D06-661DCC4CB63D";


        [Fact(DisplayName = "PingRequest")]
        public async Task PingRequest()
        {

            System.Environment.SetEnvironmentVariable("ENVIRONMENT", "dev");
            System.Environment.SetEnvironmentVariable("LOGLEVEL", "Debug");

// Removed unused serviceCollection declaration

            var context = new TestLambdaContext();
            AlexaRequest req = new AlexaRequest();
            req.Version = "ping";
            req.Session = new AlexaSessionAttributes();
            req.Session.Application = new ApplicationAttributes();
            req.Session.Application.ApplicationId = WHETSTONE_TECH_SKILL_ID;
            req.Session.SessionId = PING_SESSION_ID;

            string messageText = JsonConvert.SerializeObject(req);

            TestClientContext testContext = new TestClientContext();


            context.ClientContext = testContext;
            var function = new AlexaFunctionProxy();

            var returnVal = await function.FunctionHandlerAsync(req, context);

            var testLogger = context.Logger as TestLambdaLogger;

            // Assert.Contains("Stream processing complete", testLogger.Buffer.ToString());
        }




    }
}
