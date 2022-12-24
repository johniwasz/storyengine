using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.RDS.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Crypto.Tls;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.Data.EntityFramework
{


    public class IamUserContextRetriever : UserConnectionRetreiverBase, IUserContextRetriever
    {

        private readonly Microsoft.Extensions.Logging.ILogger<IamUserContextRetriever> _dataLogger;

        protected readonly DbUserType _userType;

        public IamUserContextRetriever(IOptions<EnvironmentConfig> envConfig, IOptions<DatabaseConfig> dbConfig, IMemoryCache memCache, ILogger<UserDataContext> contextLogger, ILogger<IamUserContextRetriever> logger) : base(envConfig, dbConfig, memCache, contextLogger, logger)
        {
            _userType = (envConfig?.Value?.DbUserType).GetValueOrDefault(DbUserType.StoryEngineUser);

            _dataLogger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");
        }



        /// <summary>
        /// Dynamically add the user name and AWS RDS authentication token to the password.
        /// </summary>
        /// <returns>A connection string designed to work with the AWS RDS role based authentication.
        /// It is based on the approach described here for MySQL: https://aws.amazon.com/premiumsupport/knowledge-center/users-connect-rds-iam/
        /// 
        /// </returns>
        public override async Task<string> GetConnectionStringAsync()
        {
            return await Task.Run(() =>
            {
                StringBuilder connectionBuilder = new StringBuilder();

                string userName = GetUserName();

                connectionBuilder.Append(_baseConnectionString);
                connectionBuilder.Append("UserName=");
                connectionBuilder.Append(userName);
                connectionBuilder.Append(";Password=");
                string token = GetSecureToken(userName);
                connectionBuilder.Append(token);
                connectionBuilder.Append(";");


                string connectionString = connectionBuilder.ToString();

                return connectionString;
            });
        }

        private string GetUserName()
        {

            string userName = null;

            switch (_userType)
            {
                case DbUserType.AdminUser:
                    userName = _dbConfig.AdminUser;
                    break;
                case DbUserType.SessionLoggingUser:
                    userName = _dbConfig.SessionLoggingUser;
                    break;
                case DbUserType.StoryEngineUser:
                    userName = _dbConfig.EngineUser;
                    break;
                case DbUserType.SmsUser:
                    userName = _dbConfig.SmsUser;
                    break;
            }

            _dataLogger.LogDebug($"Generating new RDS token for user type {_userType} mapped to db user {userName}");

            return userName;
        }


        /// <summary>
        /// The secure token returned by AWS is good for 15 minutes.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        private string GetSecureToken(string userName)
        {
            string sessionToken;


            sessionToken = RDSAuthTokenGenerator.GenerateAuthToken(_endPoint,
                _host, _port, userName);


            return sessionToken;
        }




    }
}
