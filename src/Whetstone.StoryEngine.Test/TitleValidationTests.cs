using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.Caching;
using Whetstone.StoryEngine.Data.Yaml;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Story.Cards;
using Whetstone.StoryEngine.Models.Story.Text;
using Whetstone.StoryEngine.Repository;
using Xunit;

namespace Whetstone.StoryEngine.Test
{

    public class TitleValidationTests : TestServerFixture
    {



        [Fact]
        public async Task GetNodeRoute()
        {


            IFileRepository fileRep = Services.GetService<IFileRepository>();

            var memDict = GetMemoryCache();
            IMemoryCache memCache = Services.GetService<IMemoryCache>();

            ITitleCacheRepository titleCacheRep = GetTitleCacheRepository();

            ILogger<YamlTitleReader> yamlLogger = Services.GetService<ILogger<YamlTitleReader>>();

            ITitleReader titleReader = new YamlTitleReader(titleCacheRep, yamlLogger);

            string titleId = "eyeoftheeldergods";

            TitleVersion titleVer = new TitleVersion(titleId, "1.0");

            ITitleValidator validator = new TitleValidator(titleReader, fileRep);


            var nodeMap = await validator.GetNodeRouteAsync(titleVer, "A1", "D8");



        }


        [Fact]
        public async Task GetNodeMap()
        {
            IFileRepository fileRep = Services.GetService<IFileRepository>();

            ITitleCacheRepository titleCacheRep = GetTitleCacheRepository();

            ILogger<YamlTitleReader> yamlLogger = Services.GetService<ILogger<YamlTitleReader>>();

            ITitleReader titleReader = new YamlTitleReader(titleCacheRep, yamlLogger);

            string titleId = "eyeoftheeldergods";

            TitleVersion titleVer = new TitleVersion(titleId, "1.0");

            ITitleValidator validator = new TitleValidator(titleReader, fileRep);


            var nodeMap = await validator.GetNodeMapAsync(titleVer);

        }



        [Fact]
        public async Task ValidateElderGodsAsync()
        {

            IFileRepository fileRep = Services.GetService<IFileRepository>();

            ITitleCacheRepository titleCacheRep = GetTitleCacheRepository();



            ILogger<YamlTitleReader> yamlLogger = Services.GetService<ILogger<YamlTitleReader>>();

            ITitleReader titleReader = new YamlTitleReader(titleCacheRep, yamlLogger);


            TitleVersion titleVersion = new TitleVersion("eyeoftheeldergods", "1.0");

            StoryTitle title = await titleReader.GetByIdAsync(titleVersion);

            ITitleValidator validator = new TitleValidator(titleReader, fileRep);


            StoryValidationResult valResult = await validator.ValidateTitleAsync(titleVersion);

            if ((valResult.NodeIssues?.Any()).GetValueOrDefault(false))
            {

                foreach (var nodeIssue in valResult.NodeIssues)
                {
                    Debug.WriteLine(nodeIssue.NodeName);
                    if ((nodeIssue.Messages?.Any()).GetValueOrDefault())
                    {
                        foreach (string valMessage in nodeIssue.Messages)
                            Debug.WriteLine(string.Concat("   ", valMessage));

                    }
                }
            }

            if ((valResult.UnusedAudioFiles?.Any()).GetValueOrDefault(false))
            {
                Debug.WriteLine("---------");
                Debug.WriteLine("Unused Audio Files");

                foreach (string unusedFile in valResult.UnusedAudioFiles)
                {
                    Debug.WriteLine(unusedFile);
                }
            }
        }

        [Fact]
        public async Task ValidateWhetstoneTechnologiesAsync()
        {

            ITitleCacheRepository titleCacheRep = GetTitleCacheRepository();

            ILogger<YamlTitleReader> yamlLogger = Services.GetService<ILogger<YamlTitleReader>>();

            ITitleReader titleReader = new YamlTitleReader(titleCacheRep, yamlLogger);

            IFileRepository fileRep = Services.GetService<IFileRepository>();

            TitleVersion titleVersion = new TitleVersion("whetstonetechnologies", "0.3");

            StoryTitle title = await titleReader.GetByIdAsync(titleVersion);

            ITitleValidator validator = new TitleValidator(titleReader, fileRep);

            StoryNode editNode = null;

            foreach (StoryNode node in title.Nodes)
            {
                if (String.Compare(node.Name, "SonibridgeInfo", true) == 0)
                {
                    editNode = node;
                    break;
                }
            }


            CardResponse bixbyCard = new CardResponse();

            LocalizedResponse localizedResponse = editNode.ResponseSet[0].LocalizedResponses[0];
            localizedResponse.CardResponses = new List<CardResponse>();

            bixbyCard.SpeechClient = Client.Bixby;
            bixbyCard.LargeImageFile = localizedResponse.SmallImageFile;
            bixbyCard.SmallImageFile = localizedResponse.LargeImageFile;
            bixbyCard.TextFragments = new List<TextFragmentBase>();

            SimpleTextFragment fragment = new SimpleTextFragment();
            fragment.Text = "Hello weird";
            bixbyCard.TextFragments.Add(fragment);
            bixbyCard.CardTitle = localizedResponse.CardTitle;

            CardResponse defaultCard = new CardResponse();

            defaultCard.LargeImageFile = localizedResponse.SmallImageFile;
            defaultCard.SmallImageFile = localizedResponse.LargeImageFile;
            defaultCard.TextFragments = new List<TextFragmentBase>();

            fragment = new SimpleTextFragment();
            fragment.Text = "Hello weird";
            defaultCard.TextFragments.Add(fragment);
            defaultCard.CardTitle = localizedResponse.CardTitle;

            localizedResponse.CardResponses.Add(bixbyCard);
            localizedResponse.CardResponses.Add(defaultCard);

            localizedResponse.TextFragments = null;
            localizedResponse.CardTitle = null;
            localizedResponse.SmallImageFile = null;
            localizedResponse.LargeImageFile = null;

            StoryValidationResult valResult = await validator.ValidateTitleAsync(titleVersion);

            if ((valResult.NodeIssues?.Any()).GetValueOrDefault(false))
            {

                foreach (var nodeIssue in valResult.NodeIssues)
                {
                    Debug.WriteLine(nodeIssue.NodeName);
                    if ((nodeIssue.Messages?.Any()).GetValueOrDefault())
                    {
                        foreach (string valMessage in nodeIssue.Messages)
                            Debug.WriteLine(string.Concat("   ", valMessage));

                    }
                }
            }

            if ((valResult.UnusedAudioFiles?.Any()).GetValueOrDefault(false))
            {
                Debug.WriteLine("---------");
                Debug.WriteLine("Unused Audio Files");

                foreach (string unusedFile in valResult.UnusedAudioFiles)
                {

                    Debug.WriteLine(unusedFile);
                }


            }



        }

    }
}
