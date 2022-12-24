using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.Caching;
using Whetstone.StoryEngine.Data.Yaml;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Story.Ssml;
using Xunit;

namespace Whetstone.StoryEngine.Test
{

    public class StreamingMediaTest : TestServerFixture
    {


        private async Task<StoryTitle> GetTitleWithVoice()
        {
            ITitleCacheRepository titleCacheRep = Services.GetService<TitleCacheRepository>();

            ILogger<YamlTitleReader> yamlLogger = Services.GetService<ILogger<YamlTitleReader>>();

            ITitleReader titleReader = new YamlTitleReader(titleCacheRep, yamlLogger);

            TitleVersion titleVersion = new TitleVersion("animalfarmtest", "1.0");


            StoryTitle newTitle = await titleReader.GetByIdAsync(titleVersion);

            StoryNode wallowMud = newTitle.Nodes.FirstOrDefault(x => x.Name.Equals("WallowInMud"));


            var responseSet = wallowMud.ResponseSet.FirstOrDefault();

            var locResponses = responseSet.LocalizedResponses[0];

            var clientResponses = locResponses.SpeechResponses;


            var clientResponse = clientResponses[0];

            var speechResponse = (PlainTextSpeechFragment)clientResponse.SpeechFragments[2];

            speechResponse.Voice = "Hans";


            return newTitle;
        }








        [Fact(DisplayName = "Generate Keys")]
        public void GenerateKeys()
        {

            byte[] iv = GetRandomData(128);
            byte[] keyAes = GetRandomData(256);


            string vector = Convert.ToBase64String(iv);

            string key = Convert.ToBase64String(keyAes);

        }


        private byte[] GetRandomData(int bits)
        {
            var result = new byte[bits / 8];
            RandomNumberGenerator.Create().GetBytes(result);
            return result;
        }

    }
}
