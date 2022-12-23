using Amazon.Lambda.TestUtilities;
using Whetstone.Alexa;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Whetstone.StoryEngine;
namespace Whetstone.StoryEngine.AlexaFunction.Test
{
    public class RevenoxTest : LambdaTestBase
    {
        internal const string revenoxTestId = "amzn1.ask.skill.04027bc3-0e7b-48ca-98d0-5d41ab24fc7c";
        internal const string userId = "amzn1.ask.account.AEZ6KLE3AV5KKLCBBSGYYYKHTB7E4KM7MMQQPGAMPEZQBIYKMME44356YMPOE675JPNBUOF7PDOKZUSVMK2OR7FTNZZYSOADOB2GWW2D2HIAV7XV5KVZSNZFHFCWSQTIS3Y55XDNXSTJB3VIJRG7ZMKPDA2KM5VABKR7HL2QG5ZZOADH657JSL3UBCB2XV5AHLOLC6VMOQA7HPI";
        internal const string sessionId = "amzn1.echo-api.session.f8236ba9-9ddd-4a09-be41-6f2d8706a369";

        [Fact(DisplayName = "Revenox Test Run Tester")]
        public async Task RunRevenoxTest()
        {

            var function = new AlexaFunctionProxy();


            AlexaSessionContext sessionContext = new AlexaSessionContext(revenoxTestId, sessionId, "en-US", userId);


            var context = GetLambdaContext();

            AlexaResponse resp = await GetLaunchResult(sessionContext, function);
            WriteResponse(resp);

            // Are you 18 or older
            resp = await GetIntentResult(sessionContext, function, "YesIntent");
            WriteResponse(resp);

            // Are you on commercial health insurance
            resp = await GetIntentResult(sessionContext, function, "YesIntent");
            WriteResponse(resp);

            // Please provide your phone number
            Dictionary<string, string> phoneNumberSlot = new Dictionary<string, string>();
            phoneNumberSlot.Add("phonenumber", "6105551212");

            resp = await GetIntentResult(sessionContext, function, "PhoneNumberIntent", phoneNumberSlot);
            WriteResponse(resp);

            // Consent to send a text
            resp = await GetIntentResult(sessionContext, function, "YesIntent");
            WriteResponse(resp);

        }


    }
}
