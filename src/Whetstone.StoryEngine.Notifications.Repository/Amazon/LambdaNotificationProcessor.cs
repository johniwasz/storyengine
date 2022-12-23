using System;
using System.Threading.Tasks;
using Amazon.Lambda;
using Amazon.Lambda.Model;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Notifications;

namespace Whetstone.StoryEngine.Notifications.Repository.Amazon
{
    public class LambdaNotificationProcessor : INotificationProcessor
    {
        private readonly ILogger<LambdaNotificationProcessor> _logger;
        private readonly string NotificationsLambdaName;
        public LambdaNotificationProcessor(ILogger<LambdaNotificationProcessor> logger, IOptions<SocketConfig> socketOptions)
        {
            _logger = logger;
            NotificationsLambdaName = socketOptions.Value.NotificationsLambdaName;

            if (String.IsNullOrEmpty(NotificationsLambdaName))
                throw new ArgumentNullException("LambdaNotificationProcessor NotificationsLambdaName cannot be empty");
        }

        public async Task ProcessNotification(NotificationRequest request)
        {
            _logger.LogDebug($"Invoking Notification Lambda: {NotificationsLambdaName}");
            _logger.LogDebug($"RequestType: {request.RequestType}, IsRegisteredUser: {request.UserId}, ClientId: {request.ClientId}, ConnectionId: {request.ConnectionId}");

            // Using an event invocation type causes an async notification of the lambda
            AmazonLambdaClient lambdaClient = new AmazonLambdaClient();

            // Calling in this fashion will execute the lambda using the LIVE alias.
            InvokeRequest lambdaRequest = new InvokeRequest
            {
                FunctionName = NotificationsLambdaName,
                Payload = JsonConvert.SerializeObject(request),
                InvocationType = InvocationType.Event,
#pragma warning disable Lambda1002 // Property value does not match required pattern
                Qualifier = "LIVE"
#pragma warning restore Lambda1002 // Property value does not match required pattern
            };

            InvokeResponse response = await lambdaClient.InvokeAsync(lambdaRequest);

            _logger.LogDebug($"Notification Lambda: {NotificationsLambdaName} returned status {response.StatusCode}");

        }
    }
}
