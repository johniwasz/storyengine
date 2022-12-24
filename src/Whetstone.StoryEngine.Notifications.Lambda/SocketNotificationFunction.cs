using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Notifications;
using Whetstone.StoryEngine.Notifications.Repository;
using Whetstone.StoryEngine.Notifications.Repository.Extensions;
using Whetstone.StoryEngine.WebLibrary;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Whetstone.StoryEngine.Notifications.Lambda
{
    public class SocketNotificationFunction : ApiLambdaBase
    {
        private readonly ILogger<SocketNotificationFunction> _logger;
        private readonly INotificationProcessor _notificationProcessor;

        public SocketNotificationFunction()
            : base()
        {
            _logger = Services.GetService<ILogger<SocketNotificationFunction>>();
            _notificationProcessor = Services.GetService<INotificationProcessor>();
        }

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(NotificationRequest request, ILambdaContext context)
        {
            _logger.LogDebug("Notification Lambda - begin processing");
            await _notificationProcessor.ProcessNotification(request);
            _logger.LogDebug("Notification Lambda - end processing");
        }

        protected override void ConfigureServices(IServiceCollection services, IConfiguration config, BootstrapConfig bootConfig)
        {
            base.ConfigureServices(services, config, bootConfig);
            services.EnableNotificationServices(bootConfig);
        }

    }
}
