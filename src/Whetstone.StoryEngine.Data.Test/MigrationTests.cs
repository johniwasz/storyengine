using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data.Amazon;
using Whetstone.StoryEngine.Data.Caching;
using Whetstone.StoryEngine.Data.EntityFramework.EntityManager;
using Whetstone.StoryEngine.Data.Yaml;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;
using Xunit;

namespace Whetstone.StoryEngine.Data.Tests
{
    public class MigrationTests : DataTestBase
    {

        protected const string APP_MAPPING_PATH = "global/appmappings.yaml";


        [Fact]
        public async Task CreateDeploymentsFromMappings()
        {
            string env = "dev";
            //   var devCon = GetDataContext();
            var distCacheDict = GetMemoryCache();
            IOptions<EnvironmentConfig> envOpts = GetEnvironmentConfig();


            EnvironmentConfig envConfig = envOpts.Value;

            IOptions<DatabaseConfig> dbConfig = GetDatabaseConfig(envConfig);

            IUserContextRetriever userContextRetriever = GetUserContextRetriever(DBConnectionRetreiverType.Direct);


            ILogger<S3FileStore> fileStoreLogger = CreateLogger<S3FileStore>();

            IAmazonS3 s3Client = GetS3Client();

            IFileRepository fileRep = new S3FileStore(envOpts, userContextRetriever, s3Client, fileStoreLogger);


            IOptions<MemoryCacheOptions> cacheOpts = Options.Create<MemoryCacheOptions>(new MemoryCacheOptions());

            IMemoryCache memCache = new MemoryCache(cacheOpts);


            ILoggerFactory factory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole()
                  .AddDebug()
                      .AddFilter(level => level >= LogLevel.Debug);


            });


            ILogger<YamlTitleReader> yamlLogger = factory.CreateLogger<YamlTitleReader>();

            var inMemoryCache = GetInMemoryCache();


            ITitleCacheRepository titleCacheRep = GetTitleCache();
            ITitleReader titleReader = new YamlTitleReader(titleCacheRep, yamlLogger);


            DataTitleVersionRepository dataVersionRep = new DataTitleVersionRepository(userContextRetriever, titleCacheRep, fileRep);


            ITitleRepository titleRep = new TitleRepository(titleReader);


            Dictionary<string, string> appMappings = await GetAppMappingsStoreAsync(env);


            List<string> titles = new List<string>();

            foreach (string key in appMappings.Keys)
            {
                string title = appMappings[key];


                if (!titles.Contains(title))
                    titles.Add(title);
            }

            Dictionary<string, string> titleVersions = new Dictionary<string, string>();



            List<string> addedTitles = new List<string>();

            foreach (string title in titles)
            {
                bool isTitleRead = false;
                StoryTitle fullTitle = null;
                try
                {

                    TitleVersion titleVer = new TitleVersion(title, null);

                    fullTitle = await titleReader.GetByIdAsync(titleVer);
                    isTitleRead = true;
                    addedTitles.Add(title);
                }
                catch (Exception)
                {
                    Debug.WriteLine($"Title {title} could not be read");

                }

                if (isTitleRead)
                {
                    titleVersions.Add(title, fullTitle.Version);
                    await dataVersionRep.CloneVersionAsync(title, null, fullTitle.Version);
                }
            }

            IOptions<EnvironmentConfig> envConfigOpts = GetEnvironmentConfig();


            foreach (string key in appMappings.Keys)
            {
                Client curClient;

                if (key.Contains("amzn1.ask.skill"))
                    curClient = Client.Alexa;
                else if (key.Contains("+1"))
                    curClient = Client.Sms;
                else
                    curClient = Client.GoogleHome;


                string mappedTitle = appMappings[key];

                if (titleVersions.ContainsKey(mappedTitle))
                {
                    string titleVersion = titleVersions[mappedTitle];

                    PublishVersionRequest publishRequest = new PublishVersionRequest()
                    {
                        TitleName = mappedTitle,
                        Version = titleVersion,
                        ClientType = curClient,
                        ClientId = key
                    };

                    await dataVersionRep.PublishVersionAsync(publishRequest);
                }

            }
        }



        private async Task<Dictionary<string, string>> GetAppMappingsStoreAsync(string environment)
        {



            string container = string.Concat(environment, "-sbsengine");
            RegionEndpoint endpoint = RegionEndpoint.USEast1;

            string fileContents = await GetConfigTextContentsAsync(endpoint, container, APP_MAPPING_PATH);

            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();

            Dictionary<string, string> titleDictionary = yamlDeserializer.Deserialize<Dictionary<string, string>>(fileContents);

            return titleDictionary;
        }


        internal static async Task<string> GetConfigTextContentsAsync(RegionEndpoint endpoint, string containerName, string path)
        {
            string configContents = null;
            try
            {
                using (IAmazonS3 client = new AmazonS3Client(endpoint))
                {

                    GetObjectRequest request = new GetObjectRequest
                    {
                        BucketName = containerName,
                        Key = path
                    };

                    using (GetObjectResponse response = await client.GetObjectAsync(request))
                    {
                        using (BufferedStream buffer = new BufferedStream(response.ResponseStream))
                        {
                            using (StreamReader reader = new StreamReader(buffer))
                            {
                                configContents = reader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (AmazonS3Exception s3Ex)
            {
                throw new Exception($"Error retrieving text file {path} from bucket {containerName}", s3Ex);

            }
            catch (AmazonServiceException servEx)
            {
                throw new Exception($"Error retrieving text file {path} from bucket {containerName}", servEx);

            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving text file {path} from bucket {containerName}", ex);
            }
            return configContents;
        }


    }
}
