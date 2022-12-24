using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;
using Whetstone.StoryEngine.Data.Amazon;

namespace Whetstone.StoryEngine.Data
{
    public enum UserRepositoryType
    {
        DynamoDB = 1,
        Database = 2
    }


    public static class RegistrationExtensions
    {

        public static void RegisterDynamoDbUserRepository(this IServiceCollection services, string userTableName)
        {


            services.Configure<DynamoUserTableConfig>(x =>
            {

                x.TableName = userTableName;
            });


            services.AddTransient<DynamoDBUserRepository>();

        }

        public static Guid? GetUserSid(this ClaimsPrincipal prin)
        {
            var userClaim = prin.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.Sid));

            if (userClaim == null)
                throw new ArgumentException($"Claim {ClaimTypes.Sid} not found", nameof(prin));

            Guid? userSid = null;


            if (Guid.TryParse(userClaim.Value, out Guid result))
            {
                userSid = result;
            }

            return userSid;
        }
    }
}
