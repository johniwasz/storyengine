using System;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.Alexa;
using Whetstone.StoryEngine.AlexaProcessor;
using Whetstone.StoryEngine.Models.Story;
using Xunit;

namespace Whetstone.UnitTests
{
    public class AlexaIntentExporterTests
    {

        [Fact]
        public async Task GetAlexaIntentsAsync()
        {

            IAlexaIntentExporter exporter = new AlexaIntentExporter();


            TitleVersion titleVer = TitleVersionUtil.GetAnimalFarmPITitle();


            StoryTitle title = MockFactory.LoadStoryTitle(titleVer);

            InteractionModel intentModel = await exporter.GetIntentModelAsync(title, "en-US");


            Assert.True(intentModel.LanguageModel != null, "LanguageModel is null");

            var langModel = intentModel.LanguageModel;


            var intents = langModel.Intents;

            Assert.True(intents != null, "Intents are null");

            var gotoLocationIntent = intents.FirstOrDefault(x => x.Name.Equals("GotoLocationIntent", StringComparison.OrdinalIgnoreCase));

            Assert.True(gotoLocationIntent != null, "GotoLocationIntent not found");

        }


    }
}
