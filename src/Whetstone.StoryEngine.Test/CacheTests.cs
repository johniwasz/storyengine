using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Story;
using Xunit;

namespace Whetstone.StoryEngine.Test
{
    public class CacheTests : TestServerFixture
    {

        /// <summary>
        /// This tests an update to a title version deployment
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task TitleVersionDeploymentTest()
        {
            IStoryVersionRepository storyVerRep = this.Services.GetService<IStoryVersionRepository>();
            IAppMappingReader appMappingReader = this.Services.GetService<IAppMappingReader>();


            string animalFarmPiSkillId = "amzn1.ask.skill.92304d4d-42a5-4371-9b13-97b4a79b9ad0";


            PublishVersionRequest publishRequest = new PublishVersionRequest()
            {
                TitleName = "animalfarmpi",
                Version = "1.2",
                ClientType = Client.Alexa,
                ClientId = animalFarmPiSkillId

            };

            //  await storyVerRep.PublishVersionAsync(publishRequest);

            TitleVersion foundVersion = await appMappingReader.GetTitleAsync(Client.Alexa, animalFarmPiSkillId, null);


        }

        //[Fact]
        //public async Task DeleteTitleVersionTestAsync()
        //{
        //    IStoryVersionRepository storyVerRep = this.Services.GetService<IStoryVersionRepository>();

        //    string titleName = "animalfarmpi";
        //    string version = "1.2";

        //    await storyVerRep.DeleteVersionAsync(titleName, version);
        //}


        [Fact]
        public void UpdateTitleVersion()
        {
            IStoryVersionRepository storyVerRep = this.Services.GetService<IStoryVersionRepository>();



        }





    }
}
