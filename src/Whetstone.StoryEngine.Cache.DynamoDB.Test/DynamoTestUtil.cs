using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Whetstone.StoryEngine.Cache.DynamoDB;
using Whetstone.StoryEngine.Cache.Manager;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.Amazon;
using Whetstone.StoryEngine.Data.EntityFramework;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Repository.Amazon;

namespace Whetstone.StoryEngine.Cache.DynamoDB.Test
{
    internal static  class DynamoTestUtil
    {
        private static readonly RegionEndpoint CURREGION = RegionEndpoint.GetBySystemName(RegionEndpoint.USEast1.SystemName);


        internal static IStoryUserRepository GetStoryUserRepository()
        {
            IOptions<AmazonDynamoDBConfig> dbConfig = GetDynamoDBConfig();

            ILogger<WhetstoneDynamoDbClient> dbLogger = GetLogger<WhetstoneDynamoDbClient>();

            IAmazonDynamoDB dbClient = new WhetstoneDynamoDbClient(dbConfig, dbLogger);

            ILogger<DynamoDBUserRepository> cacheServiceLogger = GetLogger<DynamoDBUserRepository>();


            string userNameName = "Whetstone-DynamoDb-Dev-UserTable-V0KS2V0Z2Y4J";

            IOptions<DynamoUserTableConfig> tableConfig = Options.Create<DynamoUserTableConfig>(new DynamoUserTableConfig() { TableName = userNameName });


            IStoryUserRepository dynamoDbStoryRep = new DynamoDBUserRepository(tableConfig, dbClient, cacheServiceLogger);

            return dynamoDbStoryRep;
        }

        internal static IDistributedCache GetDynamoDbCache()
        {
            return GetDynamoDbCache(2, 2000);
        }

        internal static IDistributedCache GetDynamoDbCache(int maxRetries, int timeOut)
        {

            DistributedCacheEntryOptions distOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 30, 0)
            };

            Whetstone.StoryEngine.Cache.DistributedCacheExtensions.SetDefaultCacheOptions(distOptions, true);

            string tableName = "Whetstone-CacheTable-Dev-CacheTable-1A0X189QJZXYD";
      

            IOptions<AmazonDynamoDBConfig> dbConfig = GetDynamoDBConfig();


            ILogger<WhetstoneDynamoDbClient> dbLogger = GetLogger<WhetstoneDynamoDbClient>();

            IAmazonDynamoDB dbClient = new WhetstoneDynamoDbClient(dbConfig, dbLogger);
            //   DynamoDBOperationConfig opConfig = new DynamoDBOperationConfig() { OverrideTableName = tableName };

            //   var curTable = dynamoContext.GetTargetTable<DefaultCacheTable>(opConfig);


            ICacheTtlManager ttlManager = new CacheTtlManager();

            IOptions<DynamoDBCacheConfig> dbCacheConfig = Options.Create<DynamoDBCacheConfig>(new DynamoDBCacheConfig()
            {                   
                TableName = tableName,
                MaxRetries = maxRetries,
                Timeout = timeOut
            });


            ILogger<DynamoDBCacheService> cacheServiceLogger = GetLogger<DynamoDBCacheService>();

            IDistributedCache distCache = new DynamoDBCacheService(dbCacheConfig, dbClient, ttlManager, cacheServiceLogger);

            return distCache;

        }

        internal static IAmazonS3 GetAmazonS3Client()
        {

            AmazonS3Config s3Config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.USEast1,
                Timeout = new TimeSpan(0, 0, 0, 2, 0),
                //   dynamoConfig.HttpClientCacheSize = 10;
                ReadWriteTimeout = new TimeSpan(0, 0, 0, 2, 0),
                MaxErrorRetry = 2,
                ResignRetries = false
            };

            IAmazonS3 s3Client = new AmazonS3Client(s3Config);


            return s3Client;

        }

        private static IOptions<AmazonDynamoDBConfig> GetDynamoDBConfig()
        {

            AmazonDynamoDBConfig dynamoConfig = new AmazonDynamoDBConfig
            {
                RegionEndpoint = RegionEndpoint.USEast1,
                Timeout = new TimeSpan(0, 0, 0, 2, 0),
                //   dynamoConfig.HttpClientCacheSize = 10;
                ReadWriteTimeout = new TimeSpan(0, 0, 0, 2, 0),
                MaxErrorRetry = 2,
                ResignRetries = false
            };
            //   dynamoConfig.ThrottleRetries = false;
            //dynamoConfig.LogMetrics = true;


            IOptions<AmazonDynamoDBConfig> dbConfig = Options.Create<AmazonDynamoDBConfig>(dynamoConfig);


            return dbConfig;

        }


        internal static ILogger<T> GetLogger<T>()
        {


            var loggerFactory = LoggerFactory.Create(builder => {
                builder
                       .AddConsole()
                      .AddDebug()
                          .AddFilter(level => level >= LogLevel.Debug);

            });

            return loggerFactory.CreateLogger<T>();

        }



        internal static IMemoryCache GetInMemoryCache()
        {

            IOptions<MemoryCacheOptions> memCacheConfig = Options.Create<MemoryCacheOptions>(new MemoryCacheOptions());

            MemoryCache memCache = new MemoryCache(memCacheConfig);


            return memCache;

        }


        private static IOptions<EnvironmentConfig> GetEnvironmentConfig()
        {
            EnvironmentConfig envConfig = new EnvironmentConfig
            {
                Region = CURREGION,
                BucketName = "dev-sbsstoryengine",
                DbUserType = DbUserType.AdminUser
            };

            IOptions<EnvironmentConfig> envConfigOpts = Options.Create<EnvironmentConfig>(envConfig);


            return envConfigOpts;
        }

        private static IOptions<DatabaseConfig> GetDatabaseConfig(EnvironmentConfig envConfig)
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

        internal static IUserContextRetriever GetUserContextRetriever(DBConnectionRetreiverType connectionRetrieverType)
        {
            var distCacheDict = GetInMemoryCache();
            IOptions<EnvironmentConfig> envOpts = GetEnvironmentConfig();

            EnvironmentConfig envConfig = envOpts.Value;

            IOptions<DatabaseConfig> dbConfig = GetDatabaseConfig(envConfig);

            IUserContextRetriever userContextRetriever = null;

            ILogger<UserDataContext> dataContextLogger = GetLogger<UserDataContext>();

            switch (connectionRetrieverType)
            {
                case DBConnectionRetreiverType.IamRole:

                    var iamLogger = GetLogger<IamUserContextRetriever>();
                    userContextRetriever = new IamUserContextRetriever(envOpts, dbConfig, distCacheDict, dataContextLogger, iamLogger);
                    break;
                case DBConnectionRetreiverType.Direct:

                    var userLogger = GetLogger<DirectUserContextRetriever>();
                    userContextRetriever = new DirectUserContextRetriever(envOpts, dbConfig, distCacheDict, dataContextLogger, userLogger);
                    break;
            }


            return userContextRetriever;
        }
    }
}
