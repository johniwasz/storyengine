using Amazon;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.Repository.Amazon
{
    public interface IStepFunctionSender
    {
        Task SendMessageToStepFunctionAsync<T>(string stepFunctionName, T message);
    }


    public class StepFunctionSender : IStepFunctionSender
    {
        private readonly ILogger<StepFunctionSender> _logger;
        private RegionEndpoint _endpoint;


        public StepFunctionSender(IOptions<EnvironmentConfig> envConfig, ILogger<StepFunctionSender> logger)
        {
            _endpoint = envConfig.Value?.Region ?? throw new ArgumentNullException(nameof(envConfig), "Region cannot be null");

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));


        }



        public async Task SendMessageToStepFunctionAsync<T>(string stepFunctionName, T message)
        {
            StartExecutionRequest executionRequest = new StartExecutionRequest();
            executionRequest.StateMachineArn = stepFunctionName;
            executionRequest.Name = Guid.NewGuid().ToString();
            executionRequest.Input = JsonConvert.SerializeObject(message);

            try
            {
                using (AmazonStepFunctionsClient stepFunctionClient = new AmazonStepFunctionsClient(_endpoint))
                {

                    StartExecutionResponse sendResponse = await stepFunctionClient.StartExecutionAsync(executionRequest);

                    _logger.LogInformation($"Scheduled Sms StepFunction with name {executionRequest.Name} to StepFunction {stepFunctionName} and received https status code {sendResponse.HttpStatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error sending ms StepFunction with name {executionRequest.Name} to StepFunction {stepFunctionName}",
                                      ex);
            }
        }

    }
}
