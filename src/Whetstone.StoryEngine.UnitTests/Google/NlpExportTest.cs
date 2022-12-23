using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Google.Management;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.UnitTests;
using Xunit;

namespace Whetstone.StoryEngine.UnitTests.Google
{
    public class NlpExportTest
    {
    

        [Fact]
        public async Task ExportWhetstoneNlp()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            Microsoft.Extensions.Logging.ILogger logger = loggerFactory.CreateLogger("WhetstoneExportTest");

            TitleVersion titleVer = TitleVersionUtil.GetWhetstoneTitle();


            var mocker = new MockFactory();
            IServiceCollection servCol = mocker.InitServiceCollection(titleVer);
            servCol.AddTransient<IDialogFlowManager, DialogFlowManager>();

            IServiceProvider servProv = servCol.BuildServiceProvider();

            IDialogFlowManager manager = servProv.GetRequiredService<IDialogFlowManager>();

            ITitleReader reader = servProv.GetRequiredService<ITitleReader>();


            StoryTitle title = await reader.GetByIdAsync(titleVer);

            byte[] zipBytes = await manager.ExportTitleNlpAsync(title, null);

            using (var memStream = new MemoryStream(zipBytes))
            {
                using (ZipArchive archive = new ZipArchive(memStream, ZipArchiveMode.Read))
                {

                    ValidateIntent(archive, "BeginIntent");
                    ValidateIntent(archive, "CancelIntent");
                    ValidateIntent(archive, "ContactWhetstone");
                    ValidateIntent(archive, "EndGameIntent");
                    ValidateIntent(archive, "GetContactInfo");
                    ValidateIntent(archive, "HearMoreIntent");
                    ValidateIntent(archive, "HearOurStory");
                    ValidateIntent(archive, "HelpIntent");
                    ValidateIntent(archive, "LearnAboutWhetstone");
                    ValidateIntent(archive, "LearnMoreIntent");
                    ValidateIntent(archive, "NoIntent");
                    ValidateIntent(archive, "PauseIntent");
                    ValidateIntent(archive, "PhoneNumberIntent");
                    ValidateIntent(archive, "RepeatIntent");
                    ValidateIntent(archive, "RestartIntent");
                    ValidateIntent(archive, "ResumeIntent");
                    ValidateIntent(archive, "SeeADemoIntent");
                    ValidateIntent(archive, "StopIntent");
                    ValidateIntent(archive, "YesIntent");

                    ValidateEntity(archive, "WhetstoneNames");

                }
            }
        }

        [Fact]
        public async Task ExportAnimalFarmNlp()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            Microsoft.Extensions.Logging.ILogger logger = loggerFactory.CreateLogger("AnimalFarmExportTest");

            TitleVersion titleVer = TitleVersionUtil.GetAnimalFarmPITitle();


            var mocker = new MockFactory();
            IServiceCollection servCol = mocker.InitServiceCollection(titleVer);
            servCol.AddTransient<IDialogFlowManager, DialogFlowManager>();

            IServiceProvider servProv = servCol.BuildServiceProvider();

            IDialogFlowManager manager = servProv.GetRequiredService<IDialogFlowManager>();

            ITitleReader reader = servProv.GetRequiredService<ITitleReader>();


            StoryTitle title = await reader.GetByIdAsync(titleVer);


            byte[] zipBytes = await manager.ExportTitleNlpAsync(title, null);

            using (var memStream = new MemoryStream(zipBytes))
            {
                using (ZipArchive archive = new ZipArchive(memStream, ZipArchiveMode.Read))
                {

                    ValidateIntent(archive, "BeginIntent");
                    ValidateIntent(archive, "BrightAndChipperIntent");
                    ValidateIntent(archive, "CancelIntent");
                    ValidateIntent(archive, "DarkAndStormyIntent");
                    ValidateIntent(archive, "EndGameIntent");
                    ValidateIntent(archive, "GotoLocationIntent");
                    ValidateIntent(archive, "HelpIntent");
                    ValidateIntent(archive, "MakeSandwichIntent");
                    ValidateIntent(archive, "NoIntent");
                    ValidateIntent(archive, "PauseIntent");
                    ValidateIntent(archive, "RepeatIntent");
                    ValidateIntent(archive, "RestartIntent");
                    ValidateIntent(archive, "ResumeIntent");
                    ValidateIntent(archive, "StopIntent");
                    ValidateIntent(archive, "TakeItemIntent");
                    ValidateIntent(archive, "TalkToIntent");
                    ValidateIntent(archive, "TurnOffItemIntent");
                    ValidateIntent(archive, "VerbTheCharacterIntent");
                    ValidateIntent(archive, "VerbTheItemAtTheCharacterIntent");
                    ValidateIntent(archive, "VerbTheItemIntent");
                    ValidateIntent(archive, "VerbTheItemToTheCharacterIntent");
                    ValidateIntent(archive, "WaitForIntent");
                    ValidateIntent(archive, "WallowIntent");
                    ValidateIntent(archive, "YesIntent");

                    ValidateEntity(archive, "FarmCharacters");
                    ValidateEntity(archive, "FarmItems");
                    ValidateEntity(archive, "FarmLocations");
                    ValidateEntity(archive, "Verbs");

                }
            }
        }

        private void ValidateIntent( ZipArchive archive, string intentName )
        {
            const string _intentsFolderName = "intents/";

            var intentEntry = archive.Entries.FirstOrDefault(x => x.Name.Equals($"{intentName}.json"));
            var utterancesEntry = archive.Entries.FirstOrDefault(x => x.Name.Equals($"{intentName}_usersays_en.json"));
            Assert.True(intentEntry != null, $"{intentName} intent archive not found");
            Assert.True(intentEntry.FullName.StartsWith(_intentsFolderName), $"{intentName} not in intents folder");
            Assert.True(utterancesEntry != null, $"{intentName} utterances archive not found");
            Assert.True(utterancesEntry.FullName.StartsWith(_intentsFolderName), $"{intentName} utterances not in intents folder");
        }

        private void ValidateEntity(ZipArchive archive, string entityName)
        {
            const string _entitiesFolderName = "entities/";

            var entitiesEntry = archive.Entries.FirstOrDefault(x => x.Name.Equals($"{entityName}.json"));
            var valuesEntry = archive.Entries.FirstOrDefault(x => x.Name.Equals($"{entityName}_entries_en.json"));
            Assert.True(entitiesEntry != null, $"{entityName} entity archive not found");
            Assert.True(entitiesEntry.FullName.StartsWith(_entitiesFolderName), $"{entityName} not in entities folder");
            Assert.True(valuesEntry != null, $"{entityName} entries archive not found");
            Assert.True(valuesEntry.FullName.StartsWith(_entitiesFolderName), $"{entityName} entries not in entities folder");
        }
    }
}
