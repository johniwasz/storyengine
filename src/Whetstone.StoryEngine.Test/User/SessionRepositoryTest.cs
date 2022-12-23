using Microsoft.Extensions.Caching.Distributed;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.Amazon;
using Whetstone.StoryEngine.Data.Yaml;
using Whetstone.StoryEngine.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Whetstone.StoryEngine.Repository.Messaging;
using Whetstone.StoryEngine.Data.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Whetstone.StoryEngine.Test.User
{

    public class SessionRepositoryTest : TestServerFixture
    {

        [Fact(DisplayName = "Save Session In Queue")]
        public async Task SaveSessionQueueAsync()
        {

            ITitleCacheRepository titleCacheRep = GetTitleCacheRepository();

          

            ILogger<YamlTitleReader> yamlLogger = Services.GetService<ILogger<YamlTitleReader>>();

            ITitleReader titleReader = new YamlTitleReader(titleCacheRep, yamlLogger);
            string titleName = "eyeoftheeldergods";
            string titleVersion = "1.0";

            StoryRequest req = new StoryRequest();
            req.UserId = "amzn1.ask.account.AG7OJCVYEBVLAF3AOIGWE44RSSXFPTOUKCU4UXZZ3F2QFTA6TJZBO2NIAZVZEXROBRE7IU4VXYRUR2JFPKSU52MEJGLUFHWOQ6BUCYYVPMQO2XZXS6MLXYFQFTJYJAHAGKKMUBYQADEJYCXOSAO4LG4H63IKERLH7H4HLJUEXS32UI6GGR2P7GGO3GP6V63DHJTEV57W4QQIAXQ";

            req.SessionContext = new EngineSessionContext();
            req.SessionContext.TitleVersion = new Models.Story.TitleVersion();
            req.SessionContext.TitleVersion.ShortName = titleName;
            req.SessionContext.TitleVersion.Version = titleVersion;
            req.Locale = "en-US";
            req.IsNewSession = true;
            req.RequestType = StoryRequestType.Launch;
            req.SessionId = Guid.NewGuid().ToString();
            req.Client = Models.Client.Alexa;


            StoryResponse resp = new StoryResponse();
            req.SessionContext.TitleVersion = new Models.Story.TitleVersion();
            req.SessionContext.TitleVersion.ShortName = titleName;
            req.SessionContext.TitleVersion.Version = titleVersion;

            resp.NodeName = await titleReader.GetStartNodeNameAsync(req.SessionContext.TitleVersion, true);


            var dbOptions = GetUserDatabaseOptions();
            ISessionLogger sessionRep = Services.GetService<SessionQueueLogger>();


            await sessionRep.LogRequestAsync(req, resp);


            req.IsNewSession = false;
            req.RequestType = StoryRequestType.Resume;
            req.Intent = "ResumeIntent";
            //req.Slots = new Dictionary<string, string>();
            //req.Slots.Add("location", "house");
            //req.Slots.Add("verb", "search");

            resp.NodeName = "A3";

            await sessionRep.LogRequestAsync(req, resp);
        }


    }
}
