using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Whetstone.StoryEngine.Cache.Manager;
using Whetstone.StoryEngine.Cache.Models;
using Whetstone.StoryEngine.Cache.Settings;

namespace Whetstone.StoryEngine.Cache.DynamoDB
{
    public static class RegistrationExtensions
    {

        public static void RegisterDynamoDbCacheService(this IServiceCollection services, string tableName, int maxRetries, int timeout)
        {

            RegisterDynamoDbCacheService(services, tableName, maxRetries, timeout, ServiceLifetime.Transient);

        }



        public static void RegisterDynamoDbCacheService(this IServiceCollection services, string tableName, int maxRetries, int timeout,  ServiceLifetime lifeTime) 
        {
             

            services.Configure<DynamoDBCacheConfig>(x =>
            {
                x.TableName = tableName;
                x.MaxRetries = maxRetries;
                x.Timeout = timeout;

            });


            services.AddSingleton<ICacheTtlManager, CacheTtlManager>();

            services.AddTransient<IDistributedCache, DynamoDBCacheService>();

            
        }
    }




}
