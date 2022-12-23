using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Notifications;
using Whetstone.StoryEngine.SocketApi.Repository;

using Whetstone.StoryEngine.Notifications.Repository.Amazon;

namespace Whetstone.StoryEngine.Notifications.Repository
{
    public class DebugNotificationProcessor : INotificationProcessor
    {
        private readonly INotificationProcessor _internalProcessor;
        private readonly ILogger<DynamoDBNotificationProcessor> _logger;

        public DebugNotificationProcessor(ILogger<DynamoDBNotificationProcessor> logger, IAmazonDynamoDB ddbClient, IOptions<SocketConfig> socketConfig, ISocketConnectionManager connMgr, IServiceProvider services)
        {
            _logger = logger;
            _internalProcessor = new DynamoDBNotificationProcessor(logger, ddbClient, socketConfig, connMgr, services);
        }

        // To simulate an actual async handoff, this class will spin up a thread that will call an internal method that does the actual processing.
        public Task ProcessNotification(NotificationRequest request)
        {
            Thread newThread = new Thread(this.DoWork);
            newThread.Start(request);
            return Task.CompletedTask;
        }

        public void DoWork(object request)
        {
            _ = this.InternalProcessNotification(request as NotificationRequest);
        }

        public async Task InternalProcessNotification(NotificationRequest request)
        {
            // This pretends that it does something like handing off to an actual lambda
            int threadSleep = 1000;

            _logger.LogInformation($"Simulating internal notification processing. Sleeping for {threadSleep}");

            System.Threading.Thread.Sleep(threadSleep);
          
            await _internalProcessor.ProcessNotification(request);
        }

    }
}
