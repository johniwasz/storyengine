using Amazon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;
using Xunit;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Test.Config
{

    public class EnvironmentConfigTests
    {

        //  private static Microsoft.Extensions.Logging.ILogger _logger = StoryEngineLogFactory.CreateLogger<EnvironmentConfigTests>();

        [Fact]
        public async Task LoadEnvironmentsTest()
        {
            System.Environment.SetEnvironmentVariable("BOOTSTRAP", "/storyengine/dev/bootstrap");
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.AWSDEFAULTREGION, RegionEndpoint.USEast1.SystemName);
            Bootstrapping bootStrap = new Bootstrapping();



            IConfiguration config = Bootstrapping.BuildConfiguration();

            BootstrapConfig bootConfig = config.Get<BootstrapConfig>();
            IServiceCollection services = new ServiceCollection();
            Bootstrapping.ConfigureServices(services, config, bootConfig);

            ILoggerFactory logFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });


            bootConfig.DatabaseSettings.Settings.Remove("Password");

            bootConfig.DatabaseSettings.ConnectionRetrieverType = DBConnectionRetreiverType.Direct;

            bootConfig.DatabaseSettings.DirectConnect = new DBDirectConnectConfig
            {
                UserName = "storyengineuser",
                ClientSecret = "x59k+6|3.(N~zrqFU(e?2^MvJ<J^}_V}"
            };



            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();
            _ = yamlSer.Serialize(bootConfig);

            var servProv = services.BuildServiceProvider();


            IOptions<CacheConfig> cacheConfig = servProv.GetService<IOptions<CacheConfig>>();

            ILogger<EnvironmentConfigTests> logger = logFactory.CreateLogger<EnvironmentConfigTests>();

            RedisManager.FlushRedis(cacheConfig, logger);
            // Get a user from the database.
            IStoryUserRepository storyUserRep = servProv.GetRequiredService<IStoryUserRepository>();
            IAppMappingReader appReader = servProv.GetRequiredService<IAppMappingReader>();

            // string userId = "test-user-7DFD8AC2-2A61-404E-B62C-4A355DF1D2D7";
            string clientId = "amzn1.ask.skill.c4cabd50-2cd5-4e4c-a03c-a57d4f2a0e5f";
            var clientType = Client.Alexa;

            TitleVersion titleVer = await appReader.GetTitleAsync(clientType, clientId, null);
            EngineClientContext engineContext = new EngineClientContext(titleVer, clientId, clientType, "en-US");


            StoryRequest req = engineContext.GetLaunchRequest();



        }

        [Fact]
        public void SaveCacheConfig()
        {
            string bootstrapText = File.ReadAllText($"Config/config.yml");



            Deserializer deser = new Deserializer();

            BootstrapConfig bootConfig = deser.Deserialize<BootstrapConfig>(bootstrapText);

            bootConfig.DynamoDBTables = new DynamoDBTablesConfig
            {
                UserTable = "Whetstone-DynamoDbStore-Dev-UserTable-1VS3IYWFNK5J7"
            };

            Serializer yamlSer = new Serializer();
            _ = yamlSer.Serialize(bootConfig);

        }






    }
}
