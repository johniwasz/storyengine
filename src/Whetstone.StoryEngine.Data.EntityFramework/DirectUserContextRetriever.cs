using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.Data.EntityFramework
{
    public class DirectUserContextRetriever : UserConnectionRetreiverBase, IUserContextRetriever
    {
        private readonly Microsoft.Extensions.Logging.ILogger<DirectUserContextRetriever> _dataLogger;


        private readonly DBDirectConnectConfig _directConnectConfig;

        public DirectUserContextRetriever(IOptions<EnvironmentConfig> envConfig,
            IOptions<DatabaseConfig> dbConfig,
            IMemoryCache memCache,
            ILogger<UserDataContext> contextLogger,
            ILogger<DirectUserContextRetriever> logger) : base(envConfig, dbConfig, memCache, contextLogger, logger)
        {

            // Check for the direct connect settings
            _directConnectConfig = dbConfig?.Value?.DirectConnect ??
                                                   throw new ArgumentNullException(
                                                       $"{nameof(dbConfig)} cannot have a null DirectConnect setting");


            if (string.IsNullOrWhiteSpace(_directConnectConfig.ClientSecret))
                throw new ArgumentNullException(
                    $"{nameof(dbConfig)} DirectConnect.ClientSecret password cannot be null or empty");

            _dataLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        }



        public override async Task<string> GetConnectionStringAsync()
        {

            return await Task.Run(() =>
            {
                StringBuilder connectionBuilder = new StringBuilder();


                connectionBuilder.Append(_baseConnectionString);
                connectionBuilder.Append("UserName=");
                connectionBuilder.Append(_directConnectConfig.UserName);
                connectionBuilder.Append(";Password=");
                connectionBuilder.Append(_directConnectConfig.ClientSecret);
                connectionBuilder.Append(";");


                string connectionString = connectionBuilder.ToString();

                return connectionString;
            });
        }


    }
}
