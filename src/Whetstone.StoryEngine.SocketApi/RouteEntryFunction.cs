using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Notifications.Repository;
using Whetstone.StoryEngine.Notifications.Repository.Extensions;
using Whetstone.StoryEngine.SocketApi.Repository;
using Whetstone.StoryEngine.SocketApi.Repository.Extensions;
using Whetstone.StoryEngine.WebLibrary;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Whetstone.StoryEngine.SocketApi
{
    public class RouteEntryFunction : ApiLambdaBase
    {
        public const string AUTHTYPECONFIG = "AUTHTYPE";
        public const string AUTHPOOLID = "AUTHPOOLID";
        public const string AUTHCLIENTID = "AUTHCLIENTID";

        private readonly ILogger<RouteEntryFunction> _logger;

        private readonly SocketHandlers _socketHandlers;

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public RouteEntryFunction() :
            base()
        {
            _logger = Services.GetService<ILogger<RouteEntryFunction>>();

            var socketHandler = Services.GetService<ISocketHandler>();
            IOptions<SocketConfig> socketConfig = Services.GetService<IOptions<SocketConfig>>();

            INotificationProcessor notificationProcessor = Services.GetService<INotificationProcessor>();

            _logger.LogDebug($"ConnectionTableName: {socketConfig.Value.SocketConnectionTableName}");
            _logger.LogDebug($"SocketWriteEndpoint: {socketConfig.Value.SocketWriteEndpoint}");
            _logger.LogDebug($"PendingNotificationsTableName: {socketConfig.Value.PendingNotificationsTableName}");
            _logger.LogDebug($"NotificationsLambdaArn: {socketConfig.Value.NotificationsLambdaArn}");

            _socketHandlers = new SocketHandlers(_logger, socketHandler, socketConfig.Value, notificationProcessor);
        }

        public async Task<APIGatewayProxyResponse> RouteEntryHandlerAync(APIGatewayProxyRequest request, ILambdaContext context)
        {

            _logger.LogDebug($"SocketHandler RouteKey: {request.RequestContext.RouteKey}");

            APIGatewayProxyResponse resp = null;
            switch (request.RequestContext.RouteKey)
            {
                case "$connect": resp = await _socketHandlers.OnConnectHandler(request, context); break;
                case "$disconnect": resp = await _socketHandlers.OnDisconnectHandler(request, context); break;
                case "sendMessage": resp = await _socketHandlers.SendMessageHandler(request, context); break;
                default:
                    context.Logger.LogLine($"Unrecognized Route: {request.RequestContext.RouteKey}");
                    throw new InvalidOperationException($"Unrecognized Route: {request.RequestContext.RouteKey}");
            }

            return resp;
        }

        protected override void ConfigureServices(IServiceCollection services, IConfiguration config, BootstrapConfig bootConfig)
        {
            // Setup the base services first!
            base.ConfigureServices(services, config, bootConfig);

            // Configure authenticated sockets now
            services.EnableAuthenticatedWebSockets(bootConfig);

            // Setup our notification processor
            services.AddNotificationProcessor(bootConfig);
        }

    }
}