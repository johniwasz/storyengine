using Amazon;
using Amazon.DynamoDBv2;
using Amazon.S3;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Strategies;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.Amazon;
using Whetstone.StoryEngine.Data.Caching;
using Whetstone.StoryEngine.Data.DependencyInjection;
using Whetstone.StoryEngine.Data.EntityFramework;
using Whetstone.StoryEngine.Data.EntityFramework.EntityManager;
using Whetstone.StoryEngine.Data.Yaml;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Story.Cards;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.Repository.Amazon;
using Whetstone.StoryEngine.Test.DbTests;

namespace Whetstone.StoryEngine.Test
{

    public class TestServerFixture : EntityContextTestBase, IDisposable
    {

        internal const string TESTCRED01 = "dev/testcred01";

        private TestServer testServer;


        //protected IOptions<MemoryDistributedCacheOptions> _memCacheOptions =
        //    Options.Create(new MemoryDistributedCacheOptions());

        protected IOptions<EnvironmentConfig> _envOptions;
        protected IDistributedCache _cache;

        //ILogger Logger { get; }
        //    = ApplicationLogging.CreateLogger<TestServerFixture>();

        public IConfiguration Configuration { get; set; }

        public IServiceProvider Services { get; set; }

        public IServiceCollection ServiceCollection { get; set; }

        public TestServerFixture()
        {
            AWSXRayRecorder.Instance.ContextMissingStrategy = ContextMissingStrategy.LOG_ERROR;

            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.AWSDEFAULTREGION,
                RegionEndpoint.USEast1.SystemName);
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.BOOTSTRAP, "/storyengine/dev/bootstrap");
            //System.Environment.SetEnvironmentVariable(ContainerSettingsReader., "AdminUser");
            //System.Environment.SetEnvironmentVariable(ContainerSettingsReader.AWSDEFAULTREGION, RegionEndpoint.USWest2.SystemName);
            //System.Environment.SetEnvironmentVariable(ContainerSettingsReader.BOOTSTRAP, "/storyengine/Prod/bootstrap");
            System.Environment.SetEnvironmentVariable(Bootstrapping.DBUSERTYPE, "AdminUser");

            //AdminUser = 0,
            //StoryEngineUser = 1,
            //SessionLoggingUser = 2,
            //SmsUser = 3

            ServiceCollection = new ServiceCollection();

            Configuration = Bootstrapping.BuildConfiguration();

            BootstrapConfig bootConfig = Configuration.Get<BootstrapConfig>();

            System.Environment.SetEnvironmentVariable(ClientLambdaBase.STORYBUCKETCONFIG, bootConfig.Bucket);

            System.Environment.SetEnvironmentVariable(ClientLambdaBase.MESSAGESTEPFUNCTIONCONFIG, bootConfig.SmsConfig.NotificationStepFunctionArn);

            System.Environment.SetEnvironmentVariable(ClientLambdaBase.CACHETABLECONFIG, bootConfig.CacheConfig.DynamoDBTableName);

            System.Environment.SetEnvironmentVariable(ClientLambdaBase.USERTABLECONFIG, bootConfig.DynamoDBTables.UserTable);

            System.Environment.SetEnvironmentVariable(ClientLambdaBase.SESSIONAUDITURLCONFIG, bootConfig.SessionAuditQueue);

            System.Environment.SetEnvironmentVariable(ClientLambdaBase.LOGLEVELCONFIG, bootConfig.LogLevel.GetValueOrDefault(LogLevel.Debug).ToString());

            Bootstrapping.ConfigureServices(ServiceCollection, Configuration, bootConfig);
            DataBootstrapping.ConfigureDatabaseService(this.ServiceCollection, bootConfig.DatabaseSettings);

            this.ServiceCollection.AddTransient<UserDataRepository>();

            ServiceCollection.AddSingleton<Func<UserRepositoryType, IStoryUserRepository>>(serviceProvider => handlerKey =>
            {
                IStoryUserRepository retUserRep = null;
                switch (handlerKey)
                {
                    case UserRepositoryType.Database:
                        retUserRep = serviceProvider.GetService<UserDataRepository>();

                        break;
                    case UserRepositoryType.DynamoDB:
                        retUserRep = serviceProvider.GetService<DynamoDBUserRepository>();
                        break;
                    default:
                        throw new KeyNotFoundException(); // or maybe return null, up to you
                }

                return retUserRep;
            });

