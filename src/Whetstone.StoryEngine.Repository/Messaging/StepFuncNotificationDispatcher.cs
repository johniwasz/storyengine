using Amazon;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Messaging;

namespace Whetstone.StoryEngine.Repository.Messaging
{
    public class StepFuncNotificationDispatcher : INotificationDispatcher
    {
        private readonly ILogger<StepFuncNotificationDispatcher> _logger;


        private RegionEndpoint _endpoint;
        private StepFunctionNotificationConfig _functionConfig;
        public StepFuncNotificationDispatcher(IOptions<EnvironmentConfig> envConfig, IOptions<StepFunctionNotificationConfig> functionConfig, ILogger<StepFuncNotificationDispatcher> logger)
        {
            if (envConfig == null)
                throw new ArgumentNullException($"{nameof(envConfig)}");


            _functionConfig = functionConfig?.Value ?? throw new ArgumentNullException($"{nameof(functionConfig)}");

            EnvironmentConfig envSettings =  envConfig.Value ?? throw new ArgumentNullException($"{nameof(envConfig)}", "Value cannot have null settings");


            _endpoint = envSettings.Region ?? throw new ArgumentNullException($"{nameof(envConfig)}", "cannot have a null Region");

            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");

        }


        public async Task DispatchNotificationAsync(INotificationRequest notificationRequest)
        {

            if (notificationRequest == null)
                throw new ArgumentNullException($"{nameof(notificationRequest)} cannot be null");


            string resourceName = _functionConfig.ResourceName;

            if (string.IsNullOrWhiteSpace(resourceName))
                throw new Exception($"ResourceName is null or blank in configuration");


            // All this does is send the notification message to the step function. That's it.

            StartExecutionRequest executionRequest = new StartExecutionRequest
            {
                StateMachineArn = resourceName,
                Name = notificationRequest.Id.GetValueOrDefault(default(Guid)).ToString(),

                Input = JsonConvert.SerializeObject(notificationRequest)
            };

            try
            {
                using (AmazonStepFunctionsClient stepFunctionClient = new AmazonStepFunctionsClient(_endpoint))
                {

                    StartExecutionResponse sendResponse = await stepFunctionClient.StartExecutionAsync(executionRequest);

                    _logger.LogInformation($"Sent NotificationRequest with name {executionRequest.Name} to StepFunction {resourceName} and received https status code {sendResponse.HttpStatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error sending Notification request with name {executionRequest.Name} to StepFunction {resourceName}",
                                      ex);
            }


        }
    }
}
