using System;
using Microsoft.Extensions.DependencyInjection;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Notifications.Repository.Amazon;
using Whetstone.StoryEngine.SocketApi.Repository;
using Whetstone.StoryEngine.SocketApi.Repository.Amazon;
using Whetstone.StoryEngine.SocketApi.Repository.Extensions;

namespace Whetstone.StoryEngine.Notifications.Repository.Extensions
{
    public static class NotificationExtensions
    {
        public static IServiceCollection AddNotificationProcessor(this IServiceCollection services, BootstrapConfig bootConfig, bool useLambda = true)
        {
            if ( useLambda )
            {
                services.AddSingleton<INotificationProcessor, LambdaNotificationProcessor>();
            }
            else
            {
                services.AddSingleton<INotificationProcessor, DynamoDBNotificationProcessor>();
            }

            return services;
        }

        public static IServiceCollection EnableNotificationServices(this IServiceCollection services, BootstrapConfig bootConfig)
        {
            // As long as we have Cognito authentication, add the web authorizer and the socket support objects
            if (bootConfig.Security.AuthenticatorType.GetValueOrDefault(AuthenticatorType.Cognito) == AuthenticatorType.Cognito)
            {
                services.ConfigureSocketOptions(bootConfig);

                services.AddTransient<ISocketConnectionManager, DynamoDBSocketManager>();
                services.AddTransient<ISocketMessageSender, ApiGatewayMessageSender>();

                services.AddSingleton<INotificationProcessor, DynamoDBNotificationProcessor>();
            }
            else
            {
                throw new InvalidOperationException("Socket Authentication currently requires Cognito!");
            }

            return services;
        }
    }
}
