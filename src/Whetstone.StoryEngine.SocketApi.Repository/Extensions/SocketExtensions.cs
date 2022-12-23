using System;
using Microsoft.Extensions.DependencyInjection;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.SocketApi.Repository.Amazon;

namespace Whetstone.StoryEngine.SocketApi.Repository.Extensions
{
    public static class SocketExtensions
    {
        public static IServiceCollection ConfigureSocketOptions(this IServiceCollection services, BootstrapConfig bootConfig)
        {
            services.Configure<SocketConfig>(x =>
            {
                x.SocketConnectionTableName = bootConfig.SocketConfig.SocketConnectionTableName;
                x.SocketWriteEndpoint = bootConfig.SocketConfig.SocketWriteEndpoint;
                x.PendingNotificationsTableName = bootConfig.SocketConfig.PendingNotificationsTableName;
                x.NotificationsLambdaArn = bootConfig.SocketConfig.NotificationsLambdaArn;
                x.NotificationsLambdaName = bootConfig.SocketConfig.NotificationsLambdaName;
            });

            return services;
        }

        public static IServiceCollection EnableAuthenticatedWebSockets(this IServiceCollection services, BootstrapConfig bootConfig)
        {
            // As long as we have Cognito authentication, add the web authorizer and the socket support objects
            if (bootConfig.Security.AuthenticatorType.GetValueOrDefault(AuthenticatorType.Cognito) == AuthenticatorType.Cognito)
            {
                services.ConfigureSocketOptions(bootConfig);

                services.AddTransient<IWebAuthorizer, WebAuthorizer>();
                services.AddTransient<ISocketConnectionManager, DynamoDBSocketManager>();
                services.AddTransient<ISocketHandler, SocketHandler>();
                services.AddTransient<ISocketMessageSender, ApiGatewayMessageSender>();
            }
            else
            {
                throw new InvalidOperationException("Socket Authentication currently requires Cognito!");
            }

            return services;
        }

    }
}
