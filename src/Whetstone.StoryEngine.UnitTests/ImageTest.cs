
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Repository;
using Xunit;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Repository.Amazon;

namespace Whetstone.UnitTests
{

    public class ImageTest 
    {

        [Theory()]
        [InlineData( "launch_sm.jpg", "image")]
        [InlineData("restart.mp3", "audio")]
        [InlineData( "troll_lg.jpg", "image")]
        public void  GetMediaLink(string fileName, string expectedSubFolder) 
        {

            MockFactory mockFac = new MockFactory();
            

   
            TitleVersion titleVer = TitleVersionUtil.GetAnimalFarmPITitle();

            var servCol =  mockFac.InitServiceCollection(titleVer);

            var servProv = servCol.BuildServiceProvider();

            var envConfig = servProv.GetService<IOptions<EnvironmentConfig>>();

            string configBucket = envConfig.Value.BucketName;

            IMediaLinker mediaLinker = new S3MediaLinker(envConfig);


            string fileLink = mediaLinker.GetFileLink( titleVer, fileName);

            Assert.True(fileLink.Equals($"https://{configBucket}.s3.amazonaws.com/stories/{titleVer.ShortName}/{titleVer.Version}/{expectedSubFolder}/{fileName}"),
                $"Unexpected media link URL: {fileLink}");


        }




    }
}
