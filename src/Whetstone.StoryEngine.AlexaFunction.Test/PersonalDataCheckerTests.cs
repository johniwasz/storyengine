using System.Threading.Tasks;
using Whetstone.Alexa;
using Xunit;

namespace Whetstone.StoryEngine.AlexaFunction.Test
{
    public class PersonalDataCheckerTests : LambdaTestBase
    {

        internal const string skillId = "BAC0A41E-3A8F-4E69-AC82-6F707B1AD18A";
        internal const string userId = "amzn1.ask.account.34A8FF7E-C867-4414-89D6-5037D7D020A7";
        internal const string sessionId = "amzn1.echo-api.session.23E63E68-B4C4-422F-A57F-0AFC82850618";

        [Fact(DisplayName = "Personal Checker Launch Tester")]
        public async Task LaunchPersonalChecker()
        {
            var function = new AlexaFunctionProxy();

            AlexaSessionContext sessionContext = new AlexaSessionContext(skillId, sessionId, "en-US", userId);

            AlexaResponse resp = await GetLaunchResult(sessionContext, function);

            WriteResponse(resp);

            resp = await GetIntentResult(sessionContext, function, "Resume");


        }


    }
}
