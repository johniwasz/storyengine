using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Security.Amazon;

namespace Whetstone.StoryEngine.Security
{
    public static class SecurityConfigExtensions
    {


        public static void AddCognitoAuthentication(this IServiceCollection services, CognitoConfig cognitoConfig)
        {
            services.Configure<CognitoConfig>(
                options =>
                {
                    options.UserPoolClientId = cognitoConfig.UserPoolClientId;
                    options.UserPoolClientSecret = cognitoConfig.UserPoolClientSecret;
                    options.UserPoolId = cognitoConfig.UserPoolId;
                    options.UserPoolRegion = cognitoConfig.UserPoolRegion;

                });

            services.AddAWSService<IAmazonCognitoIdentityProvider>();
            services.AddTransient<IAuthenticator, CognitoAuthenticator>();
            services.AddTransient<IJwtTokenParser, CognitoTokenParser>();


        }


        internal static bool IsConfirmed(this AdminGetUserResponse userResponse)
        {
            bool isConfirmed = false;

            if (!string.IsNullOrWhiteSpace(userResponse.UserStatus?.Value))
            {
                string statusVal = userResponse.UserStatus.Value;
                isConfirmed = statusVal.Equals("CONFIRMED");
            }

            return isConfirmed;
        }




        internal static bool IsEmailConfirmed(this AdminGetUserResponse userResponse)
        {
            bool isEmailConfirmed = false;

            var foundItem = userResponse.UserAttributes?.FirstOrDefault(x => x.Name.Equals("email_verified"));
            if (foundItem != null)
            {
                isEmailConfirmed = foundItem.Value.Equals("confirmed");


            }



            return isEmailConfirmed;
        }
    }
}