            ServiceCollection.AddTransient<IStoryVersionRepository, DataTitleVersionRepository>();

            ServiceCollection.AddTransient<IFileReader, S3FileReader>();

            ServiceCollection.AddTransient<IFileRepository, S3FileStore>();

            ServiceCollection.AddTransient<IProjectRepository, ProjectRepository>();

            ServiceCollection.AddTransient<IStoryVersionRepository, DataTitleVersionRepository>();

            ServiceCollection.AddTransient<IOrganizationRepository, OrganizationRepository>();

            ServiceCollection.Configure<AmazonDynamoDBConfig>(x =>
            {
                x.RegionEndpoint = RegionEndpoint.USEast1;
                x.ReadWriteTimeout = new TimeSpan(0, 0, 0, 0, Bootstrapping.DEFAULT_CACHE_ENDPOINT_TIMEOUT);
                x.Timeout = new TimeSpan(0, 0, 0, 0, Bootstrapping.DEFAULT_CACHE_ENDPOINT_TIMEOUT);
                x.MaxErrorRetry = Bootstrapping.DEFAULT_CACHE_ENDPOINT_RETRIES;
            });

            // Apply the same Dynamo throttling settings to S3 storage retrieval.
            ServiceCollection.Configure<AmazonS3Config>(x =>
            {
                x.RegionEndpoint = RegionEndpoint.USEast1;
                x.ReadWriteTimeout = new TimeSpan(0, 0, 0, 0, Bootstrapping.DEFAULT_CACHE_ENDPOINT_TIMEOUT);
                x.Timeout = new TimeSpan(0, 0, 0, 0, Bootstrapping.DEFAULT_CACHE_ENDPOINT_TIMEOUT);
                x.MaxErrorRetry = Bootstrapping.DEFAULT_CACHE_ENDPOINT_RETRIES;
            });

            ServiceCollection.AddSingleton<IAmazonDynamoDB, WhetstoneDynamoDbClient>();

            ServiceCollection.AddSingleton<IAmazonS3, WhetstoneS3Client>();

