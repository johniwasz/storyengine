using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Xunit;

namespace Whetstone.StoryEngine.Test
{
    public class TitleRetrieverTests : TestServerFixture
    {




        [Fact(DisplayName = "SMS Title Retriever Integration Test")]
        public async Task ImportStoryTitleZipYaml()
        {

            byte[] importZip = await GetResourceAsync("ImportFiles.animalfarmpi.zip");

            IAppMappingReader appMapping = Services.GetRequiredService<IAppMappingReader>();

            var titleVer = await appMapping.GetTitleAsync(Models.Client.Sms, "+12672140345", null);



        }
    }
}
