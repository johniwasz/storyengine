using Amazon;
using Amazon.RDS.Util;
using Amazon.S3;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using Whetstone.StoryEngine.Data.Amazon;
using Whetstone.StoryEngine.Data.Caching;
using Whetstone.StoryEngine.Data.EntityFramework;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Serialization;

namespace Whetstone.StoryEngine.Data.Tests
{
    public class DataTestBase
    {

        protected readonly RegionEndpoint CURREGION = RegionEndpoint.GetBySystemName(RegionEndpoint.USEast1.SystemName);


        protected static readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
            .AddDebug();
        });


        protected string GetDevConnectionString()
        {

            string dbConString = null;

            string rdsToken = RDSAuthTokenGenerator.GenerateAuthToken(RegionEndpoint.USEast1,
                "devsbsstoryengine.c1z3wkpsmw56.us-east-1.rds.amazonaws.com", 5432, "lambda_proxy");

            // System.Environment.SetEnvironmentVariable("PGPASSWORD", rdsToken);

            dbConString =
                $"Host=devsbsstoryengine.c1z3wkpsmw56.us-east-1.rds.amazonaws.com;Database=devsbsstoryengine;Username=lambda_proxy;Password={rdsToken};SSL Mode=Require;Trust Server Certificate=true;Application Name=VoiceConnectr_Dev;";

            return dbConString;
        }

        protected IOptions<EnvironmentConfig> GetEnvironmentConfig()
        {
            EnvironmentConfig envConfig = new EnvironmentConfig();

            envConfig.Region = CURREGION;
            envConfig.BucketName = "dev-sbsstoryengine";
            envConfig.DbUserType = DbUserType.AdminUser;

            IOptions<EnvironmentConfig> envConfigOpts = Options.Create<EnvironmentConfig>(envConfig);


            return envConfigOpts;
        }

        protected IOptions<DatabaseConfig> GetDatabaseConfig(EnvironmentConfig envConfig)
        {
            GetParameterResponse getResp = null;
            using (var ssmClient = new AmazonSimpleSystemsManagementClient(envConfig.Region))
            {
                getResp = ssmClient.GetParameterAsync(new GetParameterRequest
                {
                    Name = "/storyengine/dev/bootstrap",
                    WithDecryption = true
                }).Result;
            }


            string paramValue = getResp.Parameter.Value;

            var yamlDeser = YamlSerializationBuilder.GetYamlDeserializer();

            BootstrapConfig bootConfig = yamlDeser.Deserialize<BootstrapConfig>(paramValue);

            DatabaseConfig dbConfig = bootConfig.DatabaseSettings;

            return Options.Create(dbConfig);
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


        protected IAmazonS3 GetS3Client()
        {
            ILogger<WhetstoneS3Client> s3Logger = CreateLogger<WhetstoneS3Client>();
            AmazonS3Config s3Config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.USEast1,
                MaxErrorRetry = 3,
                Timeout = new TimeSpan(0, 0, 0, 2),
                ReadWriteTimeout = new TimeSpan(0, 0, 0, 2)

            };

            IOptions<AmazonS3Config> s3Options = Options.Create<AmazonS3Config>(s3Config);

            IAmazonS3 s3Client = new WhetstoneS3Client(s3Options, s3Logger);


            return s3Client;
        }

        protected IMemoryCache GetInMemoryCache()
        {
            MemoryCacheOptions memOpts = new MemoryCacheOptions();

            IOptions<MemoryCacheOptions> memSettings = Options.Create(memOpts);

            IMemoryCache memCache = new MemoryCache(memSettings);

            return memCache;
        }





        protected IUserContextRetriever GetUserContextRetriever(DBConnectionRetreiverType connectionRetrieverType)
        {
            var distCacheDict = GetInMemoryCache();
            IOptions<EnvironmentConfig> envOpts = GetEnvironmentConfig();

            EnvironmentConfig envConfig = envOpts.Value;

            IOptions<DatabaseConfig> dbConfig = GetDatabaseConfig(envConfig);

            IUserContextRetriever userContextRetriever = null;

            ILogger<UserDataContext> dataContextLogger = _loggerFactory.CreateLogger<UserDataContext>();

            switch (connectionRetrieverType)
            {
                case DBConnectionRetreiverType.IamRole:

                    var iamLogger = _loggerFactory.CreateLogger<IamUserContextRetriever>();
                    userContextRetriever = new IamUserContextRetriever(envOpts, dbConfig, distCacheDict, dataContextLogger, iamLogger);
                    break;
                case DBConnectionRetreiverType.Direct:

                    var userLogger = _loggerFactory.CreateLogger<DirectUserContextRetriever>();
                    userContextRetriever = new DirectUserContextRetriever(envOpts, dbConfig, distCacheDict, dataContextLogger, userLogger);
                    break;
            }


            return userContextRetriever;
        }


        protected ITitleCacheRepository GetTitleCache()
        {


            var distCacheDict = GetMemoryCache();
            var inMemoryCache = GetInMemoryCache();

            ILogger<TitleCacheRepository> titleCacheLogger = _loggerFactory.CreateLogger<TitleCacheRepository>();

            ILogger<S3FileStore> s3Logger = _loggerFactory.CreateLogger<S3FileStore>();

            EnvironmentConfig envConfig = new EnvironmentConfig();
            envConfig.Region = RegionEndpoint.USEast1;

            envConfig.BucketName = "whetstonebucket-dev-s3bucket-1nridm382p5vm";

            IOptions<EnvironmentConfig> envOpt = Options.Create<EnvironmentConfig>(envConfig);
            IAmazonS3 s3Client = GetS3Client();

            IUserContextRetriever contRet = GetUserContextRetriever(DBConnectionRetreiverType.Direct);

            IFileRepository fileRep = new S3FileStore(envOpt, contRet, s3Client, s3Logger);


            return new TitleCacheRepository(fileRep, distCacheDict, inMemoryCache, titleCacheLogger);
        }


        protected ILogger<T> CreateLogger<T>()
        {

            return _loggerFactory.CreateLogger<T>();
        }
    }
}
