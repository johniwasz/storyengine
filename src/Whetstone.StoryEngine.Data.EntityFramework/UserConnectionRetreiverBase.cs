using Amazon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.Data.EntityFramework
{
    public abstract class UserConnectionRetreiverBase
    {
        private readonly ILogger<UserConnectionRetreiverBase> _dataLogger;


        protected const string DATABASECACHEPREFIX = "DBCONSTRING";

        protected IMemoryCache _cache;

        protected readonly DatabaseConfig _dbConfig;

        protected readonly string _baseConnectionString;

        protected readonly RegionEndpoint _endPoint;



        protected readonly int _port;

        protected readonly string _host;

        protected readonly MemoryCacheEntryOptions _cacheExpirationOptions;

        private readonly ILogger<UserDataContext> _dataContextLogger;

        public UserConnectionRetreiverBase(IOptions<EnvironmentConfig> envConfig, IOptions<DatabaseConfig> dbConfig,
            IMemoryCache memCache, ILogger<UserDataContext> dataContextLogger, ILogger<UserConnectionRetreiverBase> logger)
        {

            _cache = memCache ?? throw new ArgumentNullException(nameof(memCache));

            _dataLogger = logger ?? throw new ArgumentNullException(nameof(logger));

            _dataContextLogger = dataContextLogger ?? throw new ArgumentNullException(nameof(dataContextLogger));

            DatabaseConfig config = dbConfig?.Value ??
                                    throw new ArgumentNullException(nameof(dbConfig));


            if (config.Settings == null)
                throw new ArgumentNullException(nameof(dbConfig), "Settings property cannot be null or empty");

            if (config.Settings.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                foreach (string settingName in config.Settings.Keys)
                {
                    string settingValue = config.Settings[settingName];
                    builder.Append(settingName);
                    builder.Append("=");
                    builder.Append(settingValue);
                    builder.Append(";");

                    if (settingName.Equals("Host", StringComparison.OrdinalIgnoreCase))
                        _host = settingValue;

                }
                _baseConnectionString = builder.ToString();
            }
            else
            {
                throw new ArgumentNullException(nameof(dbConfig), "Settings must have values");

            }

            _dbConfig = config;

            _endPoint = envConfig?.Value?.Region ??
                        throw new ArgumentNullException(nameof(envConfig), "Region setting cannot be null");




            _port = _dbConfig.Port.GetValueOrDefault(5432);


            int absCacheTimeout = _dbConfig.TokenExpirationSeconds.GetValueOrDefault(840);

            _cacheExpirationOptions = new MemoryCacheEntryOptions();
            _cacheExpirationOptions.AbsoluteExpiration = DateTime.Now.AddSeconds(absCacheTimeout);
            _cacheExpirationOptions.Priority = CacheItemPriority.Normal;


            _dataLogger.LogDebug($"Database RDS token timeout set for {absCacheTimeout} seconds");



        }



        public async Task<IUserDataContext> GetUserDataContextAsync()
        {

            var contextOptions = await GetContextOptionsAsync();


            IUserDataContext dataContext = new UserDataContext(contextOptions, _dataContextLogger);


            return dataContext;

        }

        /// <summary>
        /// Pulls the context options from cache if available. It builds them if not found in cache.
        /// </summary>
        /// <returns></returns>
        public async Task<DbContextOptions<UserDataContext>> GetContextOptionsAsync()
        {
            // Attempt to get the context options from the memory cache.

            DbContextOptions<UserDataContext> dataContext =
                _cache.Get<DbContextOptions<UserDataContext>>(DATABASECACHEPREFIX);

            if (dataContext == null)
            {

                string dbConnection = await GetConnectionStringAsync();

                DbContextOptionsBuilder<UserDataContext>
                    contextBuilder = new DbContextOptionsBuilder<UserDataContext>();

                if (_dbConfig.EnableSensitiveLogging.GetValueOrDefault(false))
                {
                    LoggerFactory logFact = new LoggerFactory();


                    logFact.AddProvider(new EFLoggerProvider());
                    contextBuilder.UseLoggerFactory(logFact);

                    contextBuilder.EnableSensitiveDataLogging();

                }


                contextBuilder.UseNpgsql(dbConnection,

                    b =>
                    {
                        b.CommandTimeout(2);
                        b.EnableRetryOnFailure(3);
                    });

                dataContext = contextBuilder.Options;

                _cache.Set(DATABASECACHEPREFIX, dataContext, _cacheExpirationOptions);

            }
            else
            {
                _dataLogger.LogTrace("Retrieved database context from cache");
            }

            return dataContext;
        }

        public abstract Task<string> GetConnectionStringAsync();

    }
}
