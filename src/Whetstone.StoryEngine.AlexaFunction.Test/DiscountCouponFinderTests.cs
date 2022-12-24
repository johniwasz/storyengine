#region namespaces
using Amazon.Lambda.TestUtilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Whetstone.Alexa;
using Xunit;
#endregion

namespace Whetstone.StoryEngine.AlexaFunction.Test
{
    public class DiscountCouponFinderTests : LambdaTestBase
    {


        [Fact]
        public async Task FindDiscountCoupons()
        {


            TestLambdaLogger testLogger;
            string testLog;
            var function = new AlexaFunctionProxy();


            AlexaSessionContext sessionContext = new AlexaSessionContext(AlexaSessionContext.DiscountCouponFinderId, JohnSessionId, "en-US", JohnTestId);


            var context = GetLambdaContext();

            AlexaRequest req = sessionContext.CreateLaunchRequest();

            var returnVal = await function.FunctionHandlerAsync(req, context);

            Debug.WriteLine(returnVal.Response.OutputSpeech.Ssml);
            testLogger = context.Logger as TestLambdaLogger;
            testLog = testLogger.Buffer.ToString();

            Debug.WriteLine(testLog);


            Dictionary<string, string> drugSlots = new Dictionary<string, string>();

            drugSlots.Add("drug", "Stateline");


            var intentResult = await GetIntentResult(sessionContext, function, "FindDiscountCouponForDrug", drugSlots, RequestType.IntentRequest);




            intentResult = await GetIntentResult(sessionContext, function, "AMAZON.YesIntent", RequestType.IntentRequest);

            //string ssmlText = SendIntent(sessionContext, function, "FindTrialByCityAndConditionIntent", trialSlots, RequestType.IntentRequest);

            intentResult = await GetIntentResult(sessionContext, function, "AMAZON.YesIntent", RequestType.IntentRequest);


            Dictionary<string, string> phoneNumberSlots = new Dictionary<string, string>();

            phoneNumberSlots.Add("phonenumber", "2675551212");

            // phoneNumberSlots.Add("phonenumber", "6504352188");


            intentResult = await GetIntentResult(sessionContext, function, "PhoneNumberIntent", phoneNumberSlots, RequestType.IntentRequest);


            intentResult = await GetIntentResult(sessionContext, function, "AMAZON.NoIntent", RequestType.IntentRequest);

            intentResult = await GetIntentResult(sessionContext, function, "PhoneNumberIntent", phoneNumberSlots, RequestType.IntentRequest);


            intentResult = await GetIntentResult(sessionContext, function, "AMAZON.NoIntent", RequestType.IntentRequest);

            intentResult = await GetIntentResult(sessionContext, function, "PhoneNumberIntent", phoneNumberSlots, RequestType.IntentRequest);

            intentResult = await GetIntentResult(sessionContext, function, "AMAZON.YesIntent", RequestType.IntentRequest);


            Debug.Assert(intentResult.Response.ShouldEndSession == true, "Session not ended as expected");

            //ssmlText = SendIntent(sessionContext, function, "AMAZON.FallbackIntent");


        }





    }
}
