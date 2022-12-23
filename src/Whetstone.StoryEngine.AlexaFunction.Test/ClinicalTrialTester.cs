#region namespaces
using Amazon.Lambda.TestUtilities;
using Newtonsoft.Json;
using Whetstone.Alexa;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using System.Threading.Tasks;

#endregion

namespace Whetstone.StoryEngine.AlexaFunction.Test
{
    public class ClinicalTrialTester : LambdaTestBase
    {







        [Fact(DisplayName = "ClientTrial CanFulfill Intent")]
        public async Task CanFulfillIntent()
        {
            var function = new AlexaFunctionProxy();
            // string sessionId = "amzn1.echo-api.session.7a0719cc-1c31-4b2d-8f40-97ef403f3a23";

            string sessionId = "A2352F09-0EA9-43E2-8C84-4B448125419F";

            AlexaSessionContext sessionContext = new AlexaSessionContext(AlexaSessionContext.ClinicalTrialId, sessionId, "en-US", JohnWhetstoneId);


            var context = GetLambdaContext();



            Dictionary<string, string> trialSlots = new Dictionary<string, string>();

            trialSlots.Add("city", "New York");
            trialSlots.Add("condition", "epilepsy");
            //CanFulfillIntentRequest
            var fulfillResponse = await GetIntentResult(sessionContext, function, "FindTrialByCityAndConditionIntent", trialSlots, RequestType.CanFulfillIntentRequest);

            Debug.Assert(fulfillResponse == null, "fulfillResponse cannot be null");


            Debug.Assert(fulfillResponse.Response.CanFulfillIntent.Slots[0].CanFulfill == Whetstone.Alexa.CanFulfill.CanFulfillEnum.Yes, "First slot should be fulfilled");


        }

        [Fact(DisplayName = "ClientTrial Tester")]
        public async Task ClinicalTrialSearch()
        {
            TestLambdaLogger testLogger;
            string testLog;
            var function = new AlexaFunctionProxy();


            AlexaSessionContext sessionContext = new AlexaSessionContext(AlexaSessionContext.ClinicalTrialId, JohnId, "en-US", JohnId);


            var context = GetLambdaContext();

            // AlexaRequest req = sessionContext.CreateLaunchRequest();

            //   var returnVal = function.FunctionHandler(req, context);

            // Debug.WriteLine(returnVal.Response.OutputSpeech.Ssml);
            testLogger = context.Logger as TestLambdaLogger;
            testLog = testLogger.Buffer.ToString();

            Debug.WriteLine(testLog);

            Dictionary<string, string> trialSlots = new Dictionary<string, string>();

            trialSlots.Add("city", "New York");
            trialSlots.Add("condition", "epilepsy");

            var epilepsyResult = await GetIntentResult(sessionContext, function, "FindTrialByCityAndConditionIntent", trialSlots, RequestType.IntentRequest);


            Assert.False(epilepsyResult.Response.ShouldEndSession, "Session should remain open");

            //trialSlots.Clear();

            //trialSlots.Add("city", "Philadelphia");
            //trialSlots.Add("condition", "epilepsy");

            epilepsyResult = await GetIntentResult(sessionContext, function, "NextIntent", trialSlots, RequestType.IntentRequest);


            Assert.False(epilepsyResult.Response.ShouldEndSession, "Session should remain open");


            //ssmlText = SendIntent(sessionContext, function, "DarkAndStormyIntent");

            //ssmlText = SendIntent(sessionContext, function, "AMAZON.NoIntent");

            //ssmlText = SendIntent(sessionContext, function, "AMAZON.YesIntent");

        }




        [Fact(DisplayName = "End Session Test")]
        public async Task EndSessionTest()
        {
            TestLambdaLogger testLogger;
            string testLog;
            var function = new AlexaFunctionProxy();


            AlexaSessionContext sessionContext = new AlexaSessionContext(AlexaSessionContext.ClinicalTrialId, JohnId, "en-US", JohnId);


            var context = GetLambdaContext();

            AlexaRequest req = sessionContext.CreateLaunchRequest();

            var returnVal = await function.FunctionHandlerAsync(req, context);

            Debug.WriteLine(returnVal.Response.OutputSpeech.Ssml);
            testLogger = context.Logger as TestLambdaLogger;
            testLog = testLogger.Buffer.ToString();

            Debug.WriteLine(testLog);

            Dictionary<string, string> trialSlots = new Dictionary<string, string>();

            trialSlots.Add("city", "New York");
            trialSlots.Add("condition", "epilepsy");

            var intentResult = await GetIntentResult(sessionContext, function, "FindTrialByCityAndConditionIntent", trialSlots, RequestType.IntentRequest);

            Debug.Assert(intentResult.Response.ShouldEndSession == false, "Session ended prematurely");


            intentResult = await GetIntentResult(sessionContext, function, "EndGameIntent");

            // assert that the session is closed.

            Debug.Assert(intentResult.Response.ShouldEndSession == true, "Session did not end as expected");




        }
    }
}
