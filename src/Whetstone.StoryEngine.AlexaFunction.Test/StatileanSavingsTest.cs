using System.Collections.Generic;
using System.Threading.Tasks;
using Whetstone.Alexa;
using Xunit;

namespace Whetstone.StoryEngine.AlexaFunction.Test
{
    public class StatileanSavingsTest : LambdaTestBase
    {

        internal const string statileanTestId = "amzn1.ask.skill.8088c5fa-d916-4f05-a672-64c7ea39be41";
        internal const string userId = "amzn1.ask.account.AE3BCPF2ITMNJ7JQAHBQLRY3XH6JUN6NDMSTGDJL7YSQKISFA3BF3BLQWRFNJ74Y276UJYMFJNO7F2IYGLYAJIWQ5TJR3N7MILB7SSYE2DMV5YW33OIQ76M7MRQTRWMLPZPWD55JY4CA6IQLULY2GQDY2G552B4MF2DLLB2EYD44QCLCEHFHEXQFQDNCZCJFX45VMDALVDML6FQ";
        internal const string sessionId = "amzn1.echo-api.session.f8236ba9-9ddd-4a09-be41-6f2d8706a369";

        [Fact(DisplayName = "Statilean Savings Test")]
        public async Task RunStatileanTest()
        {

            var function = new AlexaFunctionProxy();


            AlexaSessionContext sessionContext = new AlexaSessionContext(statileanTestId, sessionId, "en-US", userId);


            var context = GetLambdaContext();

            AlexaResponse resp = await GetLaunchResult(sessionContext, function);
            WriteResponse(resp);

            // Are you 18 or older
            resp = await GetIntentResult(sessionContext, function, "YesIntent");
            WriteResponse(resp);

            // Are you on commercial insurance
            resp = await GetIntentResult(sessionContext, function, "YesIntent");
            WriteResponse(resp);

            Dictionary<string, string> phoneSlots = new Dictionary<string, string>();

            phoneSlots.Add("phonenumber", "2675551212");

            resp = await GetIntentResult(sessionContext, function, "PhoneNumberIntent", phoneSlots);
            WriteResponse(resp);

            resp = await GetIntentResult(sessionContext, function, "YesIntent");
            WriteResponse(resp);

        }
    }
}
