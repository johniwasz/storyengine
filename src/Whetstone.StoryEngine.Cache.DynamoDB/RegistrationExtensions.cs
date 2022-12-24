using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Whetstone.StoryEngine.Cache.Manager;

namespace Whetstone.StoryEngine.Cache.DynamoDB
{
    public static class RegistrationExtensions
    {

        public static void RegisterDynamoDbCacheService(this IServiceCollection services, string tableName, int maxRetries, int timeout)
        {

            RegisterDynamoDbCacheService(services, tableName, maxRetries, timeout, ServiceLifetime.Transient);

        }



        public static void RegisterDynamoDbCacheService(this IServiceCollection services, string tableName, int maxRetries, int timeout, ServiceLifetime lifeTime)
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