            Services = ServiceCollection.BuildServiceProvider();
        }


        protected DbContextOptions<UserDataContext> GetUserDatabaseOptions()
        {

            DbContextOptionsBuilder<UserDataContext> builder = new DbContextOptionsBuilder<UserDataContext>();

            string dbCon = System.Environment.GetEnvironmentVariable("DATABASECONNECTION");


            builder.UseNpgsql(dbCon, b => { b.EnableRetryOnFailure(); });


            return builder.Options;


        }

        protected S3FileStore GetBlobRepository()
        {
            S3FileStore blobRep = Services.GetService<S3FileStore>();
            return blobRep;
        }

        protected SetCookieHeaderValue Cookie { get; set; }

        protected IUserContextRetriever GetLocalUserContext()
        {

            ILogger<UserDataContext> contextLogger = this.Services.GetService<ILogger<UserDataContext>>();


            EnvironmentConfig envConfig = new EnvironmentConfig(RegionEndpoint.USEast1, "notneeded");

            IOptions<EnvironmentConfig> envOpts = Options.Create<EnvironmentConfig>(envConfig);

            DatabaseConfig dbConfig = new DatabaseConfig
            {
                AdminUser = "postgres",
                EngineUser = "postgres",
                Port = 5433,
                SessionLoggingUser = "postgres",
                Settings = new Dictionary<string, string>()
            };
            dbConfig.Settings.Add("Password", "xxxxxxxx");
            dbConfig.Settings.Add("Host", "127.0.0.1");
            dbConfig.Settings.Add("Database", "postgres");
            dbConfig.SmsUser = "postgres";
            dbConfig.EnableSensitiveLogging = true;
            dbConfig.DirectConnect = new DBDirectConnectConfig();
            dbConfig.DirectConnect.UserName = "postgres";
            dbConfig.DirectConnect.ClientSecret = "xxxxxxxx";


            dbConfig.ConnectionRetrieverType =
                    dbConfig.ConnectionRetrieverType.GetValueOrDefault(DBConnectionRetreiverType.Direct);

            IOptions<DatabaseConfig> dbOptions = Options.Create<DatabaseConfig>(dbConfig);


            ILogger<DirectUserContextRetriever> directLogger =
                this.Services.GetService<ILogger<DirectUserContextRetriever>>();

            var distCache = this.Services.GetService<IMemoryCache>();

            IUserContextRetriever directRetriever = new DirectUserContextRetriever(envOpts, dbOptions, distCache, contextLogger, directLogger);


            return directRetriever;

        }


        protected JsonSerializerSettings GetJsonSettings()
        {

            JsonSerializerSettings serSettings = new JsonSerializerSettings();


            serSettings.Converters.Add(new StringEnumConverter
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            });


            // options.SerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
            serSettings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
            //    options.SerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Objects;

            serSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            // options.SerializerSettings.ReferenceResolverProvider = () => new ReferenceResolverWithUuid();

            serSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            serSettings.ObjectCreationHandling = ObjectCreationHandling.Auto;
            serSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            return serSettings;
        }


        protected ISessionStoreManager GetSessionStoreManager()
        {

            var storeManager = Services.GetService<ISessionStoreManager>();

            return storeManager;
        }

        protected async Task<StoryTitle> GetTitleAsync(TitleVersion titleVer)
        {

            ITitleCacheRepository titleCacheRep = GetTitleCacheRepository();

            ILogger<YamlTitleReader> yamlLogger = Services.GetService<ILogger<YamlTitleReader>>();

            ITitleReader titleReader = new YamlTitleReader(titleCacheRep, yamlLogger);

            StoryTitle retTitle = await titleReader.GetByIdAsync(titleVer);

            return retTitle;

        }


        protected IDistributedCache GetMemoryCache()
        {
            MemoryDistributedCacheOptions memOptions = new MemoryDistributedCacheOptions();

            IOptions<MemoryDistributedCacheOptions> memSettings = Options.Create<MemoryDistributedCacheOptions>(memOptions);

            IDistributedCache distCache = new MemoryDistributedCache(memSettings);

            DistributedCacheEntryOptions distCacheEntryOptions = new DistributedCacheEntryOptions();
            distCacheEntryOptions.SetSlidingExpiration(new TimeSpan(0, 5, 0));


            Whetstone.StoryEngine.Cache.DistributedCacheExtensions.SetDefaultCacheOptions(distCacheEntryOptions, true);

            return distCache;

        }

        protected async Task WriteResponseAsync(StoryRequest req, StoryResponse resp, Client clientType)
        {

            IMediaLinker mediaLinker = Services.GetRequiredService<IMediaLinker>();



            Func<UserRepositoryType, IStoryUserRepository> userStoryFunc = Services.GetService<Func<UserRepositoryType, IStoryUserRepository>>();

            IStoryUserRepository userRep = userStoryFunc(UserRepositoryType.DynamoDB);

            DataTitleClientUser curUser = await userRep.GetUserAsync(req);

            Debug.WriteLine($"Current node: {curUser.CurrentNodeName}");
            Debug.WriteLine($"StoryNode node: {curUser.StoryNodeName}");
            Debug.WriteLine($"Client type: {clientType}");

            var sessionLogger = Services.GetService<ISessionLogger>();
            await sessionLogger.LogRequestAsync(req, resp);

            if (resp == null)
                throw new ArgumentException("response is null");

            if (resp.LocalizedResponse == null)
                throw new ArgumentException("StoryResponse has an empty response. This is an invalid conditionBase.");

            var localizedResponse = resp.LocalizedResponse;

            // Get an alexa response and if it does not exist, then get the default client response
            var speechResponse = localizedResponse.SpeechResponses;

            string generatedText = localizedResponse.GeneratedTextResponse.CleanText();

            TitleVersion titleVer = new TitleVersion(resp.TitleId, resp.TitleVersion);

            if (speechResponse != null)
            {

                string ssmlResponse = speechResponse?.ToSsml(mediaLinker, titleVer);

                Debug.WriteLine("SSML Response: ");
                Debug.WriteLine(ssmlResponse);
            }
            else if (!string.IsNullOrWhiteSpace(generatedText))
            {

                Debug.WriteLine("Text Response: ");
                Debug.WriteLine(generatedText);
            }




            var repromptResponse =
                localizedResponse.RepromptSpeechResponses;

            if (repromptResponse == null)
                repromptResponse = localizedResponse.RepromptSpeechResponses;

            if (repromptResponse != null)
            {
                string ssml =
                     repromptResponse?.ToSsml(mediaLinker, titleVer);

                Debug.WriteLine("SSML Reprompt: ");
                Debug.WriteLine(ssml);

            }
            else
            {
                Debug.WriteLine("Text Reprompt: ");
                Debug.WriteLine(localizedResponse.RepromptTextResponses);

            }

            if (!string.IsNullOrWhiteSpace(generatedText) && localizedResponse.CardResponse != null)
            {
                CardEngineResponse cardResp = localizedResponse.CardResponse;

                string cardTitle = string.IsNullOrWhiteSpace(cardResp.CardTitle) ? "Default title" : cardResp.CardTitle;

                Debug.WriteLine($"Card Title: {cardTitle}");

                if (!string.IsNullOrWhiteSpace(cardResp.SmallImageFile) ||
                     !string.IsNullOrWhiteSpace(cardResp.LargeImageFile))


                    Debug.WriteLine($"Card Text: {generatedText}");

                if (cardResp.Text != null)
                {
                    string cardText = string.Join(' ', cardResp.Text);

                    Debug.WriteLine(cardText);

                }

                if (!string.IsNullOrWhiteSpace(cardResp.SmallImageFile))
                {
                    string smallUrl = mediaLinker.GetFileLink(titleVer, cardResp.SmallImageFile);

                    Debug.WriteLine($"Card small image: {smallUrl}");
                }


                if (!string.IsNullOrWhiteSpace(cardResp.LargeImageFile))
                {
                    string largeUrl = mediaLinker.GetFileLink(titleVer, cardResp.LargeImageFile);
                    Debug.WriteLine($"Card large image: {largeUrl}");
                }
            }


            if (resp.ForceContinueSession)
            {
                Debug.WriteLine("Session Continues");
            }
            else
                Debug.WriteLine("Session Ends");

            Debug.WriteLine("------------------------------------------");

        }

        protected StoryTitle GetFileTitle(string title)
        {

            string text = File.ReadAllText($"importfiles/{title}/{title}.yaml");

            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();


            StoryTitle retTitle = yamlDeserializer.Deserialize<StoryTitle>(text);

            return retTitle;
        }

        public void Dispose()
        {
            if (testServer != null)
            {
                testServer.Dispose();
                testServer = null;
            }
        }

        public async Task<byte[]> GetResourceAsync(string resourceName)
        {

            var assembly = Assembly.GetAssembly(typeof(TestServerFixture));


            string[] asmNames = assembly.FullName.Split(',');

            string asmName = asmNames[0].Trim();

            // This shows the available items.
            string[] resources = assembly.GetManifestResourceNames();

            var stream = assembly.GetManifestResourceStream($"{asmName}.{resourceName}");

            byte[] zipBytes = null;
            using (MemoryStream memStream = new MemoryStream())
            {
                await stream.CopyToAsync(memStream);

                zipBytes = memStream.ToArray();
            }

            return zipBytes;

        }


        internal async Task<T> GetSecretAsync<T>(string secretStore) where T : class
        {
            IMemoryCache memCache = Services.GetService<IMemoryCache>();
            T retCreds = null;

            if (memCache.TryGetValue(secretStore, out object value))
            {
                retCreds = value as T;

            }
            else
            {
                ISecretStoreReader secretReader = Services.GetService<ISecretStoreReader>();

                string credValues = await secretReader.GetValueAsync(secretStore);

                retCreds = JsonConvert.DeserializeObject<T>(credValues);

                memCache.Set(secretStore, retCreds);

            }

            return retCreds;
        }

        internal ITitleCacheRepository GetTitleCacheRepository()
        {
            IFileRepository fileRep = Services.GetService<S3FileStore>();

            IMemoryCache memCache = Services.GetService<IMemoryCache>();

            // ILogger<YamlTitleReader> yamlLogger = Services.GetService<ILogger<YamlTitleReader>>();

            var distCache = GetMemoryCache();

            ILogger<TitleCacheRepository> titleLogger = Services.GetService<ILogger<TitleCacheRepository>>();

            ITitleCacheRepository titleCacheRep = new TitleCacheRepository(fileRep, distCache, memCache, titleLogger);

            return titleCacheRep;
        }




        protected IOptions<CognitoConfig> GetCognitoConfig()
        {
            CognitoConfig clientOpts = new CognitoConfig
            {
                UserPoolClientId = "4o29k16f10ir3o7job8mmiabad",
                UserPoolClientSecret = "u2umpl74d54vmf5jglhun1jg7r31ktuehvtlnl80o03m2kvk4nq",
                UserPoolId = "us-east-1_xu0geY2cC"
            };
            IOptions<CognitoConfig> cogOpts = Options.Create<CognitoConfig>(clientOpts);

            return cogOpts;

        }

    }
}
