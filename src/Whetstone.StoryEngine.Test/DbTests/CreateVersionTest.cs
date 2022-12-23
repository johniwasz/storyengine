using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;
using Xunit;

namespace Whetstone.StoryEngine.Test.DbTests
{
    public class CreateVersionTest : TestServerFixture
    {



        [Fact]
        public async Task CreateSampleStory()
        {

            string text = File.ReadAllText("importfiles/eyeoftheeldergods/eyeoftheeldergods.yaml");

            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();


            StoryTitle retTitle = yamlDeserializer.Deserialize<StoryTitle>(text);

            IStoryVersionRepository storyVersionRep = this.Services.GetService<IStoryVersionRepository>();
           
             await storyVersionRep.CreateOrUpdateVersionAsync(retTitle);



        }



    }
}
