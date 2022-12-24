using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Notifications.Repository;
using Whetstone.StoryEngine.SocketApi.Repository;

namespace Whetstone.StoryEngine.CoreApi.WebSockets
{
    public static class WebSocketExtensions
    {

        public static IApplicationBuilder EnableDebugWebSockets(this IApplicationBuilder app)
        {
            app.UseWebSockets();

            return app.UseMiddleware<WebSocketManagerMiddleware>(app.ApplicationServices);
        }

        public static IServiceCollection EnableDebugWebSockets(this IServiceCollection services, BootstrapConfig bootConfig)
        {
            services.AddTransient<IWebAuthorizer, WebAuthorizer>();
            services.AddSingleton<ISocketConnectionManager, SocketConnectionManager>();
            services.AddSingleton<ISocketHandler, SocketHandler>();
            services.AddTransient<ISocketMessageSender, WebSocketMessageSender>();

            return services;
        }

        public static IServiceCollection EnableDebugNotificationProcessor(this IServiceCollection services, BootstrapConfig bootConfig)
        {
            services.AddSingleton<INotificationProcessor, DebugNotificationProcessor>();

            return services;
        }

    }
}
