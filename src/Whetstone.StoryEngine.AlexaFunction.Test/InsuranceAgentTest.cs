using Amazon.Lambda.TestUtilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Whetstone.Alexa;
using Xunit;

namespace Whetstone.StoryEngine.AlexaFunction.Test
{
    public class InsuranceAgentTest : LambdaTestBase
    {
        internal const string userId = "amzn1.ask.account.ZZ3DZ3NJLES3Z5EM7IN3WO3ZH7LXFUGNZNVHHEDTD3YKRZMVPTKIQ6P3HR4WUHYI73XB4SG7E7AYQCXZUXOTIZRGZBLGL4R23HKINZTTWRH6VROUOFR4PLTLIT4HHJX34SNRPF3FNIMJSMFMRDC4N2GFRZNGRFGCWC2JHJMKDYHW5WORECJOHII7W4DDQOHNCJ3UXPTJKZQAOZQ";
        internal const string sessionId = "amzn1.echo-api.session.2E40F880-F20D-480E-A3ED-5606C75642F4";



        [Fact(DisplayName = "Elder Gods Walkthrough")]
        public async Task ElderGodsWalkthrough()
        {
            var function = new AlexaFunctionProxy();


            AlexaSessionContext sessionContext = new AlexaSessionContext(InsuranceAgentDevSkillId);

            sessionContext.UserId = userId;

            Debug.WriteLine(sessionContext.SessionId);


            var context = GetLambdaContext();

            AlexaResponse resp = await GetLaunchResult(sessionContext, function);
            WriteResponse(resp);


            resp = await GetIntentResult(sessionContext, function, "AddNewDriverIntent");
            WriteResponse(resp);

            // Misspell Sanj
            Dictionary<string, string> firstName = new Dictionary<string, string>();
            firstName.Add("letter", "SANJEV");
            resp = await GetIntentResult(sessionContext, function, "NameIntent", firstName);
            WriteResponse(resp);


            resp = await GetIntentResult(sessionContext, function, "NoIntent");
            WriteResponse(resp);

            
            resp = await GetIntentResult(sessionContext, function, "AMAZON.FallbackIntent", firstName);
            WriteResponse(resp);



        }
    }
}
