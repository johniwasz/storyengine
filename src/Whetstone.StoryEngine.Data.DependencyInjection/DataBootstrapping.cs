using Microsoft.Extensions.DependencyInjection;
using System;
using Whetstone.StoryEngine.Data.EntityFramework;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.Data.DependencyInjection
{
    public static class DataBootstrapping
    {

        public static void ConfigureDatabaseService(IServiceCollection services, DatabaseConfig dbConfig)
        {

            services.AddTransient<UserDataRepository>();
            // There's got to be a clearer way to do this.
            services.Configure<DatabaseConfig>(x =>
            {
                x.AdminUser = dbConfig.AdminUser;
                x.EngineUser = dbConfig.EngineUser;
                x.Port = dbConfig.Port;
                x.SessionLoggingUser = dbConfig.SessionLoggingUser;
                x.Settings = dbConfig.Settings;
                x.SmsUser = dbConfig.SmsUser;
                x.EnableSensitiveLogging = dbConfig.EnableSensitiveLogging;
                x.DirectConnect = dbConfig.DirectConnect;
                x.ConnectionRetrieverType =
                    dbConfig.ConnectionRetrieverType.GetValueOrDefault(DBConnectionRetreiverType.IamRole);
            });


            switch (dbConfig.ConnectionRetrieverType.GetValueOrDefault(
                DBConnectionRetreiverType.IamRole))
            {
                case DBConnectionRetreiverType.IamRole:
                    services.AddSingleton<IUserContextRetriever, IamUserContextRetriever>();
                    break;
                case DBConnectionRetreiverType.Direct:
                    services.AddSingleton<IUserContextRetriever, DirectUserContextRetriever>();
                    break;
                case DBConnectionRetreiverType.SecretsManager:
                    throw new NotImplementedException("Secrets manager retriever is not yet implemented");
            }
        }

    }
}
